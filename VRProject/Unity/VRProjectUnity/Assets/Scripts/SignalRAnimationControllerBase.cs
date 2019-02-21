using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class SignalRAnimationControllerBase<TSignalRController, TArgs> : SignalRUnityControllerBase<TSignalRController, TArgs>
    where TSignalRController : SignalRController<TArgs>
{
    public Animator animator;

    private readonly HashSet<string> AnimationNames = new HashSet<string>();

    public override void AwakeVirtual()
    {
        foreach (var animationName in animator.runtimeAnimatorController.animationClips.Select(ac => ac.name))
        {
            AnimationNames.Add(animationName);
        }
    }

    public bool PlayAnimation(string animation)
    {
        if (AnimationNames.Contains(animation))
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => animator.Play(animation));
            return true;
        }
        else
        {
            Debug.LogError("got an animation command with an unknown animationName: " + animation);
            return false;
        }
    }

    public string DefaultAnimation = "Idle";
}