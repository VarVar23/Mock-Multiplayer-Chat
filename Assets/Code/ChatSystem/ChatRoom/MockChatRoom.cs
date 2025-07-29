using System.Collections.Generic;

namespace ChatSystem.Core
{
    public class MockChatRoom : IChatRoom
    {
        public IReadOnlyList<IChatNetwork> Clients => _clients;
        private readonly List<IChatNetwork> _clients = new();

        public void RegisterClient(IChatNetwork client)
        {
            if (!_clients.Contains(client))
            {
                _clients.Add(client);
            }
        }

        public void UnregisterClient(IChatNetwork client)
        {
            _clients.Remove(client);
        }
    }
}