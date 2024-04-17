using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/LevelReward", order = 1)]
public class LevelReward : ScriptableObject
{
    [field: SerializeField] public LevelUpReward RewardType { get; private set; }
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [TextArea(15, 20)]
    [field: SerializeField] public string Description;
}
