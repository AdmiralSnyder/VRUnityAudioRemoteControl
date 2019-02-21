using System;
using Microsoft.AspNetCore.SignalR.Client;
using Debug = UnityEngine.Debug;

public partial class SignalRCommandController : SignalRController<(string Command, string Value)>
{
    protected override void RegisterCommands(HubConnection hubConnection)
    {
        base.RegisterCommands(hubConnection);

        hubConnection.On<string, string, string>("Command", (connectionID, userName, message) =>
        {
            Debug.Log($"CommandCommandCommandCommandCommand {connectionID} {userName} '{message}'");

            var splittedPayload = message.Split('=');
            if (splittedPayload.Length == 2)
            {
                var command = splittedPayload[0];
                var value = splittedPayload[1];

                DoExecuteCommand(command, value);
            }
            else
            {
                Debug.Log($"Command {message}");
            }
        });
    }

    public event EventHandler<EventArgs<(string Command, string Value)>> ExecuteCommand;

    private void DoExecuteCommand(string command, string value) => ExecuteCommand?.Invoke(this, (command, value));

    public override void Init(Action<object, EventArgs<(string Command, string Value)>> onRegisterCommandsEvent)
    {
        ExecuteCommand += (sender, args) => onRegisterCommandsEvent(sender, args);
    }
}
