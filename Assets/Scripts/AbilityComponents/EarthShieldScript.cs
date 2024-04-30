using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthShieldScript : AbilityComponent
{
    [SerializeField] private int _shieldAmount;
    [SerializeField] private float _duration;

    private void Start()
    {
        _playerController.ShieldPlayer(_shieldAmount, _duration);
    }
}
