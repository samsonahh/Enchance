using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAttackScript : MonoBehaviour
{
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _speed = 10f;

    private GameObject _target;

    private void Start()
    {
        _target = PlayerController.Instance.LastTarget;
        transform.position = GameObject.Find("StaffEffects").transform.localPosition + PlayerController.Instance.transform.position;

        PlayerController.Instance.AutoAttacking = true;
    }

    private void Update()
    {
        if (_target == null)
        {
            PlayerController.Instance.AutoAttacking = false;
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, _target.transform.position + Vector3.up, _speed * Time.deltaTime);

        if(Vector3.Distance(transform.position, _target.transform.position + Vector3.up) <= 0.01f)
        {
            if(_target.TryGetComponent(out EnemyController enemy))
            { 
                enemy.TakeDamage(_damage);
                PlayerController.Instance.AutoAttacking = false;
                Destroy(gameObject);
            }
            if (_target.TryGetComponent(out BossAI boss))
            {
                boss.TakeDamage(_damage);
                PlayerController.Instance.AutoAttacking = false;
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other == _target)
        {
            if (_target.TryGetComponent(out EnemyController enemy))
            {
                enemy.TakeDamage(_damage);
                PlayerController.Instance.AutoAttacking = false;
                Destroy(gameObject);
            }
            if (_target.TryGetComponent(out BossAI boss))
            {
                boss.TakeDamage(_damage);
                PlayerController.Instance.AutoAttacking = false;
                Destroy(gameObject);
            }
        }
    }
}
