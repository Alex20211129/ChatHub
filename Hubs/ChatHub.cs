using ChatRoomSys.Data;
using ChatRoomSys.Models;
using ChatRoomSys.Services;
using MemberShipSys.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ChatRoomSys.Hubs
{
    public class ChatHub : Hub
    {
        private readonly SignalRChatState _chatState;
        private readonly UserManager<MembershipUser> _userManager;
        private readonly ChatDbContext _chatDbContext;

        public ChatHub(SignalRChatState chatState, UserManager<MembershipUser> userManager, ChatDbContext chatDbContext)
        {
            _chatState = chatState;
            _userManager = userManager;
            _chatDbContext = chatDbContext;
        }

        public async Task JoinAsync(string requestedDisplayName)
        {
            //判斷會員還是訪客
            var isAuthenticated = Context.User?.Identity?.IsAuthenticated ?? false;
            string? userId = isAuthenticated ? _userManager.GetUserId(Context.User!) : null;

            var displayName = await ResolveDisplayNameAsync(requestedDisplayName, userId);

            _chatState.Add(new ChatParticipant
            {
                ConnectionId = Context.ConnectionId,
                UserId = userId,
                DisplayName = displayName,
                IsGuest = !isAuthenticated
            });

            await Clients.Caller.SendAsync("Joined", displayName, isAuthenticated);

            if (isAuthenticated)
            {
                List<SignalRChatMessage> history = await _chatDbContext.SignalRChatMessages
                    .OrderByDescending(m => m.SentAt)
                    .Take(50)
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();

                await Clients.Caller.SendAsync("History", history);
            }

            // 6. 廣播給「所有人」：加入通知 + 更新後的在線名單
            await Clients.All.SendAsync("SystemMessage", $"{displayName} 加入了聊天室");
            await Clients.All.SendAsync("OnlineList", _chatState.GetOnlineDisplayNames());
        }

        public async Task SendMessageAsync(string message)
        {
            var chatParticipant = _chatState.Get(Context.ConnectionId);
            if (chatParticipant is null || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            SignalRChatMessage rChatMessage = new SignalRChatMessage
            {
                UserId = chatParticipant.UserId,
                DisplayName = chatParticipant.DisplayName,
                IsGuest = chatParticipant.IsGuest,
                Text = message,
                SentAt = DateTime.UtcNow
            };

            _chatDbContext.SignalRChatMessages.Add(rChatMessage);
            await _chatDbContext.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveMessage", rChatMessage);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var participant = _chatState.Remove(Context.ConnectionId);

            if (participant is not null)
            {
                await Clients.All.SendAsync("SystemMessage", $"{participant.DisplayName} 離開了聊天室");
                await Clients.All.SendAsync("OnlineList", _chatState.GetOnlineDisplayNames());
            }

            await base.OnDisconnectedAsync(exception);
        }

        //撞名處理
        private async Task<string> ResolveDisplayNameAsync(string requestedDisplayName, string? userId)
        {
            var baseName = string.IsNullOrWhiteSpace(requestedDisplayName) ? "訪客" : requestedDisplayName.Trim();

            // 第一階段：撞到「真實會員帳號名稱」(排除自己)
            var candidate = baseName;
            var suffix = 0;
            while (true)
            {
                var matchedUser = await _userManager.FindByNameAsync(candidate);
                if (matchedUser is null || matchedUser.Id == userId)
                {
                    break;
                }
                suffix++;
                candidate = $"{baseName}_同名{suffix}";
            }

            // 第二階段：撞到「目前使用中的顯示名稱」
            baseName = candidate;
            suffix = 0;
            while (_chatState.IsDisplayNameTaken(candidate))
            {
                suffix++;
                candidate = $"{baseName}_v{suffix}";
            }

            return candidate;
        }
    }
}
