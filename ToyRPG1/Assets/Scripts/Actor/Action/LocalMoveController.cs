public class LocalMoveController : MoveController
{
    public override void Process()
    {
        var pseudoPos = transform.position + (Snapshot.Position * WalkSpeed);
        
        MoveTo(pseudoPos, isRemote: false);
        Rotate(Snapshot.Rotation);
    }
}