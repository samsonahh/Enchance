using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootFromPlayerComponent : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
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
            Destroy(gameObject);
        }
    }
}

