public class LocalMoveController : MoveController
{
    public override void Process()
    {
        var moveDirection = Snapshot.State == ActorState.Move ? Snapshot.Position : UnityEngine.Vector3.zero;
        var pseudoPos = transform.position + (moveDirection * WalkSpeed);
        
        MoveTo(pseudoPos, isRemote: false);
        Rotate(Snapshot.Rotation);
    }
}
