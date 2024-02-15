using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : AbilityInstance
{
    [SerializeField] private float _speed;
    private Vector3 _direction;

    public void SetDirection(Vector3 dir)
    {
        _direction = dir;
    }

    private void FixedUpdate()
    {
        transform.Translate(_speed * Time.deltaTime * _direction);
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, _abilityCaster.transform.position) > _abilityCaster.CastRadius)
        {
            Destroy(gameObject);
        }
    }
}
