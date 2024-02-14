using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Ability", order = 1)]
public class Ability : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public int Star { get; private set; }
    [field: SerializeField] public Sprite IconSprite { get; private set; }
    [field: SerializeField] public int MaxUseCount { get; private set; }
    [HideInInspector] public int UseCount;
    [field: SerializeField] public float Cooldown { get; private set; }
    [HideInInspector] public float Timer;
    [HideInInspector] public bool OnCooldown;
    [field: SerializeField] public AbilityInstance AbilityPrefab { get; private set; }

    public Ability(Ability ability)
    {
        this.Name = ability.Name;
        this.Star = ability.Star;
        this.IconSprite = ability.IconSprite;
        this.MaxUseCount = ability.MaxUseCount;
        this.UseCount = ability.MaxUseCount;
        this.Cooldown = ability.Cooldown;
        this.Timer = 0;
        this.OnCooldown = false;
    }

    private void OnValidate()
    {
        Name = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this));
    }
}
