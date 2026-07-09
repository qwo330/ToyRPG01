using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public abstract class MoveController : MonoBehaviour, IMove
{
    public ActorSnapshot Snapshot { get; private set; }
    
    public float WalkSpeed => Data.WalkSpeed;
    public float RotSpeed => Data.RotSpeed;
    public float Acceleration => Data.Acceleration;
    
    protected ActorData Data { get; private set; }

    CharacterController controller;
    float gVelocity;
    Vector3 currentVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void Init(ActorData data)
    {
        Data = data;
    }

    public void Apply(ActorSnapshot snapshot)
    {
        Snapshot = snapshot;
    }

    public abstract void Process();

    protected void MoveTo(Vector3 targetPos, bool isRemote)
    {
        var currentPos = transform.position;
        var dx = targetPos.x - currentPos.x;
        var dz = targetPos.z - currentPos.z;
        var dy = targetPos.y - currentPos.y;
        
        var horizontalSqr = dx * dx + dz * dz;
        var verticalAbs = Mathf.Abs(dy);

        if (isRemote && LFIsRelocate())
        {
            transform.position = targetPos;
            gVelocity = 0f;
            currentVelocity = Vector3.zero;
            return;
        }
        
        var targetVelocity = Vector3.zero;
        if (isRemote)
        {
            if (horizontalSqr > GlobalValues.HMoveThresholdSqr)
            {
                targetVelocity = new Vector3(dx, 0, dz).normalized * WalkSpeed;
            }
        }
        else
        {
            targetVelocity = new Vector3(dx, 0, dz);
        }
        
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Acceleration * Time.deltaTime);

        if (IsGrounded())
        {
            // 허공답보를 막기 위한 보정
            gVelocity = GlobalValues.GroundedForce;
        }
        else
        {
            gVelocity += GlobalValues.Gravity * Time.deltaTime;
        }

        var motion = currentVelocity;
        motion.y = gVelocity;

        var actor = gameObject.GetComponentInParent<Actor>();
        MyDebug.Log($"Move: {motion}, actor id : {actor?.EntityID ?? 0}");
        controller.Move(motion * Time.deltaTime);
        return;

        bool LFIsRelocate()
        {
            return horizontalSqr > GlobalValues.RelocationThresholdSqr || verticalAbs > GlobalValues.VMoveThreshold;
        }
    }

    protected void Rotate(Quaternion targetRot)
    {
        var dot = Quaternion.Dot(transform.rotation, targetRot);
        if (dot >= GlobalValues.RotationDotThreshold)
            return;
        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, RotSpeed * Time.deltaTime);
    }

    bool IsGrounded() => controller.isGrounded && gVelocity < 0;
}

public class RemoteMoveController : MoveController
{
    public override void Process()
    {
        var worldPos = Snapshot.Position;
        
        MoveTo(worldPos, isRemote: true);
        Rotate(Snapshot.Rotation);
    }
}