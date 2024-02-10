using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private PlayerController player;

    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, player.transform.position + -5 * Vector3.forward + 7 * Vector3.up, Time.deltaTime);
    }
}
