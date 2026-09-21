using ChatRoomSys.Data;
using ChatRoomSys.Models;
using MemberShipSys.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ChatRoomSys.Services
{
    public class ChatConnectionManager
    {
        private readonly WebSocketChatState _chatState;
        private readonly IServiceScopeFactory _scopeFactory;

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private class IncomingEnvelope
        {
            public string? Type { get; set; }
            public string? DisplayName { get; set; }
            public string? Text { get; set; }
        }

        public ChatConnectionManager(WebSocketChatState chatState, IServiceScopeFactory scopeFactory)
        {
            _chatState = chatState;
            _scopeFactory = scopeFactory;
        }
        public async Task HandleConnectionAsync(WebSocket socket, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<MembershipUser>>();
            var chatDbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();

            var connectionId = Guid.NewGuid().ToString();
            var isAuthenticated = user.Identity?.IsAuthenticated ?? false;
            string? userId = isAuthenticated ? userManager.GetUserId(user) : null;

            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    // 收一則完整訊息（等一下講怎麼寫）
                    // 解析 type，分派到對應的 private 方法處理
                    var text = await ReceiveTextMessageAsync(socket, cancellationToken);
                    if (text is null)
                    {
                        break; // 對方要求關閉連線
                    }

                    var envelope = JsonSerializer.Deserialize<IncomingEnvelope>(text, JsonOptions);
                    if (envelope?.Type is null)
                    {
                        continue; // 格式不對或沒有 type，忽略這則
                    }

                    switch (envelope.Type)
                    {
                        case "join":
                            await HandleJoinAsync(socket, connectionId, userId, isAuthenticated, envelope.DisplayName ?? "", userManager, chatDbContext, cancellationToken);
                            break;
                        case "message":
                            await HandleMessageAsync(connectionId, envelope.Text ?? "", userManager, chatDbContext, cancellationToken);
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 連線被取消（伺服器關閉、使用者離開頁面等），視為正常斷線，不用往外拋例外
            }
            catch (WebSocketException)
            {
                // WebSocket 層級的連線異常（例如對方網路突然斷開），同樣視為正常斷線
            }
            finally
            {
                var participant = _chatState.Remove(connectionId);
                if (participant is not null)
                {
                    await BroadcastAsync(new { type = "SystemMessage", text = $"{participant.DisplayName} 離開了聊天室" }, CancellationToken.None);
                    await BroadcastAsync(new { type = "OnlineList", displayNames = _chatState.GetOnlineDisplayNames() }, CancellationToken.None);
                }
            }
        }

        private static async Task<string?> ReceiveTextMessageAsync(WebSocket socket, CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];
            using var messageStream = new MemoryStream();

            while (true)
            {
                var result = await socket.ReceiveAsync(buffer, cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);
                    return null;
                }

                messageStream.Write(buffer, 0, result.Count);

                if (result.EndOfMessage)
                {
                    break;
                }
            }

            return Encoding.UTF8.GetString(messageStream.ToArray());
        }


        private async Task HandleJoinAsync(WebSocket webSocket, string connectionId, string? userId, bool isAuthenticated, string displayName, UserManager<MembershipUser> userManager, ChatDbContext chatDbContext, CancellationToken cancellationToken)
        {

            displayName = await ResolveDisplayNameAsync(displayName, userId, userManager);
            var socketConnection = new WebSocketConnection
            {
                Participant = new ChatParticipant
                {
                    ConnectionId = connectionId,
                    UserId = userId,
                    DisplayName = displayName,
                    IsGuest = !isAuthenticated
                },
                Socket = webSocket
            };

            _chatState.Add(socketConnection);

            await SendAsync(socketConnection, new { type = "Joined", displayName, isAuthenticated }, cancellationToken);
            if (isAuthenticated)
            {
                List<WebSocketChatMessage> history = await chatDbContext.WebSocketChatMessages
                    .OrderByDescending(m => m.SentAt)
                    .Take(50)
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();

                await SendAsync(socketConnection, new { type = "History", messages = history }, cancellationToken);
            }

            await BroadcastAsync(new { type = "SystemMessage", text = $"{displayName} 加入了聊天室" }, cancellationToken);
            await BroadcastAsync(new { type = "OnlineList", displayNames = _chatState.GetOnlineDisplayNames() }, cancellationToken);
        }

        private async Task HandleMessageAsync(string connectionId, string text, UserManager<MembershipUser> userManager, ChatDbContext chatDbContext, CancellationToken cancellationToken)
        {
            var chatParticipant = _chatState.Get(connectionId);
            if (chatParticipant is null || string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            WebSocketChatMessage wChatMessage = new WebSocketChatMessage
            {
                UserId = chatParticipant.UserId,
                DisplayName = chatParticipant.DisplayName,
                IsGuest = chatParticipant.IsGuest,
                Text = text,
                SentAt = DateTime.UtcNow
            };
            chatDbContext.WebSocketChatMessages.Add(wChatMessage);
            await chatDbContext.SaveChangesAsync();
            await BroadcastAsync(new { type = "ReceiveMessage", message = wChatMessage }, cancellationToken);
        }

        //Clients.All.SendAsync
        private async Task BroadcastAsync(object payload, CancellationToken cancellationToken)
        {

            var webSocketConnections = _chatState.GetAll();
            foreach (var item in webSocketConnections)
            {
                if (item.Socket.State == WebSocketState.Open)
                {
                    await SendAsync(item, payload, cancellationToken);
                }
            }
        }
        //Clients.Caller.SendAsync
        private static async Task SendAsync(WebSocketConnection connection, object payload, CancellationToken cancellationToken)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions);

            await connection.SendLock.WaitAsync(cancellationToken);
            try
            {
                await connection.Socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
            }
            finally
            {
                connection.SendLock.Release();
            }
        }

        private async Task<string> ResolveDisplayNameAsync(string requestedDisplayName, string? userId ,UserManager<MembershipUser> userManager)
        {
            var baseName = string.IsNullOrWhiteSpace(requestedDisplayName) ? "訪客" : requestedDisplayName.Trim();

            // 第一階段：撞到「真實會員帳號名稱」(排除自己)
            var candidate = baseName;
            var suffix = 0;
            while (true)
            {
                var matchedUser = await userManager.FindByNameAsync(candidate);
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
