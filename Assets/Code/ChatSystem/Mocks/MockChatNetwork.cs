using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ChatSystem.Core;

namespace ChatSystem.Mocks
{
    public class MockChatNetwork : IChatNetwork
    {
        private readonly Subject<string> _messageSubject = new();
        private readonly Subject<(EventType, object)> _eventSubject = new();
        private bool _isConnected = true;
        private readonly int _latencyMs = 200;

        public ISubject<string> OnMessageReceived => _messageSubject;
        public ISubject<(EventType, object)> OnEventReceived => _eventSubject;

        public async Task SendMessageAsync(string message, string sender)
        {
            if (!_isConnected) throw new Exception("Disconnected");
            await Task.Delay(_latencyMs);
            _messageSubject.OnNext($"{sender}: {message}"); // Broadcast
        }

        public async Task RaiseEventAsync(EventType eventType, object data)
        {
            if (!_isConnected) throw new Exception("Disconnected");
            await Task.Delay(_latencyMs);
            _eventSubject.OnNext((eventType, data)); // Broadcast
        }

        public void SimulateDisconnect() => _isConnected = false;
        public void SimulateReconnect() => _isConnected = true;
    }
}