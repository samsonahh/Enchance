using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _playerSpeed;
    private Vector3 _movementDirection;

    private void Update()
    {
        CaptureWASDInput();
    }

    private void FixedUpdate()
    {
        HandleWASDMovement();
    }

    private void CaptureWASDInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        _movementDirection = new Vector3(x, 0, z).normalized;
    }

    private void HandleWASDMovement()
    {
        transform.Translate(_playerSpeed * Time.deltaTime * _movementDirection);
    }
}
