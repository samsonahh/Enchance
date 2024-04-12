using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossOpener : MonoBehaviour
{
    [SerializeField] private GameObject[] _requiredEnemies;
    [SerializeField] private GameObject _bossDoor;

    private void Start()
    {
        _bossDoor.SetActive(false);
    }

    private void Update()
    {
        foreach(GameObject enemy in _requiredEnemies)
        {
            if(enemy != null)
            {
                _bossDoor.SetActive(false);
                return;
            }
        }

        _bossDoor.SetActive(true);
    }
}
