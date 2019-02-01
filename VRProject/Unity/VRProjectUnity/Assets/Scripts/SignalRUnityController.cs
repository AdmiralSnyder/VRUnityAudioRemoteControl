using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnityEngine;

public interface IRemoteControllable
{
    Color Color { get; set; }
    string SoundFile { get; set; }
}

public class SignalRUnityController : MonoBehaviour, IRemoteControllable
{
    public AudioSource audioSource;

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

    #region Logging

    public class UnityLoggingProvider : ILoggerProvider
    {
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName) => new UnityLogger(categoryName);

        public void Dispose() { }

        public static UnityLoggingProvider Instance { get; } = new UnityLoggingProvider();
    }

    public class UnityLoggerScope<TState> : IDisposable
    {
        private TState State { get; set; } //TODO: sollte benutzt werden für irgendwas.
        public UnityLoggerScope(TState state) => State = state;
        public void Dispose() { }
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

    #endregion

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
                //  "lalABSENDERblub:payload"
                // payload = "Farbe=blue"
                string sender = message.Substring(0, message.IndexOf(':'));
                string payload = message.Substring(message.IndexOf(':')+1);
                var splittedPayload = payload.Split('=');
                if (splittedPayload.Length == 2)
                {
                    var command = splittedPayload[0].Substring(1);
                    var value = splittedPayload[1];

                    ExecuteCommand(command, value);
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

    private void ExecuteCommand(string command, string value)
    {
        switch(command)
        {
            case "Farbe":
                {
                    if (TryParseColor(value, out var color))
                    {
                        Color = color;
                    }
                    break;
                }
            case "Audio":
                {
                    switch (value)
                    {
                        case "mute":
                            {
                                MuteAudio();
                                break;
                            }
                        case "unmute":
                            {
                                UnMuteAudio();
                                break;
                            }
                        default:
                            {
                                if (value.EndsWith(".wav") && File.Exists(value))
                                {
                                    PlayAudioFile(value);
                                }
                                break;
                            }
                    }

                    break;
                }
        }
    }

    private void MuteAudio() => UnityMainThreadDispatcher.Instance().Enqueue(() => audioSource.mute = true);
    private void UnMuteAudio() => UnityMainThreadDispatcher.Instance().Enqueue(() => audioSource.mute = false);

    private void PlayAudioFile(string fileName)
    {
        AudioClip clip = null;

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            WWW www1 = new WWW("file://" + fileName);
            clip = www1.GetAudioClip(false);
        });
        while(clip is null)
        {
            System.Threading.Thread.Sleep(1);
        }
        
        bool isReadyToPlay = false;
        while (!isReadyToPlay)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                isReadyToPlay = clip.isReadyToPlay;
            });
            System.Threading.Thread.Sleep(1);
        }
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            audioSource.clip = clip;
            audioSource.Play();
        });
    }

    private bool TryParseColor(string value, out Color color) => ColorsDict.TryGetValue(value.ToLowerInvariant(), out color);

    /// <summary>
    /// Gibt die Farbe des Würfels an.
    /// </summary>
    public Color Color
    {
        get => _Color;
        set
        {
            _Color = value;
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                gameObject.scene.GetRootGameObjects()
                .First(go => go.name == "Cube")
                .GetComponent<Renderer>().material.color = value;
            });
        }
    }
    private Color _Color = Color.white;

    /// <summary>
    /// Gibt die abgespielte Datei an.
    /// </summary>
    public string SoundFile { get; set; }
    private string _SoundFile;

    
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
