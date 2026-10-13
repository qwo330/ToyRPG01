using UnityEngine;

public struct ComboAttackState
{
    static readonly AttackTiming[] DefaultTimings =
    {
        new(0.25f, 0.42f, 0.5f, 0.7f),
        new(0.25f, 0.42f, 0.55f, 0.75f),
        new(0f, 0f, 0.65f, 0.85f)
    };

    public int Combo { get; private set; }
    public bool IsAttacking => Combo > 0;
    public bool IsComboStartedThisTick { get; private set; }

    AttackTiming[] Timings => timings != null && timings.Length > 0 ? timings : DefaultTimings;
    int MaxCombo => Timings.Length;
    bool HasNextCombo => Combo < MaxCombo;

    AttackTiming[] timings;
    float elapsedTime;
    float recoveryTime;
    bool isNextComboBuffered;

    public void Init(AttackTiming[] attackTimings)
    {
        timings = attackTimings;
        Reset();
    }

    public void Tick(bool attackPressed, float deltaTime)
    {
        IsComboStartedThisTick = false;

        recoveryTime -= deltaTime;
        if (recoveryTime > 0f)
            return;

        if (!IsAttacking)
        {
            if (attackPressed)
                Begin(1);

            return;
        }

        elapsedTime += deltaTime;

        var timing = GetTiming(Combo);

        if (attackPressed && HasNextCombo && timing.CanBufferInput(elapsedTime))
            isNextComboBuffered = true;

        if (elapsedTime < timing.AttackEnd)
            return;

        if (isNextComboBuffered && HasNextCombo)
        {
            Begin(Combo + 1);
            return;
        }

        Reset(timing.RecoveryDuration);
    }

    AttackTiming GetTiming(int combo)
    {
        var index = Mathf.Clamp(combo - 1, 0, Timings.Length - 1);
        return Timings[index];
    }

    void Begin(int combo)
    {
        Combo = combo;
        IsComboStartedThisTick = true;
        elapsedTime = 0f;
        recoveryTime = 0f;
        isNextComboBuffered = false;
    }

    void Reset(float recoveryDuration = 0f)
    {
        Combo = 0;
        elapsedTime = 0f;
        recoveryTime = recoveryDuration;
        isNextComboBuffered = false;
    }
}
