using ChatSystem.Core;
using Moq;
using NUnit.Framework;
using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace ChatSystem.Tests
{
    [TestFixture]
    public class ChatManagerTests
    {
        private Mock<IChatNetwork> _mockNetwork;
        private ChatManager _manager;

        [SetUp]
        public void SetUp()
        {
            _mockNetwork = new Mock<IChatNetwork>();
            _mockNetwork.Setup(n => n.OnMessageReceived).Returns(new Subject<string>());
            _mockNetwork.Setup(n => n.OnEventReceived).Returns(new Subject<(EventType, object)>());
        }

        [Test]
        public async Task SendMessageAsync_CallsNetworkSend_AndBroadcasts()
        {
            // Arrange
            var messageSubject = new Subject<string>();
            _mockNetwork.Setup(n => n.OnMessageReceived).Returns(messageSubject);
            string? receivedMessage = null;
            _manager = new ChatManager(_mockNetwork.Object);
            _manager.Messages.Subscribe(msg =>
            {
                receivedMessage = msg;
            });

            _mockNetwork.Setup(n => n.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
                       .Returns(Task.CompletedTask)
                       .Callback<string, string>((msg, sender) =>
                       {
                           messageSubject.OnNext($"{sender}: {msg}"); // Simulate broadcast
                       });

            // Act
            await _manager.SendChatMessageAsync("Hello", "Player1");

            // Assert
            _mockNetwork.Verify(n => n.SendMessageAsync("Hello", "Player1"), Times.Once());
            Assert.That(receivedMessage, Is.EqualTo("Player1: Hello"));
        }

        [Test]
        public async Task SendNotificationAsync_WithBuilder_CallsRaiseEvent()
        {
            // Arrange
            var eventSubject = new Subject<(EventType, object)>();
            _mockNetwork.Setup(n => n.OnEventReceived).Returns(eventSubject);
            (EventType, object) receivedEvent = default;
            _manager = new ChatManager(_mockNetwork.Object);
            _manager.Events.Subscribe(ev => receivedEvent = ev);

            _mockNetwork.Setup(n => n.RaiseEventAsync(It.IsAny<EventType>(), It.IsAny<object>()))
                       .Returns(Task.CompletedTask)
                       .Callback<EventType, object>((type, data) =>
                           eventSubject.OnNext((type, data)));

            var builder = new NotificationBuilder()
                .SetType(EventType.KillNotification)
                .SetMessage("Player1 killed Player2");
            var notification = builder.Build();

            // Act
            await _manager.SendNotificationAsync(notification.Item1, notification.Item2);

            // Assert
            _mockNetwork.Verify(n => n.RaiseEventAsync(EventType.KillNotification, "Player1 killed Player2"), Times.Once());
            Assert.That(receivedEvent.Item1, Is.EqualTo(EventType.KillNotification));
            Assert.That(receivedEvent.Item2, Is.EqualTo("Player1 killed Player2"));
        }

        [Test]
        public async Task SendMessageAsync_OnDisconnect_RetriesAfterReconnect()
        {
            // Arrange
            var messageSubject = new Subject<string>();
            _mockNetwork.Setup(n => n.OnMessageReceived).Returns(messageSubject);

            _mockNetwork.SetupSequence(n => n.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
                       .ThrowsAsync(new Exception("Disconnected"))
                       .Returns(Task.CompletedTask);

            _mockNetwork.Setup(n => n.SimulateReconnect()).Callback(() => { });
            _manager = new ChatManager(_mockNetwork.Object);

            // Act
            await _manager.SendChatMessageAsync("Hello", "Player1");

            // Assert
            _mockNetwork.Verify(n => n.SimulateReconnect(), Times.Once());
            _mockNetwork.Verify(n => n.SendMessageAsync("Hello", "Player1"), Times.Exactly(2)); // Initial + retry
        }
    }
}