using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicBombScript : MonoBehaviour
{
    [SerializeField] private float _duration;
    [SerializeField] private int _damage = 7;
    [SerializeField] private int _playerDamage = 5;
    [SerializeField] private float _explosionRadius;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private Transform _bombRadiusTransform;
    [SerializeField] private Transform _bombRadiusWarning;

    private void Start()
    {
        transform.position = PlayerController.Instance.transform.position;
        _bombRadiusTransform.localScale = new Vector3(_explosionRadius * 2f, _explosionRadius * 2f, 1);
        _bombRadiusWarning.localScale = new Vector3(0, 0, 1);

        StartCoroutine(Explode());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }

    IEnumerator Explode()
    {
        for(float timer = 0f; timer < _duration; timer += Time.deltaTime)
        {
            _spriteRenderer.color = Color.Lerp(Color.white, Color.red, timer / _duration);
            _bombRadiusWarning.localScale = Vector3.Lerp(new Vector3(0, 0, 1), Vector3.one, timer / _duration);

            yield return null;
        }

        Detonate();
    }

    public void Detonate()
    {
        Collider[] collisions = Physics.OverlapSphere(transform.position, _explosionRadius);
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
                }
                if(collider.TryGetComponent(out PlayerController player))
                {
                    player.TakeDamage(_playerDamage);
                    player.StunPlayer(1f);
                }
            }
        }

        Instantiate(_explosionPrefab, transform.position + Vector3.up, Quaternion.identity);
        CameraShake.Instance.Shake(0.25f, 0.25f);

        Destroy(gameObject);
    }
}
