using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatSystem.Core
{
    public class ChatMediator : IChatMediator
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Register<T>(Action<T> handler)
        {
            var type = typeof(T);

            if (!_handlers.ContainsKey(type))
            {
                _handlers[type] = new List<Delegate>();
            }

            _handlers[type].Add(handler);
        }

        public void Unregister<T>(Action<T> handler)
        {
            var type = typeof(T);

            if (_handlers.TryGetValue(type, out var list))
            {
                list.Remove(handler);
            }
        }

        public void Publish<T>(T message)
        {
            var type = typeof(T);

            if (_handlers.TryGetValue(type, out var list))
            {
                foreach (var handler in list.Cast<Action<T>>())
                {
                    handler(message);
                }
            }
        }
    }
}