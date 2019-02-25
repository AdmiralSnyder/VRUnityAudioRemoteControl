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

    private HashSet<string> ValidFileExtensions = new HashSet<string>
    {
        ".wav",
        ".ogg"
    };

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

    public bool HandleAudioCommand(string audioCommand)
    {
        switch (audioCommand)
        {
            case "stop": StopAudio(); break;
            case "mute": Muted = true; break;
            case "unmute": Muted = false; break;
            default: return PlayAudioFile(audioCommand);
        }
        return true;
    }

    private void StopAudio()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            audioSource.Stop();
        });
    }
}
