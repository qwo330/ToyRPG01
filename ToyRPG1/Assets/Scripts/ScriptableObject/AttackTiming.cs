using System;
using UnityEngine;

[Serializable]
public struct AttackTiming
{
    public float InputStart;
    public float InputEnd;
    public float AttackEnd;
    public float RecoveryEnd;

    public float RecoveryDuration => Mathf.Max(0f, RecoveryEnd - AttackEnd);

    public AttackTiming(float inputStart, float inputEnd, float attackEnd, float recoveryEnd)
    {
        InputStart = inputStart;
        InputEnd = inputEnd;
        AttackEnd = attackEnd;
        RecoveryEnd = recoveryEnd;
    }

    public bool CanBufferInput(float elapsedTime)
    {
        return elapsedTime >= InputStart && elapsedTime <= InputEnd;
    }
}
