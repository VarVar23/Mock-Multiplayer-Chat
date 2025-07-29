using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace ChatSystem.Core
{
    public interface IChatNetwork
    {
        Task SendMessageAsync(ChatMessageInfo message);
        Task RaiseEventAsync(EventType eventType, object data);
        ISubject<ChatMessageInfo> OnMessageReceived { get; }
        ISubject<(EventType, object)> OnEventReceived { get; }
        void SimulateDisconnect();
        void SimulateReconnect();
    }
}