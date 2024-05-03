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
        AudioSource.PlayClipAtPoint(_sfx, GameManager.Instance.PlayerControllerInstance.transform.position);
        GameManager.Instance.PlayerControllerInstance.Heal(_healAmount);
/*        GameManager.Instance.PlayerControllerInstance.BurnTicks = 0;
        if(GameManager.Instance.PlayerControllerInstance.PlayerRegularMoveSpeed > GameManager.Instance.PlayerControllerInstance.PlayerCurrentMoveSpeed)
        {
            GameManager.Instance.PlayerControllerInstance.ChangeCurrentMoveSpeed(GameManager.Instance.PlayerControllerInstance.PlayerRegularMoveSpeed, 0.1f);
        }*/

        Destroy(gameObject, _duration);
    }

    private void Update()
    {
        transform.position = GameManager.Instance.PlayerControllerInstance.transform.position;
    }
}
