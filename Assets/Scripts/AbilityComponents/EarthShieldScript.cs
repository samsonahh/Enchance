using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthShieldScript : MonoBehaviour
{
    [SerializeField] private int _shieldAmount;
    [SerializeField] private float _duration;

    private void Start()
    {
        PlayerController.Instance.ShieldPlayer(_shieldAmount, _duration);

        Destroy(gameObject, _duration);
    }

    private void Update()
    {
        transform.position = PlayerController.Instance.transform.position;
    }
}