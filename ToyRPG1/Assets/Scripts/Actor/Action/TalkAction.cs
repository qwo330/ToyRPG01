using UnityEngine;

public class TalkAction : MonoBehaviour, IInteract
{
    public ActorSnapshot Snapshot { get; }

    protected Actor Actor { get; private set; }

    void Awake()
    {
        if (!TryGetComponent(out Actor actor))
        {
            MyDebug.LogError($"Cannot find Actor component on {gameObject.name}");
            enabled = false;
            return;
        }

        Actor = actor;
    }
    
    public void Init(ActorData data)
    {
        throw new System.NotImplementedException();
    }

    public void Apply(ActorSnapshot snapshot)
    {
        throw new System.NotImplementedException();
    }

    public virtual bool CanProcess()
    {
        return isActiveAndEnabled &&  Actor.CanAct;
    }

    public void Process()
    {
        if (!CanProcess())
            return;

        throw new System.NotImplementedException();
    }
}
