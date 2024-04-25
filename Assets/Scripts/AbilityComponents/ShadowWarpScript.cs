using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowWarpScript : MonoBehaviour
{
    [SerializeField] private GameObject _poofPrefab;
    [SerializeField] private float _duration;

    private void Start()
    {
        transform.position = GameManager.Instance.PlayerControllerInstance.transform.position;
        StartCoroutine(Poof());
    }

    IEnumerator Poof()
    {
        GameManager.Instance.PlayerControllerInstance.IsInvincible = true;
        GameManager.Instance.PlayerControllerInstance.IsVisible = false;
        GameManager.Instance.PlayerControllerInstance.CanCast = false;

        yield return new WaitForSeconds(_duration);

        Instantiate(_poofPrefab, GameManager.Instance.PlayerControllerInstance.transform.position, Quaternion.identity);

        GameManager.Instance.PlayerControllerInstance.CanCast = true;
        GameManager.Instance.PlayerControllerInstance.IsInvincible = false;
        GameManager.Instance.PlayerControllerInstance.IsVisible = true;

        Destroy(gameObject);
        
    }
}
