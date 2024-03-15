using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballScript : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private int _damage = 5;
    [SerializeField] private GameObject _splashPrefab;

    private Vector3 _target;

    private void Start()
    {
        transform.position = AbilityCaster.Instance.transform.position;
        _target = transform.position + AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.SelectedAbility].CastRadius * PlayerController.Instance.LastForwardDirection;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _target, _speed * Time.deltaTime);

        if(Vector3.Distance(transform.position, _target) <= 0.01f)
        {
            Collider[] collisions = Physics.OverlapSphere(transform.position, 1f);
            if (collisions != null)
            {
                foreach (Collider collider in collisions)
                {
                    if (collider.TryGetComponent(out BossAI bossAI))
                    {
                        bossAI.TakeDamage(_damage);
                    }
                }
            }
            Instantiate(_splashPrefab, transform.position + Vector3.up, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out BossAI boss))
        {
            boss.TakeDamage(_damage);
            Instantiate(_splashPrefab, transform.position + Vector3.up, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}

