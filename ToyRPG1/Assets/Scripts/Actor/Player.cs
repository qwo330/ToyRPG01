using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class Player : Actor
{
    [SerializeField] bool localPlayer;
    [SerializeField] MoveController moveController;

    void Start()
    {
        Spawn();
    }

    public override void Spawn()
    {
        base.Spawn();
        ActorManager.Instance.LocalPlayerID = EntityID;
    }

    protected override void Init()
    {
        if (!TryGetComponent<AttackAction>(out _))
            MyDebug.LogError($"Cannot find AttackAction component on {gameObject.name}");

        if (moveController == null)
        {
            if (localPlayer)
                moveController = GetComponent<LocalMoveController>();
            else
                moveController = GetComponent<RemoteMoveController>();
        }

        base.Init();
    }

    protected override void OnDeathAnimationEnd()
    {
        Despawn();
        gameObject.SetActive(false);
    }
}
