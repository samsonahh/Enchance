using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Ability", order = 1)]
public class Ability : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; } = "";
    [field: SerializeField] public int Star { get; private set; }
    [field: SerializeField] public Sprite IconSprite { get; private set; }
    [field: SerializeField] public int MaxUseCount { get; private set; }
    [HideInInspector] public int UseCount;
    [field: SerializeField] public float Cooldown { get; private set; }
    [HideInInspector] public float Timer;
    [HideInInspector] public bool OnCooldown;
    [field: SerializeField] public float CastRadius { get; private set; }
    [field: SerializeField] public float CircleCastRadius { get; private set; }
    [field: SerializeField] public AbilityInstance AbilityPrefab { get; private set; }
    [field: SerializeField] public AbilityType AbilityType { get; private set; }

    public static Ability CopyAbility(Ability ability)
    {
        Ability a = CreateInstance<Ability>();
        a.Name = ability.Name;
        a.Star = ability.Star;
        a.IconSprite = ability.IconSprite;
        a.MaxUseCount = ability.MaxUseCount;
        a.UseCount = ability.MaxUseCount;
        a.Cooldown = ability.Cooldown;
        a.Timer = 0;
        a.OnCooldown = false;
        a.CastRadius = ability.CastRadius;
        a.CircleCastRadius = ability.CircleCastRadius;
        a.AbilityPrefab = ability.AbilityPrefab;
        a.AbilityType = ability.AbilityType;

        return a;
    }

    private void OnValidate()
    {
        Name = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this));
    }
}
