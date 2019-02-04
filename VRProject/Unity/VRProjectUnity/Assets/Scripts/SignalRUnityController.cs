using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using Debug = UnityEngine.Debug;

public partial class SignalRUnityController : SignalRUnityControllerBase, IRemoteControllable
{
    void Start()
    {
        StartVirtual();

        Animator = gameObject.scene.GetRootGameObjects()
            .First(go => go.name == "avatar")
            .GetComponent<Animator>();

        foreach (var animationName in Animator.runtimeAnimatorController.animationClips.Select(ac => ac.name))
        {
            AnimationNames.Add(animationName);
        }        
    }

    private Animator Animator;
    private readonly HashSet<string> AnimationNames = new HashSet<string>();

    /// <summary>
    /// Audioquelle, von der aus Töne gespielt werden.
    /// </summary>
    public AudioSource audioSource;

    protected override void RegisterCommands(HubConnection hubConnection)
    {
        base.RegisterCommands(hubConnection);

        hubConnection.On<string>("Color", (colorString) => Color = TryParseColor(colorString, Color));
        hubConnection.On<string>("Animation", (animation) => PlayAnimation(animation));

        hubConnection.On<string, string, string>("Command", (connectionID, userName, message) =>
        {
            Debug.Log($"CommandCommandCommandCommandCommand {connectionID} {userName} '{message}'");

            var splittedPayload = message.Split('=');
            if (splittedPayload.Length == 2)
            {
                var command = splittedPayload[0];
                var value = splittedPayload[1];

                ExecuteCommand(command, value);
            }
            else
            {
                Debug.Log($"Command {message}");
            }
        });
    }

    private void ExecuteCommand(string command, string value)
    {
        DoCase(command,
            () => Debug.Log($"Unknown command '{command}'. (value = '{value}')"),
            ("Color", () => DoIf<string, Color>(value, TryParseColor, c => Color = c)),
            //("Farbe", () => DoIf(() => (TryParseColor(value, out var color), color), c => Color = c)),
            ("Animation", () => PlayAnimation(value)),
            ("Audio",
                () => DoCase(value, () => DoIf((value.EndsWith(".wav") || value.EndsWith(".ogg")) && File.Exists(value), () => PlayAudioFile(value)),
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
    
    #region parsing and conditioned execution

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
    
    #endregion

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

    /// <summary>
    /// Farbe parsen
    /// </summary>
    /// <param name="value"></param>
    /// <param name="color"></param>
    /// <returns></returns>
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

    private Color TryParseColor(string value, Color defaultColor) => TryParseColor(value, out var color) ? color : defaultColor;

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

    public void PlayAnimation(string animation)
    {
        if (AnimationNames.Contains(animation))
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => Animator.Play(animation));
        }
    }
    private const string DefaultAnimation = "Idle";

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

    /// <summary>
    /// Bekannte Farbnamen
    /// </summary>
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