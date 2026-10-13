using UnityEngine;

public class AnimationEventDispatcher : MonoBehaviour
{
    IAttack _attack;

    public void Initialize(GameObject actionOwner)
    {
        _attack = actionOwner.GetComponent<IAttack>();
    }

    public void OnNormalAttackEvent()
    {
        _attack?.OnNormalAttackEvent();
    }
}
