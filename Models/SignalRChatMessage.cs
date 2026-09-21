using System.ComponentModel.DataAnnotations;

namespace ChatRoomSys.Models
{
    public class SignalRChatMessage
    {
        public int Id { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }

        [MaxLength(256)]
        public string DisplayName { get; set; } = string.Empty;

        public bool IsGuest { get; set; }

        [MaxLength(2000)]
        public string Text { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }

}
