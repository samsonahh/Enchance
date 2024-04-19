using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCollectable : MonoBehaviour
{
    [SerializeField] private RequiredKeys _keyType;


    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out PlayerController player))
        {
            GameManager.Instance.BossRoomKeys[(int)_keyType] = 1;
            Destroy(gameObject);
        }
    }
}

public enum RequiredKeys
{
    Grass,
    Fire,
    Ice
}