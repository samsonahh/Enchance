using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowWarpScript : AbilityComponent
{
    [SerializeField] private GameObject _poofPrefab;
    [SerializeField] private float _duration;

    private void Start()
    {
        transform.position = _playerController.transform.position;
        StartCoroutine(Poof());
    }

    IEnumerator Poof()
    {
        _playerController.IsInvincible = true;
        _playerController.IsVisible = false;
        _playerController.CanCast = false;

        yield return new WaitForSeconds(_duration);

        Instantiate(_poofPrefab, _playerController.transform.position, Quaternion.identity);

        _playerController.CanCast = true;
        _playerController.IsInvincible = false;
        _playerController.IsVisible = true;

        Destroy(gameObject);
        
    }
}
