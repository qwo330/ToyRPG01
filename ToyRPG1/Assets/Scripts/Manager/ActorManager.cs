using System.Collections.Generic;
using UnityEngine.Pool;

public class ActorManager : SingletonMono<ActorManager>
{
    readonly Dictionary<int, Actor> entities = new();
    readonly List<ActorSnapshot> buffers = new();
    readonly List<SkillCommand> skillCommands = new();

    public int LocalPlayerID;
    
    void Update()
    {
        foreach (var s in buffers)
        {
            if (entities.TryGetValue(s.EntityID, out var actor))
            {
                actor.ApplySnapshot(s);
            }
        }
        
        buffers.Clear();

        foreach (var command in skillCommands)
        {
            if (entities.TryGetValue(command.SourceID, out var actor))
            {
                actor.ApplySkillCommand(command);
            }
        }

        skillCommands.Clear();

        using (ListPool<Actor>.Get(out var processingActors))
        {
            processingActors.AddRange(entities.Values);
            foreach (var actor in processingActors)
            {
                if (actor != null && entities.ContainsKey(actor.EntityID))
                    actor.ProcessActions();
            }
        }
    }

    public override void Reset()
    {
        buffers.Clear();
        skillCommands.Clear();
        LocalPlayerID = 0;
    }
    
    public void AddActor(Actor actor)
    {
        if (entities.TryAdd(actor.EntityID, actor))
        {
            MyDebug.Log($"Add Actor, ID: {actor.EntityID}");
        }
    }

    public void RemoveActor(Actor actor)
    {
        if (entities.Remove(actor.EntityID))
        {
            MyDebug.Log($"Remove Actor, ID: {actor.EntityID}");
        }
    }

    public Actor GetActor(int entityID)
    {
        return entities.GetValueOrDefault(entityID);
    }

    public Actor GetLocalPlayer() => GetActor(LocalPlayerID);
    
    public void AddSnapshots(List<ActorSnapshot> snapshots)
    {
        foreach (var s  in snapshots)
        {
            AddSnapshot(s);
        }
    }
    
    public bool AddSnapshot(ActorSnapshot s)
    {
        if (entities.ContainsKey(s.EntityID) == false)
        {
            MyDebug.LogWarning($"Cannot find entity ID: {s.EntityID}");
            return false;
        }

        buffers.Add(s);
        return true;
    }

    public bool AddSkillCommand(SkillCommand command)
    {
        if (entities.ContainsKey(command.SourceID) == false)
        {
            MyDebug.LogWarning($"Cannot find entity ID: {command.SourceID}");
            return false;
        }

        skillCommands.Add(command);
        return true;
    }
}
