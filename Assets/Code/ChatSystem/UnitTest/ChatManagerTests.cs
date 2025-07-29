using ChatSystem.Core;
using ChatSystem.Mocks;
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
            _mockNetwork.Setup(n => n.OnMessageReceived).Returns(new Subject<ChatMessageInfo>());
            _mockNetwork.Setup(n => n.OnEventReceived).Returns(new Subject<(EventType, object)>());
        }

        [Test]
        public async Task SendMessageAsync_CallsNetworkSend_AndBroadcasts()
        {
            // Arrange
            var messageSubject = new Subject<ChatMessageInfo>();
            _mockNetwork.Setup(n => n.OnMessageReceived).Returns(messageSubject);

            ChatMessageInfo? receivedMessage = null;

            _manager = new ChatManager(_mockNetwork.Object);
            _manager.Messages.Subscribe(msg => receivedMessage = msg);
            ChatMessageInfo expectedMessage = new("Player1", "Hello", ChatType.Public);

            _mockNetwork.Setup(n => n.SendMessageAsync(It.IsAny<ChatMessageInfo>()))
                       .Returns(Task.CompletedTask)
                       .Callback<ChatMessageInfo>(message =>
                       {
                           messageSubject.OnNext(message);
                       });

            // Act
            await _manager.SendChatMessageAsync(expectedMessage);

            // Assert
            _mockNetwork.Verify(n => n.SendMessageAsync(It.Is<ChatMessageInfo>(message => 
                message.Sender == "Player1" &&
                message.Message == "Hello" && 
                message.Type == ChatType.Public)), Times.Once());

            Assert.That(receivedMessage, Is.Not.Null);
            Assert.That(receivedMessage.Sender, Is.EqualTo("Player1"));
            Assert.That(receivedMessage.Message, Is.EqualTo("Hello"));
            Assert.That(receivedMessage.Type, Is.EqualTo(ChatType.Public));
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
            var messageSubject = new Subject<ChatMessageInfo>();
            _mockNetwork.Setup(n => n.OnMessageReceived).Returns(messageSubject);

            _mockNetwork.SetupSequence(n => n.SendMessageAsync(It.IsAny<ChatMessageInfo>()))
                        .ThrowsAsync(new Exception("Disconnected"))
                        .Returns(Task.CompletedTask);

            _mockNetwork.Setup(n => n.SimulateReconnect()).Callback(() => { });
            _manager = new ChatManager(_mockNetwork.Object);
            ChatMessageInfo expectedMessage = new("Player1", "Hello", ChatType.Public);

            // Act
            await _manager.SendChatMessageAsync(expectedMessage);

            // Assert
            _mockNetwork.Verify(n => n.SimulateReconnect(), Times.Once());
            _mockNetwork.Verify(n => n.SendMessageAsync(expectedMessage), Times.Exactly(2)); // Initial + retry
        }

        [Test]
        public async Task SendMessageAsync_Broadcast_ToMultipleClients()
        {
            // Arrange
            var room = new MockChatRoom();
            var player1 = new MockChatNetwork("Player1", TeamType.Red, room);
            var player2 = new MockChatNetwork("Player2", TeamType.Red, room);
            var manager1 = new ChatManager(player1);
            var manager2 = new ChatManager(player2);

            ChatMessageInfo? receivedByPlayer2 = null;
            manager2.Messages.Subscribe(message => receivedByPlayer2 = message);
            ChatMessageInfo expectedMessage = new("Player1", "Hello", ChatType.Public, TeamType.Red);

            // Act
            await manager1.SendChatMessageAsync(expectedMessage);
            await Task.Delay(300);

            // Assert
            Assert.That(receivedByPlayer2, Is.EqualTo(expectedMessage));
        }

        [Test]
        public async Task SendMessageAsync_Broadcast_ToTeamClients()
        {
            // Arrange
            var room = new MockChatRoom();
            var player1 = new MockChatNetwork("Player1", TeamType.Red, room);
            var player2 = new MockChatNetwork("Player2", TeamType.Red, room);
            var player3 = new MockChatNetwork("Player3", TeamType.Blue, room);
            var manager1 = new ChatManager(player1);
            var manager2 = new ChatManager(player2);
            var manager3 = new ChatManager(player3);

            ChatMessageInfo? receivedByPlayer2 = null;
            ChatMessageInfo? receivedByPlayer3 = null;
            manager2.Messages.Subscribe(message => receivedByPlayer2 = message);
            manager3.Messages.Subscribe(message => receivedByPlayer3 = message);
            ChatMessageInfo expectedMessage = new("Player1", "Hello Red Team", ChatType.Team, TeamType.Red);

            // Act
            await manager1.SendChatMessageAsync(expectedMessage);
            await Task.Delay(300);

            // Assert
            Assert.That(receivedByPlayer2, Is.EqualTo(expectedMessage));
            Assert.That(receivedByPlayer3, Is.Null);
        }

        [Test]
        public void Mediator_Routes_ChatMessage_ToHandler()
        {
            // Arrange
            var mediator = new ChatMediator();
            ChatMessageInfo? received = null;
            mediator.Register<ChatMessageInfo>(message => received = message);

            var mockNetwork = new Mock<IChatNetwork>();
            var messageSubject = new Subject<ChatMessageInfo>();
            mockNetwork.Setup(n => n.OnMessageReceived).Returns(messageSubject);
            mockNetwork.Setup(n => n.OnEventReceived).Returns(new Subject<(EventType, object)>());

            var manager = new ChatManager(mockNetwork.Object, mediator);

            // Act
            var message = new ChatMessageInfo("Player1", "Hello", ChatType.Public);
            messageSubject.OnNext(message);

            // Assert
            Assert.That(received, Is.Not.Null);
            Assert.That(received.Message, Is.EqualTo("Hello"));
        }
    }
}