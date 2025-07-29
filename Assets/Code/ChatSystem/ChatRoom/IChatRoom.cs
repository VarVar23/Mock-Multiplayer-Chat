using System.Collections.Generic;

namespace ChatSystem.Core
{
    public interface IChatRoom
    {
        IReadOnlyList<IChatNetwork> Clients { get; }
        void RegisterClient(IChatNetwork client);
        void UnregisterClient(IChatNetwork client);
    }
}