using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningScript : MonoBehaviour
{
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private int _damage = 5;
    private float _strikeRadius;
    private SphereCollider _collider;

    private void Start()
    {
        transform.position = PlayerController.Instance.LastCircleWorldPosition;
        _strikeRadius = AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.SelectedAbility].CircleCastRadius;
        _collider = GetComponent<SphereCollider>();

        _collider.radius = _strikeRadius;

        Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        CameraShake.Instance.Shake(0.1f, 0.1f);

        Collider[] collisions = Physics.OverlapSphere(transform.position, _strikeRadius);
        if (collisions != null)
        {
            foreach(Collider collider in collisions)
            {
                if(collider.TryGetComponent(out BossAI bossAI))
                {
                    bossAI.TakeDamage(_damage);
                }
            }
        }

        Destroy(gameObject, 0.3f);
    }
}
