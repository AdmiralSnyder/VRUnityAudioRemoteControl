using Microsoft.AspNetCore.SignalR.Client;

public class SignalRRegisteredCommandAudioController : SignalRAudioControllerBase<SignalRRegisteredCommandController, HubConnection>
{
    public override void OnInitEvent(object sender, EventArgs<HubConnection> args)
    {
        args.Data.On<string>("Audio", audio => HandleAudioCommand(audio));
    }
}