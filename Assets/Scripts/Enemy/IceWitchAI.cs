using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceWitchAI : EnemyController
{
    [Header("IceWitchAI")]
    [SerializeField] private State _currentState;
    private List<Coroutine> _stateCoroutines = new List<Coroutine>();
    private enum State
    {
        Wander,
        Startled,
        CirclePlayer,
        DistanceFromPlayer,
        CastIceShard,
        Teleport,
        RetreatToSpawn
    }

    [SerializeField] private int _contactDamage = 1;
    [SerializeField] private float _stunDuration = 0.5f;
    [SerializeField] private float _pushStartVelocity = 2f;

    #region Wander
    [Header("Wander Variables")]
    [SerializeField] private float _wanderRange = 5f;
    [SerializeField] private Vector2 _wanderInterval = new Vector2(1f, 3f);
    [SerializeField] private Vector2 _wanderPauseTime = new Vector2(0.5f, 2f);
    [SerializeField] private float _viewDistance = 7.5f;
    #endregion

    #region Startled
    [Header("Startled Variables")]
    [SerializeField] private float _startledDuration = 0.5f;
    #endregion

    #region CirclePlayer
    [Header("Circle Player Variables")]
    [SerializeField] private Vector2 _hesitationDurationRange = new Vector2(1f, 5f);
    [SerializeField] private Vector2 _circlePlayerRadiusRange = new Vector2(5f, 7f);
    [SerializeField] private float _circlePlayerMovementSpeed = 1.5f;
    [SerializeField] private float _relocateInterval = 0.25f;
    private float _circlePlayerRadius;
    private float _hesitationDuration;
    private float _hesitationTimer;
    private float _moveTimer;
    private bool _cwCircle;
    private Vector3 _circleDestination;
    #endregion

    #region DistanceFromPlayer
    [Header("Distance From Player Variables")]
    [SerializeField] private float _distanceFromPlayerMovementSpeed = 5f;
    #endregion

    #region CastIceShard
    [Header("Cast Ice Shard Variables")]
    [SerializeField] private AbilityComponent _iceShardPrefab;
    [SerializeField] private float _castDuration = 0.5f;
    [SerializeField] private float _castMaxRange = 10f;
    [SerializeField] private int _iceDaggerCountWhenMad = 16;
    #endregion

    #region Teleport
    [Header("Teleport Variables")]
    [SerializeField] private float _teleportDuration = 1f;
    [SerializeField] private GameObject _poofEffectPrefab;
    [SerializeField] private AudioClip _poofSFX;
    #endregion

    protected override void OnStart()
    {
        base.OnStart();

        ChangeState(State.Wander);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        StateMachine();
    }

    protected override void OnDeath()
    {
        base.OnDeath();

        Destroy(gameObject);
    }

    protected override void OnDamaged()
    {
        base.OnDamaged();

        if(_currentState == State.Wander)
        {
            ChangeState(State.Teleport);
        }

        if(_currentState != State.Teleport)
        {
            if(Random.Range(0, 2) == 1)
            {
                ChangeState(State.Teleport);
            }
        }
    }

    protected override void HandleAnimations()
    {
        base.HandleAnimations();

        if (IsStunned)
        {
            _spriteRenderer.transform.parent.localRotation = Quaternion.Lerp(_spriteRenderer.transform.parent.localRotation, Quaternion.Euler(90f, 0, 0), 20f * Time.deltaTime);
            _spriteRenderer.transform.parent.localPosition = Vector3.Lerp(_spriteRenderer.transform.parent.localPosition, new Vector3(0, 0.01f, 0), 20f * Time.deltaTime);
        }
        else
        {
            _spriteRenderer.transform.parent.localRotation = Quaternion.Lerp(_spriteRenderer.transform.parent.localRotation, Quaternion.Euler(45f, 0, 0), 20f * Time.deltaTime);
            _spriteRenderer.transform.parent.localPosition = Vector3.Lerp(_spriteRenderer.transform.parent.localPosition, Vector3.zero, 20f * Time.deltaTime);
        }

        _spriteRenderer.transform.parent.gameObject.SetActive(!IsInvincible);
    }

    private void OnDrawGizmos()
    {

    }

    private void StateMachine()
    {
        if (IsStunned)
        {
            ChangeState(State.Teleport);
            return;
        }

        switch (_currentState)
        {
            case State.Wander:
                
                if (_distanceToPlayer < _viewDistance && IsFacingPlayer())
                {
                    ChangeState(State.Startled);
                }

                break;
            case State.Startled:
                break;
            case State.CirclePlayer:

                if (PlayerController.Instance.IsInvincible)
                {
                    _navMeshAgent.speed = 0;
                    return;
                }

                LookAtPlayer();

                _hesitationTimer += Time.deltaTime;
                _moveTimer += Time.deltaTime;

                if(_moveTimer > _relocateInterval)
                {
                    if(_distanceToPlayer < _circlePlayerRadiusRange.y)
                    {
                        EnemyCurrentMoveSpeed = _circlePlayerMovementSpeed;
                    }
                    else
                    {
                        EnemyCurrentMoveSpeed = _distanceFromPlayerMovementSpeed;
                    }

                    _circleDestination = CalculateCircleDestination();
                    _cwCircle = Random.Range(0, 10) == 0 ? !_cwCircle : _cwCircle;
                    _moveTimer = 0f;
                }

                if (_navMeshAgent.enabled)
                {
                    _navMeshAgent.speed = EnemyCurrentMoveSpeed;

                    if (!IsPointInsideNavMeshSurface(_circleDestination))
                    {
                        _cwCircle = !_cwCircle;
                        _circleDestination = CalculateCircleDestination();
                    }

                    _navMeshAgent.SetDestination(_circleDestination);
                }

                if (_hesitationTimer > _hesitationDuration)
                {
                    if(_distanceToPlayer < _castMaxRange)
                    {
                        ChangeState(State.CastIceShard);
                    }
                    else
                    {
                        ChangeState(State.Teleport);
                    }
                }

                if(_distanceFromStart > 5f * _viewDistance)
                {
                    ChangeState(State.RetreatToSpawn);
                }

                break;
            case State.DistanceFromPlayer:

                if (PlayerController.Instance.IsInvincible)
                {
                    _navMeshAgent.speed = 0;
                    return;
                }

                LookAtPlayer();

                if (_navMeshAgent.enabled)
                {
                    _navMeshAgent.speed = EnemyCurrentMoveSpeed;
                    _navMeshAgent.SetDestination(transform.position - PlayerController.Instance.transform.position + transform.position);
                }

                if (_distanceToPlayer > _circlePlayerRadius)
                {
                    ChangeState(State.CirclePlayer);
                }

                break;
            case State.CastIceShard:
                break;
            case State.Teleport:
                break;
            case State.RetreatToSpawn:
                break;
            default:
                break;
        }
    }

    private void ChangeState(State state)
    {
        _currentState = state;

        foreach (Coroutine coroutine in _stateCoroutines)
        {
            StopCoroutine(coroutine);
        }

        _navMeshAgent.enabled = false;

        EnemyCurrentMoveSpeed = EnemyRegularMoveSpeed;

        _hesitationTimer = 0f;
        _moveTimer = 0f;
        _circlePlayerRadius = Random.Range(_circlePlayerRadiusRange.x, _circlePlayerRadiusRange.y);

        _animator.Play("Idle");

        switch (state)
        {
            case State.Wander:
                _navMeshAgent.enabled = true;
                _stateCoroutines.Add(StartCoroutine(WanderCoroutine()));
                break;
            case State.Startled:
                _stateCoroutines.Add(StartCoroutine(StartledCoroutine()));
                break;
            case State.CirclePlayer:
                _navMeshAgent.enabled = true;
                _cwCircle = Random.Range(0, 2) == 1;
                _hesitationDuration = Random.Range(_hesitationDurationRange.x, _hesitationDurationRange.y);
                _circleDestination = CalculateCircleDestination();
                EnemyCurrentMoveSpeed = _circlePlayerMovementSpeed;
                break;
            case State.DistanceFromPlayer:
                _navMeshAgent.enabled = true;
                EnemyCurrentMoveSpeed = _distanceFromPlayerMovementSpeed;
                break;
            case State.CastIceShard:
                _animator.SetFloat("CastSpeedMultiplier", 1/_castDuration);
                _animator.Play("Cast");
                _stateCoroutines.Add(StartCoroutine(CastIceShardCoroutine()));
                break;
            case State.Teleport:
                _animator.SetFloat("CastSpeedMultiplier", 1/(_teleportDuration/2));
                _animator.Play("Cast2");
                _stateCoroutines.Add(StartCoroutine(TeleportCoroutine()));
                break;
            case State.RetreatToSpawn:
                _animator.SetFloat("CastSpeedMultiplier", 1/_castDuration);
                _animator.Play("Cast");
                _stateCoroutines.Add(StartCoroutine(RetreatToSpawnCoroutine()));
                break;
            default:
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController player))
        {
            player.TakeDamage(_contactDamage);

            Vector3 dirToPush = (player.transform.position - transform.position).normalized;

            player.PushPlayer(dirToPush, _stunDuration, _pushStartVelocity);
        }
    }

    IEnumerator WanderCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_wanderPauseTime.x, _wanderPauseTime.y));

            _navMeshAgent.enabled = true;

            Vector2 randomCoords = _wanderRange * Random.insideUnitCircle;
            Vector3 randomDest = new Vector3(randomCoords.x, 0, randomCoords.y) + _startPosition;

            if (_navMeshAgent.enabled)
            {
                _navMeshAgent.speed = EnemyCurrentMoveSpeed;

                if (IsPointInsideNavMeshSurface(randomDest))
                {
                    _navMeshAgent.SetDestination(randomDest);
                }
            }
            LookAt(randomDest);

            float timer = 0f;
            while (timer < Random.Range(_wanderInterval.x, _wanderInterval.y))
            {
                if (Vector3.Distance(randomDest, transform.position) <= 0.1f)
                {
                    break;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            _navMeshAgent.enabled = false;
        }
    }

    IEnumerator StartledCoroutine()
    {
        LookAtPlayer();

        yield return new WaitForSeconds(_startledDuration);

        ChangeState(State.DistanceFromPlayer);
    }

    IEnumerator TeleportCoroutine()
    {
        yield return new WaitForSeconds(_teleportDuration/2);

        Instantiate(_poofEffectPrefab, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(_poofSFX, transform.position);

        IsInvincible = true;
        CanBeTargetted = false;
        GetComponent<Collider>().enabled = false;

        yield return new WaitForSeconds(_teleportDuration);

        int tpTryCounter = 0;

        while (tpTryCounter < 100)
        {
            float randomAngle = Random.Range(90, 270);
            Vector3 randomPos = CalculateCircumferenceOffset(PlayerController.Instance.transform.position, transform.position, _circlePlayerRadius, randomAngle);

            if (IsPointInsideNavMeshSurface(randomPos))
            {
                transform.position = randomPos;
                break;
            }
            
            tpTryCounter++;
        }

        Instantiate(_poofEffectPrefab, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(_poofSFX, transform.position);

        IsInvincible = false;
        CanBeTargetted = true;
        GetComponent<Collider>().enabled = true;

        ChangeState(State.CastIceShard);
    }

    IEnumerator RetreatToSpawnCoroutine()
    {
        LookAtPlayer();

        yield return new WaitForSeconds(_castDuration);

        CastCircleIceShard(_iceDaggerCountWhenMad);

        Instantiate(_poofEffectPrefab, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(_poofSFX, transform.position);

        IsInvincible = true;
        CanBeTargetted = false;

        yield return new WaitForSeconds(_teleportDuration);

        transform.position = _startPosition;

        Instantiate(_poofEffectPrefab, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(_poofSFX, transform.position);

        IsInvincible = false;
        CanBeTargetted = true;

        ChangeState(State.Wander);
    }

    IEnumerator CastIceShardCoroutine()
    {
        LookAtPlayer();

        yield return new WaitForSeconds(_castDuration);

        if(CurrentHealth > MaxHealth / 2)
        {
            CastTripleIceShard();
        }
        else
        {
            CastCircleIceShard(_iceDaggerCountWhenMad);
        }

        yield return new WaitForSeconds(_castDuration);

        ChangeState(State.DistanceFromPlayer);
    }

    private Vector3 CalculateCircumferenceOffset(Vector3 center, Vector3 outside, float radius, float angleOffset)
    {
        Vector3 dirToCenter = outside - center;
        float angle = Mathf.Atan2(dirToCenter.z, dirToCenter.x) + angleOffset * Mathf.Deg2Rad;

        return new Vector3(radius * Mathf.Cos(angle), 0, radius * Mathf.Sin(angle)) + center;
    }

    private Vector3 CalculateCircleDestination()
    {
        int dirMultiplier = _cwCircle ? -1 : 1;

        return CalculateCircumferenceOffset(PlayerController.Instance.transform.position, transform.position, _circlePlayerRadius, dirMultiplier * 15f);
    }

    private void CastTripleIceShard()
    {
        AbilityComponent iceShard = Instantiate(_iceShardPrefab, transform.position, Quaternion.identity);
        iceShard.EnemyInit(transform, _castMaxRange, 0, PlayerController.Instance.transform.position, PlayerController.Instance.transform.position - transform.position);

        iceShard = Instantiate(_iceShardPrefab, transform.position, Quaternion.identity);
        iceShard.EnemyInit(transform, _castMaxRange, 0, CalculateCircumferenceOffset(transform.position, PlayerController.Instance.transform.position, _castMaxRange, 45f), PlayerController.Instance.transform.position - transform.position);

        iceShard = Instantiate(_iceShardPrefab, transform.position, Quaternion.identity);
        iceShard.EnemyInit(transform, _castMaxRange, 0, CalculateCircumferenceOffset(transform.position, PlayerController.Instance.transform.position, _castMaxRange, -45f), PlayerController.Instance.transform.position - transform.position);
    }

    private void CastCircleIceShard(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AbilityComponent iceShard = Instantiate(_iceShardPrefab, transform.position, Quaternion.identity);
            iceShard.EnemyInit(transform, _castMaxRange, 0, CalculateCircumferenceOffset(transform.position, PlayerController.Instance.transform.position, _castMaxRange, 360 / count * i), PlayerController.Instance.transform.position - transform.position);
        }
    }
}
