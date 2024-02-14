using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _playerSpeed;
    private void FixedUpdate()
    {
        HandleWASDMovement();
    }

    private void HandleWASDMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 movementDirection = new Vector3(x, 0, z).normalized;

        transform.Translate(_playerSpeed * Time.deltaTime * movementDirection);
    }
}
