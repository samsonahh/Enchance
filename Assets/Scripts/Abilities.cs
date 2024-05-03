using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Abilities", order = 1)]
public class Abilities : ScriptableObject
{
    [field: SerializeField] public Ability[] GameAbilities { get; private set; }
    [HideInInspector] public List<Ability>[] StarSortedAbilities { get; private set; } = { new List<Ability>(), new List<Ability>(), new List<Ability>() };
}
