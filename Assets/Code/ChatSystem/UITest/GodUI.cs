using UniRx;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ChatSystem.Core;
using ChatSystem.Mocks;

namespace ChatSystem.UI
{
    public class GodUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _inputMessage;
        [SerializeField] private Button _sendMessage;
        [SerializeField] private TMP_Text[] _texts;
        [SerializeField] private string _playerName;

        private ChatManager _chatManager;
        private ChatMessageInfo _messageInfo;

        private readonly string[] _allPlayers = new string[] { "Blaze", "Raven", "Shadow", "Viper", "Falcon", "Hunter", "Ghost", "Titan", "Phoenix", "Drifter", "Storm", "Sniper", "Wraith", "Reaper", "Bullet", "Venom", "Rogue", "Striker", "Fury", "Nova", "Blitz", "Crusher", "Raider", "Rocket", "Talon", "Joker", "Frost", "Knox", "Flame", "Havoc" };
        private readonly string[] _allChatMessages = new string[] { "Привет всем!", "Кто в пати?", "Дайте еды, плиз", "Где база?", "Я нашёл алмазы!", "Не бей, я мирный!", "Пойдём в шахту", "Кто на PvP?", "Тут грифер!", "ТП ко мне, покажу", "Сколько стоит кирка?", "Есть лишний лук?", "Спасибо!", "Поставь кровать", "Ночь наступает", "Не ломай!", "Где портал в ад?", "Нужна помощь!", "Лол, как ты выжил?", "За мной зомби!", "Кто строит дом?", "ТП на спавн", "Пошли на ивент", "Продам алмазы", "Лаги жесть...", "Я упал в лаву :(", "Где ты?", "Поставь сундук", "Фанимся на арене", "Куплю LLC SmartPayments" };
        private readonly TeamType[] _allTeams = new TeamType[] { TeamType.Red, TeamType.Green, TeamType.Blue, TeamType.Yellow };
        private readonly ChatType[] _allChatTypes = new ChatType[] { ChatType.Public, ChatType.Team };
        
        private void Start()
        {
            ClearChat();

            var room = new MockChatRoom();
            var network = new MockChatNetwork(_playerName, TeamType.Red, room);
            var mediator = new ChatMediator();
            _chatManager = new ChatManager(network, mediator);
            _messageInfo = new ChatMessageInfo("", "", ChatType.Public);

            _chatManager.Messages.Subscribe(OnMessageReceived);

            Observable.Interval(System.TimeSpan.FromSeconds(Random.Range(1, 4)))
                      .Subscribe(_ => Send(GenerateMessageInfo()))
                      .AddTo(this);

            _sendMessage.OnClickAsObservable()
                        .Where(_ => _inputMessage.text != string.Empty)
                        .Subscribe(_ =>
                        {
                            Send(new ChatMessageInfo(_playerName, _inputMessage.text, ChatType.Team, TeamType.Yellow));
                            ClearInputField();
                        })
                        .AddTo(this);
        }

        public void ClearInputField()
        {
            _inputMessage.text = string.Empty;
        }

        public void ClearChat()
        {
            for (int i = 0; i < _texts.Length; i++)
            {
                _texts[i].text = string.Empty;  
            }
        }    

        private async void Send(ChatMessageInfo messageInfo)
        {
            if (string.IsNullOrWhiteSpace(messageInfo.Message)) return;

            await _chatManager.SendChatMessageAsync(messageInfo);
        }

        private void OnMessageReceived(ChatMessageInfo message)
        {
            for (int i = 0; i < _texts.Length - 1; i++)
            {
                _texts[i].text = _texts[i + 1].text;
            }

            _texts[_texts.Length - 1].text = $"[{message.Type}] {message.Sender}: {message.Message}";
        }

        private ChatMessageInfo GenerateMessageInfo()
        {
            var player = _allPlayers[Random.Range(0, _allPlayers.Length)];
            var message = _allChatMessages[Random.Range(0, _allChatMessages.Length)];
            var chatType = _allChatTypes[Random.Range(0, _allChatTypes.Length)];
            var team = _allTeams[Random.Range(0, _allTeams.Length)];

            _messageInfo.ChangeMessage(player, message, chatType, team);

            return _messageInfo;
        }
    }
}