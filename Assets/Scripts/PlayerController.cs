using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public PlayerController Instance { get; private set; }

    [HideInInspector] public Vector3 ForwardDirection { get; private set; }

    [SerializeField] private Transform _arrowPivot;
    [SerializeField] private Transform _playerAimMarker;
    private Vector3 _playerAimMarketOffset;
    [SerializeField] private float _playerSpeed;
    [SerializeField] private float _aimSpeed;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        HandlePlayerFacingDirection();
    }

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

    private void HandlePlayerFacingDirection()
    {
        float x = Input.GetAxisRaw("RightHorizontal");
        float z = Input.GetAxisRaw("RightVertical");

        Vector3 facingDirection = new Vector3(x, 0, z).normalized;

        _playerAimMarketOffset += _aimSpeed * Time.deltaTime * facingDirection;
        _playerAimMarker.position = transform.position + _playerAimMarketOffset;

        if (Mathf.Abs(x) > 0) _spriteRenderer.flipX = x < 0;

        if (facingDirection.magnitude > 0)
        {
            ForwardDirection = facingDirection;
        }

        float targetAngle = -(Mathf.Atan2(ForwardDirection.z, ForwardDirection.x) * Mathf.Rad2Deg - 90f);
        _arrowPivot.rotation = Quaternion.Lerp(_arrowPivot.rotation, Quaternion.AngleAxis(targetAngle, Vector3.up), 50f * Time.deltaTime);
    }
}
