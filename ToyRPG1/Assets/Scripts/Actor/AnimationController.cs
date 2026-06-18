using UnityEngine;

public abstract class AnimationController : MonoBehaviour
{
    protected Animator Animator { get; private set; } 

    protected void Awake()
    {
        Animator = GetComponentInChildren<Animator>();

        if (Animator == null)
        {
            MyDebug.LogError($"Cannot find Animator component on {gameObject.name}");
        }
    }
    
    public abstract void UpdateAnimation(ActorSnapshot snapshot);
}

public enum ActorState : byte
{
    Idle = 0,
    Move = 1,
    Jump = 2,
    Hit = 9,
    Dead = 10,
    Attack = 100,
}