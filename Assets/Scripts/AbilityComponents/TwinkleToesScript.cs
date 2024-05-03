using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwinkleToesScript : MonoBehaviour
{
    [SerializeField] private float _additionalSpeed = 4f;
    [SerializeField] private float _duration = 3f;

    private void Start()
    {
        PlayerController.Instance.ChangeCurrentMoveSpeed(_additionalSpeed + PlayerController.Instance.PlayerRegularMoveSpeed, _duration);
        Destroy(gameObject, _duration);
    }

    private void Update()
    {
        transform.position = PlayerController.Instance.transform.position;
    }
}
