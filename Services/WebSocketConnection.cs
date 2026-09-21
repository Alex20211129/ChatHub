using System.Net.WebSockets;

namespace ChatRoomSys.Services
{
    public class WebSocketConnection
    {
        public required WebSocket Socket { get; init; }
        public required ChatParticipant Participant { get; set; }
        public SemaphoreSlim SendLock { get; } = new(1, 1);
    }
}
