public class SignalRCommandAudioController : SignalRAudioControllerBase<SignalRCommandController, (string Command, string Value)>
{
    public override void OnInitEvent(object sender, EventArgs<(string Command, string Value)> args)
    {
        if (args.Data.Command == "Audio")
        {
            HandleAudioCommand(args.Data.Value);
        }
    }
}
