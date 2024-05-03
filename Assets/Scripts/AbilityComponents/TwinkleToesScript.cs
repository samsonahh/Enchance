using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwinkleToesScript : MonoBehaviour
{
    [SerializeField] private float _additionalSpeed = 4f;
    [SerializeField] private float _duration = 3f;

    private void Start()
    {
        GameManager.Instance.PlayerControllerInstance.ChangeCurrentMoveSpeed(_additionalSpeed + GameManager.Instance.PlayerControllerInstance.PlayerRegularMoveSpeed, _duration);
        Destroy(gameObject, _duration);
    }

    private void Update()
    {
        transform.position = GameManager.Instance.PlayerControllerInstance.transform.position;
    }
}
