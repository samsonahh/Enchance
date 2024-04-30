using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballScript : AbilityComponent
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private int _damage = 5;
    [SerializeField] private GameObject _splashPrefab;

    private Vector3 _targetPos;

    private void Start()
    {
        transform.position = _playerController.transform.position;
        _targetPos = transform.position + _castRadius * _lastForwardDir;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _targetPos, _speed * Time.deltaTime);

        if(Vector3.Distance(transform.position, _targetPos) <= 0.01f)
        {
            DoAreaDamage(_damage);

            Instantiate(_splashPrefab, transform.position + Vector3.up, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private void DoAreaDamage(int damage)
    {
        Collider[] collisions = Physics.OverlapSphere(transform.position + Vector3.up, 2f);

        if (collisions != null)
        {
            foreach (Collider collider in collisions)
            {
                if (collider.TryGetComponent(out BossAI bossAI))
                {
                    bossAI.TakeDamage(damage);
                }
                if (collider.TryGetComponent(out EnemyController enemy))
                {
                    enemy.TakeDamage(damage);
                }
                if (collider.TryGetComponent(out MagicBombScript bomb))
                {
                    bomb.Detonate();
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out BossAI boss))
        {
            DoAreaDamage(_damage);
            Instantiate(_splashPrefab, transform.position + Vector3.up, Quaternion.identity);
            Destroy(gameObject);
        }
        if (other.TryGetComponent(out EnemyController enemy))
        {
            DoAreaDamage(_damage);
            Instantiate(_splashPrefab, transform.position + Vector3.up, Quaternion.identity);
            Destroy(gameObject);
        }
        if (other.TryGetComponent(out MagicBombScript bomb))
        {
            DoAreaDamage(_damage);
            Instantiate(_splashPrefab, transform.position + Vector3.up, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}

