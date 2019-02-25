using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SignalRConnectionManager : MonoBehaviour
{
    public string signalRServer = "http://localhost:64987";
    public string hub = "chathub";

    public string UsedSignalRServer { get; private set; }

    public void Awake()
    {
        UsedSignalRServer = signalRServer;

        if (Environment.GetCommandLineArgs().FirstOrDefault(a => a.StartsWith("server=")) is string serverArg)
        {
            UsedSignalRServer = serverArg.Split('=')[1];
        }
    }
}
