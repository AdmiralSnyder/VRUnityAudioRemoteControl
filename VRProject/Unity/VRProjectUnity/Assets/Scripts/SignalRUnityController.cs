using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnityEngine;
using Debug = UnityEngine.Debug;
using static Conditionals;

public interface IRemoteControllable
{
    Color Color { get; set; }
    string SoundFile { get; set; }
}

public class SignalRUnityController : MonoBehaviour, IRemoteControllable
{
    public AudioSource audioSource;

    public bool useSignalR = true;
    public string signalRServer = "http://localhost:64987/";
    public string hub = "chathub";
    private string SignalRUrl => $"{signalRServer}/{hub}";
    //public string signalRUrl = "http://localhost:64987/chathub"; // "http://localhost:53353/ChatHub" //http://localhost:64987/
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
        var http = new HttpClient { BaseAddress = new Uri(signalRServer) };
        var httpResponse = await http.GetAsync($"/generatetoken?user={userId}");
        httpResponse.EnsureSuccessStatusCode();
        return await httpResponse.Content.ReadAsStringAsync();
    }

    void StartSignalR()
    {
        if (_hubConnection == null)
        {
            Debug.Log("configuring Connection...");

            _hubConnection = new HubConnectionBuilder()
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
            _hubConnection.Closed += _hubConnection_Closed;

            _hubConnection.On<string, string>("Receive", (user, message) =>
            {
                Debug.Log($"Receive {message}");
            });
            _hubConnection.On<string>("Farbe", (colorString) =>
            {
                Debug.Log($"FAAAAAAAAAAAARBEEEEEEEEEEEE {colorString}");
                if (TryParseColor(colorString, out var color))
                {
                    Color = color;
                }
            });

            _hubConnection.On<string, string, string>("Send", (connectionID, userName, message) =>
            {
                var splittedPayload = message.Split('=');
                if (splittedPayload.Length == 2)
                {
                    var command = splittedPayload[0];
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
            Debug.Log("SignalR already connected...");
        }
    }

    private Task _hubConnection_Closed(Exception arg)
    {
        Debug.Log($"Connection Closed with {arg}");
        _hubConnection = null;
        return Task.CompletedTask;
    }

    private void ExecuteCommand(string command, string value)
    {
        DoCase(command,
            () => Debug.Log($"Unknown command '{command}'. (value = '{value}')"),
            ("Farbe", () => DoIf<string, Color>(value, TryParseColor, 
            c => Color = c)),
            //("Farbe", () => DoIf(() => (TryParseColor(value, out var color), color), c => Color = c)),
            ("Audio",
                () => DoCase(value, () => DoIf((value.EndsWith(".wav") || value.EndsWith(".mp3")) && File.Exists(value), () => PlayAudioFile(value)),
                ("mute", () => Muted = true),
                ("unmute", () => Muted = false))));

        //Do(Case(command)
        //    .When("Farbe").Then(() => DoIf<string, Color>(value, TryParseColor, c => Color = c))
        //    .When("Audio").Then(
        //        Case(value)
        //        .When("mute").Then(() => Muted = true)
        //        .When("unmute").Then(() => Muted = false)
        //        .Else(() => DoIf(value.EndsWith(".wav") && File.Exists(value), () => PlayAudioFile(value)))
        //    ).Else(() => Debug.Log($"Unknown command '{command}'. (value = '{value}')")));
    }

    delegate bool TryParse<TIn, TOut>(TIn value, out TOut result);

    private void DoIf<TIn, TOut>(TIn value, TryParse<TIn, TOut> tryParse, Action<TOut> action)
    {
        if (tryParse(value, out var result))
        {
            action(result);
        }
    }

    private void DoIf<T>(Func<(bool result, T resValue)> condi, Action<T> action)
    {
        var x = condi();
        if (x.result)
        {
            action(x.resValue);
        }
    }

    private void DoIf(bool condition, Action action)
    {
        if (condition)
        {
            action();
        }
    }

    private void DoCase<T>(T value, Action defaultAction, params (T condition, Action action)[] cases)
    {
        foreach (var @case in cases)
        {
            if (Equals(@case.condition, value))
            {
                @case.action();
                return;
            }
        }
        defaultAction();
    }


    private void PlayAudioFile(string fileName)
    {
        AudioClip clip = null;

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            WWW www1 = new WWW("file://" + fileName);
            clip = www1.GetAudioClip(false);
        });
        while (clip is null)
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

    private bool TryParseColor(string value, out Color color)
    {
        if (ColorsDict.TryGetValue(value.ToLowerInvariant(), out color))
        {
            return true;
        }
        else if (value[0] == '#')
        {
            byte r = byte.Parse(value.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(value.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(value.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            color = new Color32(r, g, b, 255);
            return true;
        }
        else
        {
            color = Color.white;
            return false;
        }
    }

    #region IRemoteControllable Implementation

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
    public string SoundFile
    {
        get => _SoundFile;
        set
        {
            if (_SoundFile != value)
            {
                _SoundFile = value;
                PlayAudioFile(_SoundFile);
            }
        }
    }
    private string _SoundFile;

    public bool Muted
    {
        get => _Muted;
        set
        {
            if (_Muted != value)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => audioSource.mute = value);
                _Muted = value;
            }
        }
    }
    private bool _Muted;

    #endregion

    private static readonly Dictionary<string, Color> ColorsDict = new Dictionary<string, Color>
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

    // Update is called once per frame
    void Update() { }
}

public static class Conditionals
{
    public class CaseConditional<T>
    {
        public CaseConditional(T value) => Value = value;

        public T Value { get; }

        public CaseWhenConditional<T> When(T condition) => new CaseWhenConditional<T>(this, condition);
        public Dictionary<T, CaseWhenConditional<T>> Conditions = new Dictionary<T, CaseWhenConditional<T>>();
        public CaseElseConditional<T> CaseElseConditional;
        public void RegisterWhen(CaseWhenConditional<T> caseWhenConditional) => Conditions.Add(caseWhenConditional.Condition, caseWhenConditional);
        public void RegisterElse(CaseElseConditional<T> caseElseConditional) => CaseElseConditional = caseElseConditional;
        public void Execute()
        {
            if (Conditions.TryGetValue(Value, out var whenCondition))
            {
                whenCondition.Execute();
            }
        }
    }

    public class CaseWhenConditional<T>
    {
        public CaseConditional<T> CaseConditional;
        public T Condition;

        public CaseWhenConditional(CaseConditional<T> caseConditional, T condition)
        {
            Condition = condition;
            CaseConditional = caseConditional;
            CaseConditional.RegisterWhen(this);
        }

        public CaseWhenThenConditional<T> Then(Action action) => new CaseWhenThenConditional<T>(this, action);
        private CaseWhenThenConditional<T> Consequence;
        public void RegisterConsequence(CaseWhenThenConditional<T> consequence) => Consequence = consequence;

        public CaseWhenThenConditional<T> Then(IEndConditional innerConditional) => new CaseWhenThenConditional<T>(this, innerConditional);

        internal void Execute() => Consequence.Execute();
    }

    public class CaseWhenThenConditional<T>
    {
        private CaseWhenConditional<T> CaseWhenConditional;
        private Action action;
        private IEndConditional InnerConditional;

        private CaseWhenThenConditional(CaseWhenConditional<T> caseWhenConditional)
        {
            CaseWhenConditional = caseWhenConditional;
            CaseWhenConditional.RegisterConsequence(this);
        }

        public CaseWhenThenConditional(CaseWhenConditional<T> caseWhenConditional, Action action) : this(caseWhenConditional) => this.action = action;

        public CaseWhenThenConditional(CaseWhenConditional<T> caseWhenConditional, IEndConditional innerConditional) => InnerConditional = innerConditional;
        
        public CaseWhenConditional<T> When(T condition) => new CaseWhenConditional<T>(CaseWhenConditional.CaseConditional, condition);
        public CaseElseConditional<T> Else(Action action) => new CaseElseConditional<T>(CaseWhenConditional.CaseConditional, action);
        public CaseEndConditional<T> End() => new CaseEndConditional<T>(CaseWhenConditional.CaseConditional);

        internal void Execute()
        {
            action?.Invoke();
            InnerConditional?.Execute();
        }
    }

    public class CaseElseConditional<T> : IEndConditional
    {
        private CaseConditional<T> CaseConditional;
        private Action action;

        public CaseElseConditional(CaseConditional<T> caseConditional, Action action)
        {
            CaseConditional = caseConditional;
            this.action = action;
            CaseConditional.RegisterElse(this);
        }

        public void Execute() => CaseConditional.Execute();
    }

    public class CaseEndConditional<T> : IEndConditional
    {
        private CaseConditional<T> caseConditional;

        public CaseEndConditional(CaseConditional<T> caseConditional)
        {
            this.caseConditional = caseConditional;
        }

        public void Execute() => caseConditional.Execute();
    }

    public interface IEndConditional
    {
        void Execute();
    }
    
    public static CaseConditional<T> Case<T>(T value) => new CaseConditional<T>(value);

    public static void Do(IEndConditional conditional) => conditional.Execute();
}
