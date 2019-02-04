using UnityEngine;

public interface IRemoteControllable
{
    Color Color { get; set; }
    string SoundFile { get; set; }
    void PlayAnimation(string animation);
}
