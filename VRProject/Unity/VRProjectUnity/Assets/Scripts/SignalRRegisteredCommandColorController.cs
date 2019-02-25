using Microsoft.AspNetCore.SignalR.Client;

public class SignalRRegisteredCommandColorController : SignalRColorControllerBase<SignalRRegisteredCommandController, HubConnection>
{
    public override void OnInitEvent(object sender, EventArgs<HubConnection> args)
    {
        args.Data.On<string>("Color", HandleColor);
    }
}
