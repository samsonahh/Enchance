using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleOfFireScript : AbilityComponent
{
    [SerializeField] private ParticleSystem _particle;
    
    [SerializeField] private float _duration = 5f;
    [SerializeField] private float _rotationSpeed = 5f;

    private bool _detectingCollisions = true;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, _castRadius);
    }

    private void Start()
    {
        var shape = _particle.shape;
        shape.radius = _castRadius;

        var vel = _particle.velocityOverLifetime;
        vel.orbitalY = _rotationSpeed;

        AddCompositeCollider(16);

        StartCoroutine(RingCoroutine());
    }

    private void Update()
    {
        if (_playerController == null) return;

        transform.position = _playerController.transform.position;
    }

    IEnumerator RingCoroutine()
    {
        yield return new WaitForSeconds(_duration);
        _detectingCollisions = false;
        _particle.Stop();
        while (_particle)
        {
            if (!_particle.IsAlive())
            {
                Destroy(gameObject);
            }
            yield return null;
        }
    }

    private void AddCompositeCollider(int spheres)
    {
        for(int i = 0; i < spheres; i++)
        {
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 1f;
            collider.center = new Vector3(_castRadius * Mathf.Cos(i * (2 * Mathf.PI / spheres)), 0, _castRadius * Mathf.Sin(i * (2 * Mathf.PI / spheres)));
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_detectingCollisions) return;

        if (other.TryGetComponent(out EnemyController enemy))
        {
            enemy.BurnEnemy(3);
            enemy.BurnTicks = 3;
        }
        if (other.TryGetComponent(out BossAI boss))
        {
            boss.BurnEnemy(3);
            boss.BurnTicks = 3;
        }
    }

}
