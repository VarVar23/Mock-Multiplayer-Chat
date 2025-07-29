using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ChatSystem.Core;

namespace ChatSystem.Mocks
{
    public class MockChatNetwork : IChatNetwork
    {
        public static List<MockChatNetwork> Clients = new();
        public string PlayerName { get; }
        public TeamType Team { get; }
        public ISubject<ChatMessageInfo> OnMessageReceived => _messageSubject;
        public ISubject<(EventType, object)> OnEventReceived => _eventSubject;

        private readonly Subject<ChatMessageInfo> _messageSubject = new();
        private readonly Subject<(EventType, object)> _eventSubject = new();
        private bool _isConnected = true;
        private readonly int _latencyMs = 200;

        public MockChatNetwork(string playerName, TeamType team)
        {
            PlayerName = playerName;
            Team = team;
            Clients.Add(this);
        }

        public async Task SendMessageAsync(ChatMessageInfo message)
        {
            if (!_isConnected) throw new Exception("Disconnected");
            await Task.Delay(_latencyMs);

            foreach (var client in Clients)
            {
                if(message.Type == ChatType.Public)
                {
                    client._messageSubject.OnNext(message);
                }
                else if(message.Type == ChatType.Team && message.Team == client.Team)
                {
                    client._messageSubject.OnNext(message);
                }
            }
        }

        public async Task RaiseEventAsync(EventType eventType, object data)
        {
            if (!_isConnected) throw new Exception("Disconnected");
            await Task.Delay(_latencyMs);

            foreach (var client in Clients)
            {
                client._eventSubject.OnNext((eventType, data));
            }
        }

        public void SimulateDisconnect() => _isConnected = false;
        public void SimulateReconnect() => _isConnected = true;
    }
}