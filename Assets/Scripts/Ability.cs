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
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public int MaxUseCount { get; private set; }
    [field: SerializeField] public float Cooldown { get; private set; }

    private void OnValidate()
    {
        Name = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this));
    }

    public virtual void Activate() { }
}
