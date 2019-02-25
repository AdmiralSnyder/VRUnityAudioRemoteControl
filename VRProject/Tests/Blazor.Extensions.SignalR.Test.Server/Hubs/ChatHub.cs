using VrProjectWebsite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using CommonTypes;

namespace Blazor.Extensions.SignalR.Test.Server.Hubs
{
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        #region Demo-Operationen
        public async Task DoSomething()
        {
            await Clients.All.SendAsync("DemoMethodObject", new DemoData { Id = 1, Data = "Demo Data" });
            await Clients.All.SendAsync("DemoMethodList", Enumerable.Range(1, 10).Select(x => new DemoData { Id = x, Data = $"Demo Data #{x}" }).ToList());
        }

        public async Task<byte[]> DoByteArrayArg()
        {
            var array = new byte[] { 1, 2, 3 };

            await Clients.All.SendAsync("DemoByteArrayArg", array);

            return array;
        }

        //public Task Send(string message)
        //{
        //    return this.Clients.All.SendAsync("Send", $"{this.Context.ConnectionId}: {message}");
        //}

        public Task DoMultipleArgs() => Clients.All.SendAsync("DemoMultiArgs", "one", 2, "three", 4);

        public Task SendToOthers(string message) => Clients.Others.SendAsync("Send", $"{Context.ConnectionId}: {message}");

        public Task SendToConnection(string connectionId, string message) => Clients.Client(connectionId).SendAsync("Send", $"Private message from {Context.ConnectionId}: {message}");

        public Task SendToGroup(string groupName, string message) => Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId}@{groupName}: {message}");

        public Task SendToOthersInGroup(string groupName, string message) => Clients.OthersInGroup(groupName).SendAsync("Send", $"{Context.ConnectionId}@{groupName}: {message}");

        public Task DoMultipleArgsComplex()
        {
            return Clients.All.SendAsync("DemoMultiArgs2", new DemoData { Id = 1, Data = "Demo Data" },
                Enumerable.Range(1, 10).Select(x => new DemoData { Id = x, Data = $"Demo Data #{x}" }).ToList());
        }

        public Task Echo(string message) => Clients.Caller.SendAsync("Send", $"{Context.ConnectionId}: {message}");

        #endregion

        public Task AddAnimations(string animations) => Clients.All.SendAsync("AddAnimations", animations);

        #region Connect / Disconnect

        public override async Task OnConnectedAsync() => await Clients.All.SendAsync("Connected", $"{Context.ConnectionId}");

        public override async Task OnDisconnectedAsync(Exception ex) => await Clients.Others.SendAsync("Disconnected", $"{Context.ConnectionId}");

        #endregion

        public Task Command(string message) => Clients.All.SendAsync("Command", Context.ConnectionId, Context.UserIdentifier, message);

        public Task Color(string color) => Clients.All.SendAsync("Color", color);

        public Task Animation(string animation) => Clients.All.SendAsync("Animation", animation);

        public Task File(string filename, string guid, int count, int pckg, string part)
        {
            Debug.WriteLine($"FileFileFileFileFile {filename} {guid} {pckg}/{count} : {part.Length}");
            return Task.CompletedTask;
        }

        

        #region Group Management

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} joined {groupName}");
        }

        public async Task LeaveGroup(string groupName)
        {
            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} left {groupName}");

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
        
        #endregion
    }
}
