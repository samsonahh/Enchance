using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballScript : AbilityComponent
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private int _damage = 5;
    [SerializeField] private GameObject _splashPrefab;

    [SerializeField] private AudioClip _fireballStartSFX;
    [SerializeField] private AudioClip _fireballExplodeSFX;

    private AudioSource _fireballInAirAudioSource;
    private Vector3 _target;

    private void Start()
    {
        transform.position = _playerController.transform.position;
        _target = transform.position + _castRadius * _lastForwardDirection;

        AudioSource.PlayClipAtPoint(_fireballStartSFX, transform.position);
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _target, _speed * Time.deltaTime);

        if(Vector3.Distance(transform.position, _target) <= 0.01f)
        {
            Explode();
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
            Explode();
        }
        if (other.TryGetComponent(out EnemyController enemy))
        {
            Explode();
        }
        if (other.TryGetComponent(out MagicBombScript bomb))
        {
            Explode();
        }
    }

    private void Explode()
    {
        AudioSource.PlayClipAtPoint(_fireballExplodeSFX, transform.position + Vector3.up);

        DoAreaDamage(_damage);
        Instantiate(_splashPrefab, transform.position + Vector3.up, Quaternion.identity);
        Destroy(gameObject);
    }
}

