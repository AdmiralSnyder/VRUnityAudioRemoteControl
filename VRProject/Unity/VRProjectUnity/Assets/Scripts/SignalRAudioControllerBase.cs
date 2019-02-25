using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class SignalRAudioControllerBase<TSignalRController, TArgs> : SignalRUnityControllerBase<TSignalRController, TArgs>
    where TSignalRController : SignalREntityController<TArgs>
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

    private HashSet<string> ValidFileExtensions = new HashSet<string>
    {
        ".wav",
        ".ogg"
    };

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
                url = entity.entity.clientController.connectionManager.UsedSignalRServer + "/" + url;
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

    private bool PlayAudioFile(string fileName)
    {
        if (!ValidFileExtensions.Contains(Path.GetExtension(fileName)))
        {
            Debug.LogError("unknown file extension " + fileName);
            return false;
        }

        if (!File.Exists(fileName))
        {
            Debug.LogError("file doesn't exist " + fileName);
            return false;
        }

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
        return true;
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
