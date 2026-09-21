using System.ComponentModel.DataAnnotations;

namespace ChatRoomSys.Models
{
    public enum LoginAuditEventType
    {
        Login,
        Logout
    }

    public class LoginAudit
    {
        public int Id { get; set; }

        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [MaxLength(256)]
        public string UserName { get; set; } = string.Empty;

        public LoginAuditEventType EventType { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
