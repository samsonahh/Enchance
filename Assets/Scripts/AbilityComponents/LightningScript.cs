using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningScript : MonoBehaviour
{
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private int _damage = 5;
    private float _strikeRadius;
    private BossAI _boss;
    private SphereCollider _collider;

    private void Start()
    {
        transform.position = PlayerController.Instance.LastCircleWorldPosition;
        _strikeRadius = AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.SelectedAbility].CircleCastRadius;
        _boss = GameObject.Find("Boss").GetComponent<BossAI>();
        _collider = GetComponent<SphereCollider>();

        _collider.radius = _strikeRadius;

        Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        CameraShake.Instance.Shake(0.1f, 0.1f);

/*        if (Vector3.Distance(_boss.transform.position, transform.position) < _strikeRadius)
        {
            _boss.TakeDamage(_damage);
        }*/

        Destroy(gameObject, 0.2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out BossAI boss))
        {
            boss.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }
}
