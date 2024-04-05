using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleOfFireScript : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particle;
    
    [SerializeField] private float _duration = 5f;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private float _rotationSpeed = 5f;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, _radius);
    }

    private void Start()
    {
        var shape = _particle.shape;
        shape.radius = _radius;

        var vel = _particle.velocityOverLifetime;
        vel.orbitalY = _rotationSpeed;

        AddCompositeCollider(32);

        Destroy(gameObject, _duration);
    }

    private void Update()
    {
        if (PlayerController.Instance == null) return;

        transform.position = PlayerController.Instance.transform.position;
    }

    private void AddCompositeCollider(int spheres)
    {
        for(int i = 0; i < spheres; i++)
        {
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.5f;
            collider.center = new Vector3(_radius * Mathf.Cos(i * (2 * Mathf.PI / spheres)), 0, _radius * Mathf.Sin(i * (2 * Mathf.PI / spheres)));
        }
    }

    private void OnTriggerStay(Collider other)
    {
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
