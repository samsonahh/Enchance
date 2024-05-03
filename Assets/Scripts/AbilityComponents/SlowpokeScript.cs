using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowpokeScript : MonoBehaviour
{
    [SerializeField] private float _newSpeed = 1f;
    [SerializeField] private float _duration = 3f;

    private Transform _targetTransform;

    void Start()
    {
        if(PlayerController.Instance.Target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (PlayerController.Instance.Target.TryGetComponent(out BossAI boss))
        {
            _targetTransform = boss.transform;
            boss.ChangeCurrentMoveSpeed(0.5f, _duration);
            Destroy(gameObject, _duration);
            return;
        }

        if (PlayerController.Instance.Target.TryGetComponent(out EnemyController enemy))
        {
            _targetTransform = enemy.transform;
            enemy.ChangeCurrentMoveSpeed(_newSpeed, _duration);
            Destroy(gameObject, _duration);
        }
    }

    void Update()
    {
        if (_targetTransform == null) return;

        transform.position = _targetTransform.position;
    }
}
