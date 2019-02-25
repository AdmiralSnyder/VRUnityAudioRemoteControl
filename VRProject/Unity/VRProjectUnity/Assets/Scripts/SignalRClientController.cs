using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using UnityEngine;
using System.Net.Http;

using Debug = UnityEngine.Debug;

public class SignalRClientController : MonoBehaviour
{
    public SignalRConnectionManager connectionManager;
    public string clientName = "unity";
    public bool signalREnabled = true;

    public List<SignalREntityController> EntityControllers { get; } = new List<SignalREntityController>();

    public void Start() => StartVirtual();

    protected virtual void StartVirtual()
    {
        Debug.Log("starting");

        if (signalREnabled)
        {
            StartSignalRAsync();
        }
    }

    private string SignalRUrl => $"{connectionManager.UsedSignalRServer}/{connectionManager.hub}";

    public async Task<string> GetJwtToken(string userId)
    {
        var http = new HttpClient { BaseAddress = new Uri(connectionManager.UsedSignalRServer) };
        var httpResponse = await http.GetAsync($"/generatetoken?user={userId}");
        httpResponse.EnsureSuccessStatusCode();
        return await httpResponse.Content.ReadAsStringAsync();
    }

    private HubConnection HubConnection = null;

    async void StartSignalRAsync()
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
            //HubConnection.On<string>("Connected", connectionID =>
            //{
            //    if (connectionID == HubConnection.Connection)
            //});

            foreach (var entityController in EntityControllers)
            {
                entityController.RegisterCommands(HubConnection);
            }

            //_hubConnection.On<string, string>("Receive", (user, message) =>
            //{
            //    Debug.Log($"Receive {message}");
            //});

            Debug.Log("Starting Asynchronously...");
            await HubConnection.StartAsync();

            foreach (var entity in EntityControllers)
            {
                entity.Connected(HubConnection);
            }

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
}
