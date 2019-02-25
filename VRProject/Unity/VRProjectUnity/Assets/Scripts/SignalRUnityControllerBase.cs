using UnityEngine;

public abstract class SignalRUnityControllerBase : MonoBehaviour
{
    public void Awake() => AwakeVirtual();
    public virtual void AwakeVirtual() { }

    public virtual void Connected(Microsoft.AspNetCore.SignalR.Client.HubConnection hubConnection) { }
}

public abstract class SignalRUnityControllerBase<TSignalRController, TArgs> : SignalRUnityControllerBase
    where TSignalRController : SignalREntityController<TArgs>
{
    public abstract void OnInitEvent(object sender, EventArgs<TArgs> args);


    public override void AwakeVirtual()
    {
        base.AwakeVirtual();
        InitController();
        entity.EventControllers.Add(this);
    }

    /// <summary>
    /// Hier werden die Registrierungs-Events des Controllers angekabelt.
    /// </summary>
    public void InitController() => entity.Init(OnInitEvent);

    public TSignalRController entity;
}
