using System.Collections.Concurrent;

namespace ChatRoomSys.Services
{
    public class SignalRChatState
    {
        private readonly ConcurrentDictionary<string, ChatParticipant> _participants = new();

        public void Add(ChatParticipant participant)
        {
            _participants[participant.ConnectionId] = participant;
        }

        public ChatParticipant? Remove(string connectionId)
        {
            _participants.TryRemove(connectionId, out var participant);
            return participant;
        }
        public ChatParticipant? Get(string connectionId)
        {
            _participants.TryGetValue(connectionId, out var participant);
            return participant;
        }
        public bool IsDisplayNameTaken(string displayName)
        {
            return _participants.Values.Any(p =>
                string.Equals(p.DisplayName, displayName, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<string> GetOnlineDisplayNames()
        {
            return _participants.Values
                .GroupBy(p => p.UserId ?? p.ConnectionId)
                .Select(g => g.First().DisplayName);
        }
    }
}
