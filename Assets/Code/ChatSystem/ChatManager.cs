using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace ChatSystem.Core
{
    public class ChatManager
    {
        public IObservable<ChatMessageInfo> Messages => _messages;
        public IObservable<(EventType, object)> Events => _events;

        private readonly IChatNetwork _network;
        private readonly Subject<ChatMessageInfo> _messages = new();
        private readonly Subject<(EventType, object)> _events = new();

        public ChatManager(IChatNetwork network)
        {
            _network = network;
            _network.OnMessageReceived.Subscribe(_messages.OnNext);
            _network.OnEventReceived.Subscribe(_events.OnNext);
        }

        public async Task SendChatMessageAsync(ChatMessageInfo message)
        {
            // Асинхронная отправка с retry на disconnect
            try
            {
                await _network.SendMessageAsync(message);
            }
            catch (Exception)
            {
                await NetworkReconnectSendMessage(message);
            }
        }

        public async Task SendNotificationAsync(EventType eventType, object data)
        {
            await _network.RaiseEventAsync(eventType, data);
        }

        private async Task NetworkReconnectSendMessage(ChatMessageInfo messageInfo)
        {
            _network.SimulateReconnect();
            await Task.Delay(500);
            await _network.SendMessageAsync(messageInfo);
        }
    }
}