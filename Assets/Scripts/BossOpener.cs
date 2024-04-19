using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossOpener : MonoBehaviour
{
    [SerializeField] private GameObject _bossDoor;
    [SerializeField] private Transform _enemiesParent;

    private void Start()
    {
        _bossDoor.SetActive(false);
    }

    private void Update()
    {
        foreach(Transform enemy in _enemiesParent)
        {
            if(enemy.gameObject != null)
            {
                _bossDoor.SetActive(false);
                return;
            }
        }

        _bossDoor.SetActive(true);
    }
}
