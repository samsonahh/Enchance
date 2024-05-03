using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAttackScript : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particle;
    [SerializeField] private GameObject _wandSplashPrefab;

    [SerializeField] private int _damage = 1;
    [SerializeField] private float _speed = 10f;

    private GameObject _target;
    private bool _detectingCollisions = true;

    private void Start()
    {
        _target = GameManager.Instance.PlayerControllerInstance.LastTarget;
        transform.position = GameObject.Find("StaffEffects").transform.localPosition + GameManager.Instance.PlayerControllerInstance.transform.position;

        GameManager.Instance.PlayerControllerInstance.AutoAttacking = true;
    }

    private void Update()
    {
        if (_target == null)
        {
            GameManager.Instance.PlayerControllerInstance.AutoAttacking = false;
            Destroy(gameObject);
            return;
        }

        if (!_detectingCollisions) return;

        transform.position = Vector3.MoveTowards(transform.position, _target.transform.position + Vector3.up, _speed * Time.deltaTime);

        if(Vector3.Distance(transform.position, _target.transform.position + Vector3.up) <= 0.01f)
        {
            if(_target.TryGetComponent(out EnemyController enemy))
            {
                if (GameManager.Instance.PlayerControllerInstance.LifeSteal)
                {
                    GameManager.Instance.PlayerControllerInstance.Heal(1);
                }

                enemy.TakeDamage(_damage);
                GameManager.Instance.PlayerControllerInstance.AutoAttacking = false;
                Instantiate(_wandSplashPrefab, transform.position, Quaternion.identity);
                _detectingCollisions = false;
                StartCoroutine(DestroySelfCoroutine());
            }
            if (_target.TryGetComponent(out BossAI boss))
            {
                if (GameManager.Instance.PlayerControllerInstance.LifeSteal)
                {
                    GameManager.Instance.PlayerControllerInstance.Heal(1);
                }

                boss.TakeDamage(_damage);
                GameManager.Instance.PlayerControllerInstance.AutoAttacking = false;
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
        if (other.gameObject == _target)
        {
            if (_target.TryGetComponent(out EnemyController enemy))
            {
                if (GameManager.Instance.PlayerControllerInstance.LifeSteal)
                {
                    GameManager.Instance.PlayerControllerInstance.Heal(1);
                }

                enemy.TakeDamage(_damage);
                GameManager.Instance.PlayerControllerInstance.AutoAttacking = false;
                Instantiate(_wandSplashPrefab, transform.position, Quaternion.identity);
                _detectingCollisions = false;
                StartCoroutine(DestroySelfCoroutine());
            }
            if (_target.TryGetComponent(out BossAI boss))
            {
                if (GameManager.Instance.PlayerControllerInstance.LifeSteal)
                {
                    GameManager.Instance.PlayerControllerInstance.Heal(1);
                }

                boss.TakeDamage(_damage);
                GameManager.Instance.PlayerControllerInstance.AutoAttacking = false;
                Instantiate(_wandSplashPrefab, transform.position, Quaternion.identity);
                _detectingCollisions = false;
                StartCoroutine(DestroySelfCoroutine());
            }
        }
    }
}
