using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealScript : MonoBehaviour
{
    [SerializeField] private AudioClip _sfx;

    [SerializeField] private int _healAmount;
    [SerializeField] private float _duration;

    private void Start()
    {
        AudioSource.PlayClipAtPoint(_sfx, PlayerController.Instance.transform.position);
        PlayerController.Instance.Heal(_healAmount);
        PlayerController.Instance.BurnTicks = 0;
        if(PlayerController.Instance.PlayerRegularMoveSpeed > PlayerController.Instance.PlayerCurrentMoveSpeed)
        {
            PlayerController.Instance.ChangeCurrentMoveSpeed(PlayerController.Instance.PlayerRegularMoveSpeed, 0.1f);
        }

        Destroy(gameObject, _duration);
    }

    private void Update()
    {
        transform.position = PlayerController.Instance.transform.position;
    }
}
