using VrProjectWebsite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Blazor.Extensions.SignalR.Test.Server.Hubs
{
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        public Task AddAnimations(string animations) => Clients.All.SendAsync("AddAnimations", animations);

        #region Connect / Disconnect

        public override async Task OnConnectedAsync() => await Clients.All.SendAsync("Connected", $"{Context.ConnectionId}");

        public override async Task OnDisconnectedAsync(Exception ex) => await Clients.Others.SendAsync("Disconnected", $"{Context.ConnectionId}");

        #endregion

        public Task Command(string message) => Clients.All.SendAsync("Command", Context.ConnectionId, Context.UserIdentifier, message);

        public Task Color(string color) => Clients.All.SendAsync("Color", color);

        public Task Audio(string audio) => Clients.All.SendAsync("Audio", audio);

        public Task Animation(string animation) => Clients.All.SendAsync("Animation", animation);
    }
}
