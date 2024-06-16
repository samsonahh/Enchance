using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VampireScript : AbilityComponent
{
    [SerializeField] private float _duration;

    private void Start()
    {
        _playerController.EnableLifeSteal(_duration);

        Destroy(gameObject, _duration);
    }

    private void Update()
    {
        transform.position = _playerController.transform.position;
    }
}
