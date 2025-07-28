using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace ChatSystem.Core
{
    public interface IChatNetwork
    {
        Task SendMessageAsync(string message, string sender); // Отправить сообщение, отправитель
        Task RaiseEventAsync(EventType eventType, object data); // Уведомление
        ISubject<string> OnMessageReceived { get; } // Получено сообщение
        ISubject<(EventType, object)> OnEventReceived { get; } // Получено уведомление
        void SimulateDisconnect(); // Симуляция отключения
        void SimulateReconnect(); // Симуляция повторного подключения
    }

    public enum EventType
    {
        MatchStart, // Начало матча
        KillNotification // Уведомление об убийстве
    }
}
