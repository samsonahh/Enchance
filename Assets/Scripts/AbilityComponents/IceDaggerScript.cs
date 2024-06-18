using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceDaggerScript : AbilityComponent
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private int _damage = 2;
    [SerializeField] private GameObject _shatterPrefab;

    [SerializeField] private AudioClip _iceShardStartSFX;
    [SerializeField] private AudioClip _iceShardImpactSFX;

    private Vector3 _target;

    private void Start()
    {
        transform.position = _caster.position;
        _target = (_lastTargetWorldPosition - transform.position).normalized * _castRadius + transform.position;

        // AudioSource.PlayClipAtPoint(_iceShardStartSFX, transform.position, 2);
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _target, _speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _target) <= 0.01f)
        {
            //Instantiate(_shatterPrefab, transform.position + Vector3.up, Quaternion.identity);

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hostile)
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                //Instantiate(_shatterPrefab, transform.position + Vector3.up, Quaternion.identity);

                player.TakeDamage(_damage);

                Destroy(gameObject);
            }
        }
        else
        {
            if (other.TryGetComponent(out BossAI boss))
            {
                //Instantiate(_shatterPrefab, transform.position + Vector3.up, Quaternion.identity);

                boss.TakeDamage(_damage);

                Destroy(gameObject);
            }
            if (other.TryGetComponent(out EnemyController enemy))
            {
                //Instantiate(_shatterPrefab, transform.position + Vector3.up, Quaternion.identity);

                enemy.TakeDamage(_damage);

                Destroy(gameObject);
            }
        }
    }
}
