using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _playerSpeed;

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        if (!PlayerController.Instance.IsMoving) return;

        transform.position = Vector3.MoveTowards(transform.position, PlayerController.Instance.PlayerDestinationPositon, _playerSpeed * Time.deltaTime);
    }
}
