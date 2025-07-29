namespace ChatSystem.Core
{
    public class ChatMessageInfo
    {
        public string Sender { get; }
        public string Message { get; }
        public ChatType Type { get; }
        public TeamType? Team { get; }

        public ChatMessageInfo(string sender, string message, ChatType type, TeamType? team = null)
        {
            Sender = sender;
            Message = message;
            Type = type;
            Team = team;
        }
    }
}