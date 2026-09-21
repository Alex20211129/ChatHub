using System.Collections.Concurrent;

namespace ChatRoomSys.Services
{
    public class WebSocketChatState
    {
        private readonly ConcurrentDictionary<string, WebSocketConnection> _participants = new();

        public void Add(WebSocketConnection webSocketConnection)
        {
            _participants[webSocketConnection.Participant.ConnectionId] = webSocketConnection;
        }

        public ChatParticipant? Remove(string connectionId)
        {
            _participants.TryRemove(connectionId, out var webSocketConnection);
            return webSocketConnection?.Participant;
        }
        public ChatParticipant? Get(string connectionId)
        {
            _participants.TryGetValue(connectionId, out var webSocketConnection);
            return webSocketConnection?.Participant;
        }
        public bool IsDisplayNameTaken(string displayName)
        {
            return _participants.Values.Any(p =>
                string.Equals(p.Participant.DisplayName, displayName, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<string> GetOnlineDisplayNames()
        {
            return _participants.Values
                .GroupBy(p => p.Participant.UserId ?? p.Participant.ConnectionId)
                .Select(g => g.First().Participant.DisplayName);
        }

        public IEnumerable<WebSocketConnection> GetAll() => _participants.Values;
    }
}
