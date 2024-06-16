using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealScript : AbilityComponent
{
    [SerializeField] private AudioClip _sfx;

    [SerializeField] private int _healAmount;
    [SerializeField] private float _duration;

    private void Start()
    {
        AudioSource.PlayClipAtPoint(_sfx, _playerController.transform.position);
        _playerController.Heal(_healAmount);

        Destroy(gameObject, _duration);
    }

    private void Update()
    {
        transform.position = _playerController.transform.position;
    }
}
