using System;
using Microsoft.AspNetCore.SignalR.Client;

public partial class SignalRRegisteredCommandController : SignalRController<HubConnection>
{
    protected override void RegisterCommands(HubConnection hubConnection)
    {
        base.RegisterCommands(hubConnection);

        RegisterCommandsEvent?.Invoke(this, hubConnection);
    }

    public event EventHandler<EventArgs<HubConnection>> RegisterCommandsEvent;

    public override void Init(Action<object, EventArgs<HubConnection>> onRegisterCommandsEvent)
    {
        RegisterCommandsEvent += (sender, args) => onRegisterCommandsEvent(sender, args);
    }
}