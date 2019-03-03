using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Generische abstrakte Basisklasse für das Abspielen von Audioquellen
/// </summary>
/// <typeparam name="TEntityController"></typeparam>
/// <typeparam name="TArgs"></typeparam>
public abstract class SignalRAudioBase<TEntityController, TArgs> : SignalRUnityFeatureBase<TEntityController, TArgs>
    where TEntityController : SignalREntityController<TArgs>
{
    /// <summary>
    /// Audioquelle, von der aus Töne gespielt werden.
    /// </summary>
    public AudioSource audioSource;

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
                PlayAudio(_SoundFile);
            }
        }
    }
    private string _SoundFile;

    /// <summary>
    /// Stummschalten der Audioquelle
    /// </summary>
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

    /// <summary>
    /// Mit diesen Dateiendungen können wir umgehen.
    /// </summary>
    private HashSet<string> ValidFileExtensions = new HashSet<string>
    {
        ".wav",
        ".ogg"
    };

    /// <summary>
    /// Abspielen von Audiodateien
    /// </summary>
    /// <param name="url"></param>
    private void PlayAudio(string url)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            if (File.Exists(url))
            {
                url = "file://" + url;
            }
            //UnityWebRequest uwr = new UnityWebRequest(link);
            //var data = uwr.downloadHandler.data;
            //AudioClip clip2 = AudioClip.Create( new AudioClip();
            else if (!url.StartsWith("http")) // HACK - rausbekommen, ob das wirklich ein relativer URL ist.
            {
                url = entityController.entity.clientController.connectionManager.UsedSignalRServer + "/" + url;
            }
            WWW www1 = new WWW(url);
            AudioClip clip = www1.GetAudioClip(false, true/*AudioType.OGGVORBIS*/);
            //if (clip.isReadyToPlay)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        });
    }

    public void HandleAudioCommand(string audioCommand)
    {
        switch (audioCommand)
        {
            case "stop": StopAudio(); break;
            case "mute": Muted = true; break;
            case "unmute": Muted = false; break;
            default: PlayAudio(audioCommand); break;
        }
    }

    private void StopAudio()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            audioSource.Stop();
        });
    }
}
