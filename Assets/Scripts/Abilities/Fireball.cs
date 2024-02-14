using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : AbilityInstance
{
    [SerializeField] private float _speed;
    [SerializeField] private float _duration;
    private float _timer;
    private Vector3 _direction;

    public void SetDirection(Vector3 dir)
    {
        _direction = dir;
    }

    public void Update()
    {
        _timer += Time.deltaTime;

        transform.Translate(_speed * Time.deltaTime * _direction, Space.World);

        if (_timer > _duration)
            Destroy(gameObject);
    }
}
