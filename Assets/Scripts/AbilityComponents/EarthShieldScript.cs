using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthShieldScript : MonoBehaviour
{
    [SerializeField] private int _shieldAmount;
    [SerializeField] private float _duration;

    private void Start()
    {
        GameManager.Instance.PlayerControllerInstance.ShieldPlayer(_shieldAmount, _duration);
    }
}
