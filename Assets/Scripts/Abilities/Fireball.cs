using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : AbilityInstance
{
    [SerializeField] private float _speed;
    private Vector3 _direction;
    private float _distance;

    public void SetupFireball(Vector3 dir, float dist)
    {
        _direction = dir;
        _distance = dist;
    }

    private void FixedUpdate()
    {
        transform.Translate(_speed * Time.deltaTime * _direction);
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, _abilityCaster.transform.position) > _distance)
        {
            Destroy(gameObject);
        }
    }
}
