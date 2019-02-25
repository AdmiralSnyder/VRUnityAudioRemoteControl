public class SignalRCommandColorController : SignalRColorControllerBase<SignalRCommandController, (string Command, string Value)>
{
    public override void OnInitEvent(object sender, EventArgs<(string Command, string Value)> args)
    {
        if (args.Data.Command == "Color")
        {
            HandleColor(args.Data.Value);
        }
    }
}