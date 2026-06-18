using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageNumber Data", menuName = "ScriptableObject Object/DamageNumber Data", order = Int32.MaxValue)]
public class DamageNumberData : ScriptableObject
{
    [SerializeField] string skillUid;
    public string SkillUid => skillUid;

    [SerializeField] float lifeTime;
    public float LifeTime => lifeTime;

    [SerializeField] float duration;
    public float Duration => duration;
    
    
}
