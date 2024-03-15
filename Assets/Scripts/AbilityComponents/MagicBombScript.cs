using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicBombScript : MonoBehaviour
{
    [SerializeField] private float _duration;
    [SerializeField] private int _damage = 7;
    [SerializeField] private float _explosionRadius;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private GameObject _explosionPrefab;

    private BossAI _boss;

    private void Start()
    {
        transform.position = PlayerController.Instance.transform.position;
        _boss = GameObject.Find("Boss").GetComponent<BossAI>();
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

            yield return null;
        }

        if (Vector3.Distance(_boss.transform.position, transform.position) < _explosionRadius)
        {
            _boss.TakeDamage(_damage);
        }

        Instantiate(_explosionPrefab, transform.position + Vector3.up, Quaternion.identity);
        CameraShake.Instance.Shake(0.1f, 0.1f);

        Destroy(gameObject);
    }
}
