using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallFromSkyComponent : MonoBehaviour
{
    [SerializeField] private float _height;
    [SerializeField] private float _speed;

    private void Start()
    {
        transform.position = PlayerController.Instance.LastCircleWorldPosition + _height * Vector3.up;
    }

    private void Update()
    {
        transform.Translate(Time.deltaTime * _speed * Vector3.down);
        _speed += 2f * Time.deltaTime;

        if (transform.position.y <= 0.05f)
        {
            Destroy(gameObject);
        }
    }
}
