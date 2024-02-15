using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public static PlayerController Instance { get; private set; }
    [HideInInspector] public Vector3 ForwardDirection { get; private set; } = Vector3.right;

    [field: SerializeField] public bool UsingController { get; private set; } = true;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform _arrowPivot;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        HandlePlayerFacingDirection();
    }

    private void HandlePlayerFacingDirection()
    {
        float x = Input.GetAxisRaw("RightHorizontal");
        float z = Input.GetAxisRaw("RightVertical");

        Vector3 facingDirection = new Vector3(x, 0, z).normalized;

        if (Mathf.Abs(x) > 0) _spriteRenderer.flipX = x < 0;

        if (facingDirection.magnitude > 0)
        {
            ForwardDirection = facingDirection;
        }

        float targetAngle = -(Mathf.Atan2(ForwardDirection.z, ForwardDirection.x) * Mathf.Rad2Deg - 90f);
        _arrowPivot.localRotation = Quaternion.Lerp(_arrowPivot.localRotation, Quaternion.AngleAxis(targetAngle, Vector3.up), 50f * Time.deltaTime);
    }
}
