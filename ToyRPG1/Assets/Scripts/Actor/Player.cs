using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class Player : Actor
{
    [SerializeField] bool localPlayer;
    [SerializeField] MoveController moveController;

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
        gameObject.SetActive(false);
    }
}
