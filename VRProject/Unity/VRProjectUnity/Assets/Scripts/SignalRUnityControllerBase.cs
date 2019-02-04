using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class SignalRUnityControllerBase : MonoBehaviour
{
    #region public fields for unity

    /// <summary>
    /// Soll Fernsteuerung aktiviert werden?
    /// </summary>
    public bool remoteControlEnabled = true;

    /// <summary>
    /// Die Adresse des SignalR-Servers, mit dem wir uns verbinden
    /// </summary>
    public string signalRServer = "http://localhost:64987/";

    private string UsedSignalRServer;

    /// <summary>
    /// Der Name des SignalR-Hubs, mit dem wir uns verbinden
    /// </summary>
    public string hub = "chathub";
    
    #endregion

    private string SignalRUrl => $"{UsedSignalRServer}/{hub}";

    private HubConnection HubConnection = null;

    private async Task<string> GetJwtToken(string userId)
    {
        var http = new HttpClient { BaseAddress = new Uri(signalRServer) };
        var httpResponse = await http.GetAsync($"/generatetoken?user={userId}");
        httpResponse.EnsureSuccessStatusCode();
        return await httpResponse.Content.ReadAsStringAsync();
    }
    
    // Start is called before the first frame update
    protected virtual void StartVirtual()
    {
        if (Environment.GetCommandLineArgs().FirstOrDefault(a => a.StartsWith("server=")) is string serverArg)
        {
            UsedSignalRServer = serverArg.Split('=')[1];
        }
        else
        {
            UsedSignalRServer = signalRServer;
        }

        Debug.Log("starting");

        if (remoteControlEnabled)
        {
            StartSignalR();
        }
    }

    void StartSignalR()
    {
        if (HubConnection == null)
        {
            Debug.Log("configuring Connection...");

            HubConnection = new HubConnectionBuilder()
                .WithUrl(SignalRUrl,
                opt =>
                {
                    Debug.Log($"Setting Options...");
                    opt.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                    opt.SkipNegotiation = true;
                    opt.AccessTokenProvider = async () =>
                    {
                        Debug.Log($"Trying to get Access Token...");
                        var token = await GetJwtToken($"Unity{Process.GetCurrentProcess().Id}_{Environment.MachineName}");
                        Debug.Log($"Access Token: {token}");
                        return token;
                    };
                })
                .AddJsonProtocol()
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddProvider(UnityLoggingProvider.Instance);
                })
                .Build();

            Debug.Log("Attaching event...");
            HubConnection.Closed += HubConnection_Closed;

            RegisterCommands(HubConnection);

            //_hubConnection.On<string, string>("Receive", (user, message) =>
            //{
            //    Debug.Log($"Receive {message}");
            //});
            
            Debug.Log("Starting Asynchronously...");
            HubConnection.StartAsync();

            Debug.Log("Started..." + $"{HubConnection.State}");
        }
        else
        {
            Debug.Log("SignalR already connected...");
        }
    }

    private Task HubConnection_Closed(Exception arg)
    {
        Debug.Log($"Connection Closed with {arg}");
        HubConnection = null;
        return Task.CompletedTask;
    }

    protected virtual void RegisterCommands(HubConnection hubConnection)
    { }
}
