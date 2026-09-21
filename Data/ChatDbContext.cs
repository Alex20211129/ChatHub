using ChatRoomSys.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatRoomSys.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
        }

        public DbSet<WebSocketChatMessage> WebSocketChatMessages => Set<WebSocketChatMessage>();
        public DbSet<SignalRChatMessage> SignalRChatMessages => Set<SignalRChatMessage>();
        public DbSet<LoginAudit> LoginAudits => Set<LoginAudit>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WebSocketChatMessage>()
                .HasIndex(m => m.SentAt);

            modelBuilder.Entity<SignalRChatMessage>()
                .HasIndex(m => m.SentAt);

            modelBuilder.Entity<LoginAudit>()
                .HasIndex(a => a.UserId);
        }
    }
}
