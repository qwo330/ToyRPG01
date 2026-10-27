using UnityEngine;

public class AnimationEventDispatcher : MonoBehaviour
{
    IAttack _attack;
    Actor _actor;

    public void Initialize(GameObject actionOwner)
    {
        _attack = actionOwner.GetComponent<IAttack>();
        _actor = actionOwner.GetComponent<Actor>();
    }

    public void OnNormalAttackEvent()
    {
        if (_actor != null && _actor.IsDead)
            return;

        _attack?.OnNormalAttackEvent();
    }

    public void OnDeathAnimationEndEvent()
    {
        _actor?.OnDeathAnimationEndEvent();
    }
}
