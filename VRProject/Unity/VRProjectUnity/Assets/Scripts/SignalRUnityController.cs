using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnityEngine;

public class SignalRUnityController : MonoBehaviour
{
    public bool useSignalR = true;
    public string signalRUrl = "http://localhost:64987/chathub"; // "http://localhost:53353/ChatHub" //http://localhost:64987/

    private HubConnection _hubConnection = null;
    //private IHubProxy _hubProxy;
    //private Subscription _subscription;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("starting");

        if (useSignalR)
        {
            StartSignalR();

        }
    }

    public class UnityLoggingProvider : ILoggerProvider
    {
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName) => new UnityLogger(categoryName);

        public void Dispose() { }

        public static UnityLoggingProvider Instance { get; } = new UnityLoggingProvider();
    }

    public class UnityLoggerScope<TState> : IDisposable
    {
        private TState State { get; set; }

        public UnityLoggerScope(TState state)
        {
            State = state;
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }

    public class UnityLogger : Microsoft.Extensions.Logging.ILogger
    {
        public string CategoryName { get; private set; }

        public UnityLogger(string categoryName) => CategoryName = categoryName;

        public IDisposable BeginScope<TState>(TState state) => new UnityLoggerScope<TState>(state);

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => Debug.Log($"<{logLevel}> [{eventId}] {state} {exception} {formatter(state, exception)}");
    }

    private async Task<string> GetJwtToken(string userId)
    {
        var http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:64987/")
        };
        var httpResponse = await http.GetAsync($"/generatetoken?user={userId}");
        httpResponse.EnsureSuccessStatusCode();
        return await httpResponse.Content.ReadAsStringAsync();
    }

    void StartSignalR()
    {
        if (_hubConnection == null)
        {
            var http = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:64987/")
            };

            Debug.Log("configuring Connection...");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(signalRUrl,
                opt =>
                {
                    Debug.Log($"Setting Options...");

                    //opt.LogLevel = SignalRLogLevel.None;
                    //opt.Transport = HttpTransportType.WebSockets;
                    opt.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                    opt.SkipNegotiation = true;
                    opt.AccessTokenProvider = async () =>
                    {
                        Debug.Log($"Trying to get Access Token...");

                        var token = await this.GetJwtToken("DemoUser2");
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

            _hubConnection.On<string, string>("Receive", (user, message) =>
            {
                Debug.Log($"Receive {message}");
            });

            _hubConnection.On<string>("Send", message =>
            {
                var splittedMessage = message.Split(':');
                if (splittedMessage.Length > 1 && splittedMessage[1].Trim().StartsWith("Farbe"))
                {
                    var messageParts = message.Split('=');
                    if (messageParts.Length == 2 && ColorsDict.TryGetValue(messageParts[1].ToLowerInvariant(), out var color))
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            gameObject.scene.GetRootGameObjects()
                            .First(go => go.name == "Cube")
                            .GetComponent<Renderer>().material.color = color;
                        });
                    }
                }

                else
                {
                    Debug.Log($"Send {message}");
                }

            });

            Debug.Log("Starting Asynchronously...");
            _hubConnection.StartAsync();
            
            Debug.Log("Started..." + $"{_hubConnection.State}");
            
        }
        else
        {
            Debug.Log("Signalr already connected...");
        }
    }

    private static Dictionary<string, Color> ColorsDict = new Dictionary<string, Color>
    {
        [nameof(Color.red)] = Color.red,
        [nameof(Color.black)] = Color.black,
        [nameof(Color.blue)] = Color.blue,
        [nameof(Color.clear)] = Color.clear,
        [nameof(Color.cyan)] = Color.cyan,
        [nameof(Color.gray)] = Color.gray,
        [nameof(Color.green)] = Color.green,
        [nameof(Color.grey)] = Color.grey,
        [nameof(Color.magenta)] = Color.magenta,
        [nameof(Color.red)] = Color.red,
        [nameof(Color.white)] = Color.white,
        [nameof(Color.yellow)] = Color.yellow,
    };
    
    //public void Send(string method, string message)
    //{
    //    if (!useSignalR)
    //        return;

    //    var json = "{" + string.Format("\"action\": \"{0}\", \"value\": {1}", method, message) + "}";
    //    _hubProxy.Invoke("Send", "UnityClient", json);
    //}

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("State..." + $"{_hubConnection.State}");

    }
}
