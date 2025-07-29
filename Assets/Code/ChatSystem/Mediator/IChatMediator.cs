using System;

namespace ChatSystem.Core
{
    public interface IChatMediator
    {
        void Register<T>(Action<T> handler);
        void Unregister<T>(Action<T> handler);
        void Publish<T>(T message);
    }
}