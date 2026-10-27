using R3;
using UnityEngine;

public class ActorAnimator :  AnimationController
{
    static readonly int HashState = Animator.StringToHash("State");
    static readonly int HashCombo = Animator.StringToHash("Combo");
    static readonly int HashHit = Animator.StringToHash("Hit");
    static readonly int HashDie = Animator.StringToHash("Die");

    protected virtual bool CanPlayAnimation =>
        isActiveAndEnabled && Animator != null && Animator.isActiveAndEnabled;

    Actor actor;

    void OnEnable()
    {
        if (Animator != null && Animator.runtimeAnimatorController != null)
            Animator.Rebind();
    }
    
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

    public bool HasDeathAnimationEndEvent()
    {
        if (Animator == null || Animator.runtimeAnimatorController == null)
            return false;

        foreach (var clip in Animator.runtimeAnimatorController.animationClips)
        {
            if (clip == null)
                continue;

            foreach (var animationEvent in clip.events)
            {
                if (animationEvent.functionName == nameof(AnimationEventDispatcher.OnDeathAnimationEndEvent))
                    return true;
            }
        }

        return false;
    }
    
    bool IsMoving(ActorState state) => state == ActorState.Move;
    
    public void Init()
    {
        actor = GetComponentInParent<Actor>();

        if (Animator == null || actor == null)
            return;

        if (Animator.TryGetComponent(out AnimationEventDispatcher dispatcher) == false)
            dispatcher = Animator.gameObject.AddComponent<AnimationEventDispatcher>();
        dispatcher.Initialize(actor.gameObject);

        Observable.Merge(actor.OnHit.Select(_ => HashHit), actor.OnDead.Select(_ => HashDie))
            .Where(_ => CanPlayAnimation)
            .Subscribe(TriggerAnimation)
            .AddTo(this);
    }

    void TriggerAnimation(int triggerHash)
    {
        if (triggerHash == HashDie)
            Animator.ResetTrigger(HashHit);

        Animator.ResetTrigger(triggerHash);
        Animator.SetTrigger(triggerHash);
    }
}
