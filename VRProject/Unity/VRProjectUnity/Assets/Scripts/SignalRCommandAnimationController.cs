
public class SignalRCommandAnimationController : SignalRAnimationControllerBase<SignalRCommandController, (string Command, string Value)>
{
    public override void OnInitEvent(object sender, EventArgs<(string Command, string Value)> args)
    {
        if (args.Data.Command == "Animation")
        {
            PlayAnimation(args.Data.Value);
        }
    }
}