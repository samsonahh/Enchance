using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastOnSelfComponent : MonoBehaviour
{
    [SerializeField] private float _duration;

    private void Start()
    {
        transform.position = PlayerController.Instance.transform.position;
        Destroy(gameObject, _duration);
    }

    private void Update()
    {
    }
}
