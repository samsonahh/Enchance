using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public static PlayerController Instance { get; private set; }
    [HideInInspector] public Vector3 ForwardDirection { get; private set; } = Vector3.right;
    [HideInInspector] public Vector3 MouseWorldPosition { get; private set; }
    [HideInInspector] public Vector3 PlayerDestinationPositon { get; private set; }
    [HideInInspector] public bool IsMoving { get; private set; }
    [HideInInspector] public bool IsCasting;

    private NavMeshAgent _navMeshAgent;

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform _arrowPivot;
    [SerializeField] private GameObject _playerDestinationIndicatorPrefab;
    private GameObject _tempDestIndicator;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        GetMouseWorldPosition();
        HandlePlayerMoving();
        HandlePlayerFacingDirection();
    }

    private void GetMouseWorldPosition()
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            MouseWorldPosition = ray.GetPoint(distance);
        }
    }

    private void HandlePlayerMoving()
    {
        if (IsCasting)
        {
            IsMoving = false;
            _navMeshAgent.SetDestination(transform.position);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            IsMoving = true;

            PlayerDestinationPositon = MouseWorldPosition;
            _navMeshAgent.SetDestination(PlayerDestinationPositon);

            if(_tempDestIndicator != null)
                Destroy(_tempDestIndicator);
            _tempDestIndicator = Instantiate(_playerDestinationIndicatorPrefab, PlayerDestinationPositon, Quaternion.identity);
        }

        if(_navMeshAgent.isStopped)
        {
            IsMoving = false;
        }
    }

    private void HandlePlayerFacingDirection()
    {
        Vector3 facingDirection = (MouseWorldPosition - transform.position).normalized;

        if (Mathf.Abs(facingDirection.x) > 0) _spriteRenderer.flipX = facingDirection.x < 0;

        if (facingDirection.magnitude > 0)
        {
            ForwardDirection = facingDirection;
        }

        float targetAngle = -(Mathf.Atan2(ForwardDirection.z, ForwardDirection.x) * Mathf.Rad2Deg - 90f);
        _arrowPivot.localRotation = Quaternion.Lerp(_arrowPivot.localRotation, Quaternion.AngleAxis(targetAngle, Vector3.up), 50f * Time.deltaTime);
    }
}
