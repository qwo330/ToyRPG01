using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public abstract class MoveController : MonoBehaviour, IMove
{
    const float DefaultWalkSpeed = 6f;
    const float DefaultRotSpeed = 5f;
    const float DefaultAcceleration = 30f;

    public ActorSnapshot Snapshot { get; private set; }
    
    public float WalkSpeed => Data != null ? Data.WalkSpeed : DefaultWalkSpeed;
    public float RotSpeed => Data != null ? Data.RotSpeed : DefaultRotSpeed;
    public float Acceleration => Data != null ? Data.Acceleration : DefaultAcceleration;
    
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

    public virtual void Apply(ActorSnapshot snapshot)
    {
        Snapshot = snapshot;

        if (snapshot.State != ActorState.Move)
            ResetMotion();
    }

    public abstract void Process();

    protected void MoveTo(Vector3 targetPos, bool isRemote, bool allowRelocation = true)
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (controller == null)
            return;

        var currentPos = transform.position;
        var dx = targetPos.x - currentPos.x;
        var dz = targetPos.z - currentPos.z;
        var dy = targetPos.y - currentPos.y;
        
        var horizontalSqr = dx * dx + dz * dz;
        var verticalAbs = Mathf.Abs(dy);

        if (allowRelocation && isRemote && LFIsRelocate())
        {
            SyncTransform(targetPos, transform.rotation);
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

    protected void SyncTransform(Vector3 position, Quaternion rotation)
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (controller == null)
        {
            transform.SetPositionAndRotation(position, rotation);
            ResetMotion();
            return;
        }

        var wasEnabled = controller.enabled;
        if (wasEnabled)
            controller.enabled = false;

        transform.SetPositionAndRotation(position, rotation);

        if (wasEnabled)
            controller.enabled = true;

        ResetMotion();
    }

    protected void ResetMotion()
    {
        gVelocity = 0f;
        currentVelocity = Vector3.zero;
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
    [SerializeField] bool allowRelocation = true;

    public bool AllowRelocation
    {
        get => allowRelocation;
        set => allowRelocation = value;
    }

    public override void Apply(ActorSnapshot snapshot)
    {
        base.Apply(snapshot);

        if (snapshot.State != ActorState.Move)
            SyncTransform(snapshot.Position, snapshot.Rotation);
    }

    public override void Process()
    {
        var worldPos = Snapshot.State == ActorState.Move ? Snapshot.Position : transform.position;
        
        MoveTo(worldPos, isRemote: true, allowRelocation: allowRelocation);
        Rotate(Snapshot.Rotation);
    }
}
