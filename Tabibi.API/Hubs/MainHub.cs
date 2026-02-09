using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Tabibi.API.Hubs
{
    [Authorize]
    public sealed class MainHub : Hub
    {
        // This runs automatically when the mobile app connects
        public override async Task OnConnectedAsync()
        {
            string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
        }

        // Optional: You can keep this if you want to handle specific logic on disconnect
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
