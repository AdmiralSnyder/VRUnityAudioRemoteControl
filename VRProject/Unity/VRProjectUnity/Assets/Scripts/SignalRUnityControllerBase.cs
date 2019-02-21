using UnityEngine;

public abstract class SignalRUnityControllerBase<TSignalRController, TArgs> : MonoBehaviour
    where TSignalRController : SignalRController<TArgs>
{
    public abstract void OnInitEvent(object sender, EventArgs<TArgs> args);

    public void Awake()
    {
        AwakeVirtual();
        InitController();
    }

    public virtual void AwakeVirtual() { }

    /// <summary>
    /// Hier werden die Registrierungs-Events des Controllers angekabelt.
    /// </summary>
    public void InitController() => controller.Init(OnInitEvent);

    public TSignalRController controller;
}
