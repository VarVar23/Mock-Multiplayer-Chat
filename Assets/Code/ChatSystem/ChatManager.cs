using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace ChatSystem.Core
{
    public class ChatManager
    {
        private readonly IChatNetwork _network;
        private readonly Subject<string> _messages = new();
        private readonly Subject<(EventType, object)> _events = new();

        public ChatManager(IChatNetwork network)
        {
            _network = network;
            _network.OnMessageReceived.Subscribe(_messages.OnNext);
            _network.OnEventReceived.Subscribe(_events.OnNext);
        }

        public IObservable<string> Messages => _messages;
        public IObservable<(EventType, object)> Events => _events;

        public async Task SendChatMessageAsync(string message, string sender)
        {
            // Асинхронная отправка с retry на disconnect
            try
            {
                await _network.SendMessageAsync(message, sender);
            }
            catch (Exception)
            {
                _network.SimulateReconnect();
                await Task.Delay(500); // Имитация retry delay
                await _network.SendMessageAsync(message, sender);
            }
        }

        public async Task SendNotificationAsync(EventType eventType, object data)
        {
            await _network.RaiseEventAsync(eventType, data);
        }
    }
}