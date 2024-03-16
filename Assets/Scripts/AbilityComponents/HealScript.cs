using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealScript : MonoBehaviour
{
    [SerializeField] private int _healAmount;
    [SerializeField] private float _duration;

    private void Start()
    {
        PlayerController.Instance.Heal(_healAmount);
        PlayerController.Instance.BurnTicks = 0;

        Destroy(gameObject, _duration);
    }

    private void Update()
    {
        transform.position = PlayerController.Instance.transform.position;
    }
}
