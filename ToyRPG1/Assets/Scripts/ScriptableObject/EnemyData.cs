using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "ScriptableObject Object/Enemy Data")]
public class EnemyData : ActorData
{
    public float RoamRadius = 5f;
    public float DetectRadius = 10f;
    public float LeashRange = 15f;
    public LayerMask TargetLayers = GameLayers.PlayerMask;

    public float NextRoamingTime;
}
