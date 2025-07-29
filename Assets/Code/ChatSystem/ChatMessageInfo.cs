namespace ChatSystem.Core
{
    public class ChatMessageInfo
    {
        public string Sender { get; private set; }
        public string Message { get; private set; }
        public ChatType? Type { get; private set; }
        public TeamType? Team { get; private set; }

        public ChatMessageInfo(string sender, string message, ChatType type, TeamType? team = null)
        {
            Sender = sender;
            Message = message;
            Type = type;
            Team = team;
        }

        public void ChangeMessage(string? sender = null, string? message = null, ChatType? type = null, TeamType? team = null)
        {
            if (sender is not null) Sender = sender;
            if (message is not null) Message = message;
            if (type is not null) Type = type.Value;
            if (team is not null) Team = team;
        }
    }
}