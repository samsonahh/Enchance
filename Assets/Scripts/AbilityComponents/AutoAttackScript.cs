using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAttackScript : AbilityComponent
{
    [SerializeField] private ParticleSystem _particle;
    [SerializeField] private GameObject _wandSplashPrefab;

    [SerializeField] private int _damage = 1;
    [SerializeField] private float _speed = 10f;

    private bool _detectingCollisions = true;

    private void Start()
    {
        transform.position = GameObject.Find("StaffEffects").transform.localPosition + _playerController.transform.position;

        _playerController.AutoAttacking = true;
    }

    private void Update()
    {
        if (_lastTarget == null)
        {
            _playerController.AutoAttacking = false;
            Destroy(gameObject);
            return;
        }

        if (!_detectingCollisions) return;

        transform.position = Vector3.MoveTowards(transform.position, _lastTarget.transform.position + Vector3.up, _speed * Time.deltaTime);

        if(Vector3.Distance(transform.position, _lastTarget.transform.position + Vector3.up) <= 0.01f)
        {
            if(_lastTarget.TryGetComponent(out EnemyController enemy))
            {
                if (_playerController.LifeSteal)
                {
                    _playerController.Heal(1);
                }

                enemy.TakeDamage(_damage);
                _playerController.AutoAttacking = false;
                Instantiate(_wandSplashPrefab, transform.position, Quaternion.identity);
                _detectingCollisions = false;
                StartCoroutine(DestroySelfCoroutine());
            }
            if (_lastTarget.TryGetComponent(out BossAI boss))
            {
                if (_playerController.LifeSteal)
                {
                    _playerController.Heal(1);
                }

                boss.TakeDamage(_damage);
                _playerController.AutoAttacking = false;
                Instantiate(_wandSplashPrefab, transform.position, Quaternion.identity);
                _detectingCollisions = false;
                StartCoroutine(DestroySelfCoroutine());
            }
        }
    }

    IEnumerator DestroySelfCoroutine()
    {
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

    private void OnTriggerEnter(Collider other)
    {
        if (!_detectingCollisions) return;
        if (other.gameObject == _lastTarget)
        {
            if (_lastTarget.TryGetComponent(out EnemyController enemy))
            {
                if (_playerController.LifeSteal)
                {
                    _playerController.Heal(1);
                }

                enemy.TakeDamage(_damage);
                _playerController.AutoAttacking = false;
                Instantiate(_wandSplashPrefab, transform.position, Quaternion.identity);
                _detectingCollisions = false;
                StartCoroutine(DestroySelfCoroutine());
            }
            if (_lastTarget.TryGetComponent(out BossAI boss))
            {
                if (_playerController.LifeSteal)
                {
                    _playerController.Heal(1);
                }

                boss.TakeDamage(_damage);
                _playerController.AutoAttacking = false;
                Instantiate(_wandSplashPrefab, transform.position, Quaternion.identity);
                _detectingCollisions = false;
                StartCoroutine(DestroySelfCoroutine());
            }
        }
    }
}
