using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwinkleToesScript : AbilityComponent
{
    [SerializeField] private float _additionalSpeed = 4f;
    [SerializeField] private float _duration = 3f;

    private void Start()
    {
        _playerController.ChangeCurrentMoveSpeed(_additionalSpeed + _playerController.PlayerRegularMoveSpeed, _duration);
        Destroy(gameObject, _duration);
    }

    private void Update()
    {
        transform.position = _playerController.transform.position;
    }
}
