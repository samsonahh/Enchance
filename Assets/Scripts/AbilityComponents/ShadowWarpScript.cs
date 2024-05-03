using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowWarpScript : MonoBehaviour
{
    [SerializeField] private GameObject _poofPrefab;
    [SerializeField] private float _duration;

    private void Start()
    {
        transform.position = PlayerController.Instance.transform.position;
        StartCoroutine(Poof());
    }

    IEnumerator Poof()
    {
        PlayerController.Instance.IsInvincible = true;
        PlayerController.Instance.IsVisible = false;
        PlayerController.Instance.CanCast = false;

        yield return new WaitForSeconds(_duration);

        Instantiate(_poofPrefab, PlayerController.Instance.transform.position, Quaternion.identity);

        PlayerController.Instance.CanCast = true;
        PlayerController.Instance.IsInvincible = false;
        PlayerController.Instance.IsVisible = true;

        Destroy(gameObject);
        
    }
}
