# ChatRoomSys

一個 ASP.NET Core 練習作品集：**同一個聊天室功能，分別用原生 WebSocket 與 SignalR 各實作一次**，並直接沿用 `MemberShipSys.Core` 這個會員系統 library（獨立的 Razor Class Library，尚未公開），藉此對照「手刻底層連線管理」跟「框架高階封裝」的差異。

🔗 線上展示：<https://chat.alexdemo1.com>
（登入頁提供 5 組測試帳號可直接點選帶入，不需要自行註冊）

## 這個專案想展示什麼

- **兩種即時通訊技術的實作對照**：`/Chat/WebSocket` 用 `System.Net.WebSockets` 手刻連線管理、廣播、在線名單、斷線重連；`/Chat/SignalR` 用 ASP.NET Core SignalR Hub，享有內建的自動重連、事件分派機制。兩者共用同一套畫面渲染邏輯，但連線層完全獨立，互不影響、也互不互通。
- **跨專案重用會員系統**：透過 `ProjectReference` 直接引用 `MemberShipSys.Core`（一個獨立的 Identity 會員系統 Razor Class Library），不修改該 library 原始碼即可疊加聊天室功能（唯一例外是修正了該 library 一個資料 seed 順序的 bug，詳見 commit history）。
- **訪客也能聊天**：不用登入就能用暱稱加入聊天室；登入會員則額外享有「載入最近 50 則歷史訊息」的權限。暱稱撞名時會自動加上後綴（撞到真實會員帳號名稱 → `_同名N`；撞到目前使用中的顯示名稱 → `_vN`）。

## 架構總覽

```
┌─────────────────────────────────────────────────────────────┐
│                        ChatRoomSys (Web)                     │
│                                                                │
│  ┌──────────────┐   ┌──────────────┐   ┌───────────────────┐ │
│  │  ChatHub     │   │ ChatConnection│  │  ChatController /  │ │
│  │  (SignalR)   │   │ Manager (WS)  │  │  Views/Chat/*       │ │
│  └──────┬───────┘   └──────┬───────┘   └───────────────────┘ │
│         │                  │                                  │
│  ┌──────▼───────┐   ┌──────▼───────┐                          │
│  │SignalRChatState│  │WebSocketChat │  ← 各自獨立的            │
│  │  (在線名單)    │  │State (在線名單)│    Singleton 狀態       │
│  └──────┬───────┘   └──────┬───────┘                          │
│         │                  │                                  │
│  ┌──────▼──────────────────▼───────┐                          │
│  │          ChatDbContext          │  ← SignalRChatMessages /  │
│  │                                  │    WebSocketChatMessages /│
│  └──────────────────────────────────┘    LoginAudits          │
└─────────────────────────────────────────────────────────────┘
                          │
                          │ ProjectReference
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                  MemberShipSys.Core (RCL)                    │
│   Identity（MembershipUser）、AccountController（登入/註冊/    │
│   Google OAuth）、ApplicationDbContext                        │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
              SQL Server（同一顆資料庫，兩個 DbContext 分管各自的表）
```

**資料庫**：`ChatDbContext`（本專案自己的）跟 `ApplicationDbContext`（`MemberShipSys.Core` 的）指向同一顆實體資料庫，但完全不共用 EF Core model，各自 migration、各自管理自己的表，避免回頭修改被引用的 library。

**Program.cs 關鍵接線**：`AddMembershipSystem()`（會員系統）+ `AddSignalR()` + `UseWebSockets()` + 兩個聊天狀態的 `AddSingleton` + `/hubs/chat`（SignalR）與 `/ws/chat`（原生 WebSocket）兩條路由，皆允許匿名連線，是否登入由連線邏輯內部自行判斷。

## 技術棧

- ASP.NET Core 10 MVC
- SignalR（`Microsoft.AspNetCore.SignalR`）
- 原生 `System.Net.WebSockets`
- Entity Framework Core + SQL Server
- ASP.NET Core Identity（透過 `MemberShipSys.Core`）
- Serilog（Console + 每日滾動檔案）
- Bootstrap 5 + Bootstrap Icons
- 前端使用 ES Module（`import`/`export`），SignalR client 走 ESM CDN

## 專案結構

```
Controllers/        ChatController、HomeController
Hubs/                ChatHub.cs（SignalR 版聊天邏輯）
Services/            ChatConnectionManager.cs（原生 WebSocket 版）
                     SignalRChatState.cs / WebSocketChatState.cs（在線名單）
                     ChatParticipant.cs / WebSocketConnection.cs
Data/                ChatDbContext.cs
Models/              WebSocketChatMessage / SignalRChatMessage / LoginAudit
Views/Chat/          WebSocket.cshtml / SignalR.cshtml
Views/Account/       Login.cshtml（覆蓋 MemberShipSys.Core 內建的登入頁，
                     加上作品集測試帳號區塊）
wwwroot/js/          chat-render.js（共用渲染邏輯）
                     chat-websocket.js / chat-signalr.js（各自連線邏輯）
deploy-scripts/      EF Core migration SQL、測試帳號 seed script（部署用）
```

## 本機開發

1. 需要 .NET 10 SDK、SQL Server（LocalDB 即可）。
2. 專案透過相對路徑 `ProjectReference` 引用 `..\..\MemberShipSys\MemberShipSys.Core`，須確保該 repo 存在於同一層目錄下的 `MemberShipSys` 資料夾。
3. 設定 `appsettings.Development.json` 的 `ConnectionStrings:DefaultConnection`。
4. 套用 migration：
   ```
   dotnet ef database update --context ApplicationDbContext --project ..\..\MemberShipSys\MemberShipSys.Core\MemberShipSys.Core.csproj --startup-project .
   dotnet ef database update --context ChatDbContext
   ```
5. `dotnet run --launch-profile http`，首頁會自動 seed 管理員帳號（預設 `admin@example.com` / `Admin123!`，正式環境務必透過 `AdminSeed:Email`/`AdminSeed:Password` 覆蓋）。

## 部署

採「本機 `dotnet publish` 產生自包含輸出、複製到伺服器」的方式（IIS + ASP.NET Core Module），不在伺服器上 clone 原始碼 build，避開跨 repo 相對路徑在伺服器上解析不到的問題。正式環境的連線字串、`AdminSeed`、`Resend`、`GoogleOAuth` 皆透過伺服器上的 `appsettings.Production.json` 設定，不進版本控制。`deploy-scripts/` 底下的 SQL script 是搭配這個流程、在本機產生後拿去正式資料庫執行用的。

IIS 部署需注意：要另外啟用 Windows 的 **WebSocket 通訊協定**功能（`Web Server (IIS)` → `Application Development` → `WebSocket Protocol`），否則 SignalR 還能靠降級傳輸方式運作，但原生 WebSocket 版會直接連不上。
