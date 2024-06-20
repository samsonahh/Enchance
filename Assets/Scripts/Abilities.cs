using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Abilities", order = 1)]
public class Abilities : ScriptableObject
{
    [field: SerializeField] public Ability[] GameAbilities { get; private set; }
    [HideInInspector] public List<Ability>[] StarSortedAbilities { get; private set; } = { new List<Ability>(), new List<Ability>(), new List<Ability>() };

    private void Awake()
    {
        
    }

    public void ClearStarSortedAbilities()
    {
        StarSortedAbilities = new List<Ability>[3];
        StarSortedAbilities[0] = new List<Ability>();
        StarSortedAbilities[1] = new List<Ability>();
        StarSortedAbilities[2] = new List<Ability>();
    }
}
