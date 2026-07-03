using UnityEngine;

public class ActorAnimator :  AnimationController
{
    static readonly int HashState = Animator.StringToHash("State");
    static readonly int HashCombo = Animator.StringToHash("Combo");

    Actor actor;
    
    void Start()
    {
        actor = GetComponentInParent<Actor>();
    }

    void LateUpdate()
    {
        if (actor == null)
            return;
        
        UpdateAnimation(actor.Snapshot);
    }
    
    public override void UpdateAnimation(ActorSnapshot snapshot)
    {
        Animator.SetFloat(HashState, (float)snapshot.State);
        Animator.SetInteger(HashCombo, snapshot.Combo);
    }
    
    bool IsMoving(ActorState state) => state == ActorState.Move;
    
    public void Init()
    {
    }
}