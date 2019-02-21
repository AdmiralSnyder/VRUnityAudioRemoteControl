using Microsoft.AspNetCore.SignalR.Client;

public class SignalRRegisteredCommandAnimationController : SignalRAnimationControllerBase<SignalRRegisteredCommandController, HubConnection>
{
    public override void OnInitEvent(object sender, EventArgs<HubConnection> args)
    {
        args.Data.On<string>("Animation", anim => PlayAnimation(anim));
    }
}