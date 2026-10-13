using UnityEngine;

public readonly struct DamageInfo
{
    public DamageInfo(Actor attacker, int amount, Vector3 hitPoint)
    {
        Attacker = attacker;
        Amount = amount;
        HitPoint = hitPoint;
    }

    public Actor Attacker { get; }
    public int Amount { get; }
    public Vector3 HitPoint { get; }
}
