using Microsoft.AspNetCore.Mvc;

namespace ChatRoomSys.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult WebSocket() => View();
        public IActionResult SignalR() => View();
    }
}
