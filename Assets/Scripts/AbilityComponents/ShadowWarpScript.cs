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
        PlayerController.Instance.CanCast = false;

        for (float timer = 0f; timer < _duration; timer+= Time.deltaTime)
        {
            PlayerController.Instance.StopPlayer();
            PlayerController.Instance.transform.position = transform.position;
            yield return null;
        }

        Instantiate(_poofPrefab, transform.position, Quaternion.identity);
        PlayerController.Instance.CanCast = true;
        PlayerController.Instance.IsInvincible = false;

        Destroy(gameObject);
    }
}
