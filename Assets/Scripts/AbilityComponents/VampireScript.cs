using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VampireScript : MonoBehaviour
{
    [SerializeField] private float _duration;

    private void Start()
    {
        PlayerController.Instance.EnableLifeSteal(_duration);

        Destroy(gameObject, _duration);
    }

    private void Update()
    {
        transform.position = PlayerController.Instance.transform.position;
    }
}
