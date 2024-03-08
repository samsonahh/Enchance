using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;
    private PlayerController _playerController;

    public BossState _currentState;

    private float _distanceToPlayer;
    private Vector3 _destination;
    private Vector3 _direction;
    private float _stopTimer;
    private float _creepTimer;
    private float _chargeDistance;

    [SerializeField] private float _startCreepRange;
    [SerializeField] private float _breakCreepRange;
    [SerializeField] private float _startChargeTime;
    [SerializeField] private float _readyingChargeTime;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        _playerController = PlayerController.Instance;
    }

    private void Update()
    {
        CalculatePlayerDistance();

        if (Mathf.Abs(_navMeshAgent.velocity.x) > 0) transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = _navMeshAgent.velocity.x < 0;
        

        switch (_currentState)
        {
            case BossState.Idle:

                if(_distanceToPlayer <= _startCreepRange)
                {
                    _creepTimer = 0;
                    _currentState = BossState.Creep;
                }

                break;
            case BossState.Creep:

                _navMeshAgent.speed = 2.5f;
                _creepTimer += Time.deltaTime;
                Debug.Log(_creepTimer);
                ChasePlayer();

                if(_distanceToPlayer >= _breakCreepRange)
                {
                    _currentState = BossState.Idle;
                }

                if(_creepTimer > _startChargeTime)
                {
                    _direction = (_playerController.transform.position - transform.position).normalized;
                    _chargeDistance = 1.25f * Vector3.Magnitude(_playerController.transform.position - transform.position);
                    _stopTimer = 0f;
                    _currentState = BossState.ReadyingCharge;
                }

                break;
            case BossState.ReadyingCharge:

                _destination = transform.position;
                _navMeshAgent.SetDestination(_destination);

                _stopTimer += Time.deltaTime;
                if(_stopTimer >= _readyingChargeTime)
                {
                    _stopTimer = 0f;
                    _destination = _chargeDistance * _direction + transform.position;
                    _currentState = BossState.Charge;
                }

                break;
            case BossState.Charge:

                _navMeshAgent.speed = 20f;
                _navMeshAgent.SetDestination(_destination);

                if(Vector3.Distance(transform.position, _destination) <= 0.05f)
                {
                    _stopTimer += Time.deltaTime;

                    if(_stopTimer >= 1f)
                    {
                        _stopTimer = 0;
                        _creepTimer = 0;
                        _currentState = BossState.Creep;
                    }
                }

                break;
            case BossState.Cast:
                break;
            default:
                break;
        }
    }

    private void CalculatePlayerDistance()
    {
        _distanceToPlayer = Vector3.Distance(_playerController.transform.position, transform.position);
    }

    private void ChasePlayer()
    {
        _destination = _playerController.transform.position;
        _navMeshAgent.SetDestination(_destination);
    }
}

public enum BossState
{
    Idle,
    Creep,
    ReadyingCharge,
    Charge,
    Cast
}
