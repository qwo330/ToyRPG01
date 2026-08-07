using UnityEngine;

public readonly struct ActorSpawnEvent
{
    public ActorSpawnEvent(Actor actor, Vector3 position, Quaternion rotation, ActorSnapshot snapshot)
    {
        Actor = actor;
        Position = position;
        Rotation = rotation;
        Snapshot = snapshot;
    }

    public Actor Actor { get; }
    public Vector3 Position { get; }
    public Quaternion Rotation { get; }
    public ActorSnapshot Snapshot { get; }
}

public readonly struct ActorDespawnEvent
{
    public ActorDespawnEvent(Actor actor, ActorSnapshot snapshot)
    {
        Actor = actor;
        Snapshot = snapshot;
    }

    public Actor Actor { get; }
    public ActorSnapshot Snapshot { get; }
}

public readonly struct ActorHitEvent
{
    public ActorHitEvent(Actor actor, Actor attacker, int damage, int previousHP, int currentHP, int maxHP)
    {
        Actor = actor;
        Attacker = attacker;
        Damage = damage;
        PreviousHP = previousHP;
        CurrentHP = currentHP;
        MaxHP = maxHP;
    }

    public Actor Actor { get; }
    public Actor Attacker { get; }
    public int Damage { get; }
    public int PreviousHP { get; }
    public int CurrentHP { get; }
    public int MaxHP { get; }
}

public readonly struct ActorDeadEvent
{
    public ActorDeadEvent(Actor actor, Actor attacker, int damage, int previousHP, int currentHP, int maxHP)
    {
        Actor = actor;
        Attacker = attacker;
        Damage = damage;
        PreviousHP = previousHP;
        CurrentHP = currentHP;
        MaxHP = maxHP;
    }

    public Actor Actor { get; }
    public Actor Attacker { get; }
    public int Damage { get; }
    public int PreviousHP { get; }
    public int CurrentHP { get; }
    public int MaxHP { get; }
}

public readonly struct ActorHPChangedEvent
{
    public ActorHPChangedEvent(Actor actor, Actor source, int previousHP, int currentHP, int maxHP)
    {
        Actor = actor;
        Source = source;
        PreviousHP = previousHP;
        CurrentHP = currentHP;
        MaxHP = maxHP;
    }

    public Actor Actor { get; }
    public Actor Source { get; }
    public int PreviousHP { get; }
    public int CurrentHP { get; }
    public int MaxHP { get; }
}
