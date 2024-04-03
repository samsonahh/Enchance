using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleOfFireScript : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particle;
    [SerializeField] private SphereCollider _sphereCollider;
    
    [SerializeField] private float _duration = 5f;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private bool _counterClockWise = true;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, _radius);
    }

    private void Start()
    {
        var shape = _particle.shape;
        shape.radius = _radius;

        _rotationSpeed = _counterClockWise ? _rotationSpeed : -_rotationSpeed;

        Destroy(gameObject, _duration);
    }

    private void Update()
    {
        transform.position = PlayerController.Instance.transform.position + Vector3.up;

        _particle.transform.Rotate(0, _rotationSpeed * Time.deltaTime, 0);

        Collider[] insideMajorRadiusCollisions = Physics.OverlapSphere(transform.position, _radius);
        foreach(Collider collision in insideMajorRadiusCollisions)
        {
            if(IsTouchingPerimeter(collision))
            {
                if(collision.TryGetComponent(out EnemyController enemy))
                {
                    enemy.BurnEnemy(3);
                    enemy.BurnTicks = 3;
                }
                if (collision.TryGetComponent(out BossAI boss))
                {
                    boss.BurnEnemy(3);
                    boss.BurnTicks = 3;
                }
            }
        }
    }

    private bool IsTouchingPerimeter(Collider collider)
    {
        Vector3 closestPoint = _sphereCollider.ClosestPoint(collider.transform.position);

        float distanceToCenter = Vector3.Distance(_sphereCollider.transform.position, closestPoint);
        return Mathf.Approximately(distanceToCenter, _sphereCollider.radius);
    }
}
