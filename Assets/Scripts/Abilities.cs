using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Abilities", order = 1)]
public class Abilities : ScriptableObject
{
    [field: SerializeField] public Ability[] GameAbilities { get; private set; }
}
