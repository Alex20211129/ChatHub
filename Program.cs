using ChatRoomSys.Data;
using ChatRoomSys.Models;
using MemberShipSys.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;
using ChatRoomSys.Hubs;
using ChatRoomSys.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
try
{
    Log.Information("ChatRoomSys 啟動中...");

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    builder.Services.AddMembershipSystem(builder.Configuration);

    builder.Services.AddDbContext<ChatDbContext>(options=>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddSignalR();

    //誰在線上 功能，因跨連線、跨呼叫共用的狀態，要另外拉一個 singleton 服務（生命週期是整個應用程式，不是每次呼叫）
    builder.Services.AddSingleton<SignalRChatState>();
    builder.Services.AddSingleton<WebSocketChatState>();
    builder.Services.AddSingleton<ChatConnectionManager>();

    builder.Host.UseSerilog((context,services,configuration)=> configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console()
        .WriteTo.File("Logs/log-.txt",rollingInterval:RollingInterval.Day));

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Events.OnSignedIn = async context =>
        {
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ChatDbContext>();
            dbContext.LoginAudits.Add(new LoginAudit
            {
                UserId = context.Principal!.FindFirstValue(ClaimTypes.NameIdentifier)!,
                UserName = context.Principal!.Identity!.Name!,
                EventType = LoginAuditEventType.Login,
                IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString()
            });
            await dbContext.SaveChangesAsync();
        };

        options.Events.OnSigningOut = async context =>
        {
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ChatDbContext>();
            dbContext.LoginAudits.Add(new LoginAudit
            {
                UserId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                UserName = context.HttpContext.User.Identity!.Name!,
                EventType = LoginAuditEventType.Logout,
                IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString()
            });
            await dbContext.SaveChangesAsync();
        };
    });

    var app = builder.Build();

    await app.Services.SeedMembershipSystemAsync(builder.Configuration);

    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseWebSockets();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();
    app.MapHub<ChatHub>("/hubs/chat");

    app.Map("/ws/chat",async (HttpContext context , ChatConnectionManager manager) =>
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var socket = await context.WebSockets.AcceptWebSocketAsync();
        await manager.HandleConnectionAsync(socket, context.User, context.RequestAborted);
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "應用程式啟動失敗");
}
finally
{
    Log.CloseAndFlush();
}
