namespace ChatRoomSys.Services
{
    public record ChatParticipant
    {
        public required string ConnectionId { get; init; }
        public string? UserId { get; init; }        // null = 訪客
        public required string DisplayName { get; set; }
        public bool IsGuest { get; init; }
    }
}
