using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningScript : AbilityComponent
{
    [SerializeField] private AudioClip _sfx;

    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private GameObject _lightningEffect;
    [SerializeField] private int _damage = 5;
    [SerializeField] private float _strikeDelay = 0.5f;
    private float _strikeRadius;
    private SphereCollider _collider;


    private void Start()
    {
        transform.position = _lastCircleWorldPos;
        _strikeRadius = _circleCastRadius;
        _collider = GetComponent<SphereCollider>();

        _collider.radius = _strikeRadius;

        StartCoroutine(Strike());
    }

    IEnumerator Strike()
    {
        yield return new WaitForSeconds(_strikeDelay);

        _lightningEffect.SetActive(true);
        Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        CameraShake.Instance.Shake(0.25f, 0.15f);
        AudioSource.PlayClipAtPoint(_sfx, transform.position);

        Collider[] collisions = Physics.OverlapSphere(transform.position, _strikeRadius);
        if (collisions != null)
        {
            foreach (Collider collider in collisions)
            {
                if (collider.TryGetComponent(out BossAI bossAI))
                {
                    bossAI.TakeDamage(_damage);
                }
                if (collider.TryGetComponent(out EnemyController enemy))
                {
                    enemy.TakeDamage(_damage);
                    enemy.StunEnemy(1f);
                }
            }
        }

        Destroy(gameObject, 0.3f);
    }
}
