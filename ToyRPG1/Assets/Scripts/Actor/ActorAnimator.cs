using UnityEngine;

public class ActorAnimator :  AnimationController
{
    static readonly int HashState = Animator.StringToHash("State");
    static readonly int HashCombo = Animator.StringToHash("Combo");
    static readonly int HashAttackTrigger = Animator.StringToHash("AttackTrigger");

    Actor actor;
    PlayerInput playerInput;
    int prevCombo;
    
    void Start()
    {
        actor = GetComponentInParent<Actor>();

        if (actor is Player)
        {
            playerInput = GetComponentInParent<PlayerInput>();
        }
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

        if (snapshot.Combo > prevCombo)
        {
            Animator.SetTrigger(HashAttackTrigger);
        }
        
        prevCombo = snapshot.Combo;
    }

    public void OnAttackAnimationEnd()
    {
        if (playerInput == null)
            return;

        playerInput.ResetAttackState();
        prevCombo = 0;
    }
    
    bool IsMoving(ActorState state) => state == ActorState.Move;
    
    public void Init()
    {
    }
}