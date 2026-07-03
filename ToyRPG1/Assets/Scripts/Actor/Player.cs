using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class Player : Actor
{
    [SerializeField] bool localPlayer;
    [SerializeField] PlayerData data;
    [SerializeField] MoveController moveController;

    public PlayerData Data => data;

    protected override void Init()
    {
        if (moveController == null)
        {
            if (localPlayer)
                moveController = GetComponent<LocalMoveController>();
            else
                moveController = GetComponent<RemoteMoveController>();
        }
        
        moveController.Init(data);
        base.Init();
    }

    public override void Dead()
    {
        throw new System.NotImplementedException();
    }
}
