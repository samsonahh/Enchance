using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnAI : EnemyController
{
    [Header("PawnAI")]
    [SerializeField] private State _currentState;
    private List<Coroutine> _stateCoroutines = new List<Coroutine>();
    private enum State
    {
        Wander,
        FollowPlayer,
        SingleJump,
        DoubleJump,
        WalkBack
    }

    [SerializeField] private int _contactDamage = 1;
    [SerializeField] private float _stunDuration = 0.5f;
    [SerializeField] private float _pushStartVelocity = 2f;

    #region Wander
    [Header("Wander Variables")]
    [SerializeField] private float _wanderRange = 5f;
    [SerializeField] private Vector2 _wanderInterval = new Vector2(1f, 3f);
    [SerializeField] private Vector2 _wanderPauseTime = new Vector2(0.5f, 2f);
    [SerializeField] private float _viewDistance = 5f;
    #endregion

    #region FollowPlayer
    [Header("Follow Player Variables")]
    private float _followTimer = 0f;
    private float _jumpTimer = 0f;
    [SerializeField] private float _followPatienceDuration = 4f;
    #endregion

    #region Jump
    //[Header("Jump Variables")]
    //[SerializeField] private float _jumpDuration = 1f;
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

        if (_currentState == State.Wander)
        {
            ChangeState(State.FollowPlayer);
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
    }

    private void OnDrawGizmos()
    {

    }

    private void StateMachine()
    {
        if (IsStunned)
        {
            ChangeState(State.FollowPlayer);
            return;
        }

        switch (_currentState)
        {
            case State.Wander:

                if (_distanceToPlayer < _viewDistance && IsFacingPlayer())
                {
                    ChangeState(State.FollowPlayer);
                }

                break;
            case State.FollowPlayer:

                if (PlayerController.Instance.IsInvincible)
                {
                    _navMeshAgent.speed = 0;
                    return;
                }

                LookAtPlayer();

                _followTimer += Time.deltaTime;
                _jumpTimer += Time.deltaTime;

                if (_navMeshAgent.enabled)
                {
                    _navMeshAgent.speed = EnemyCurrentMoveSpeed;
                    _navMeshAgent.SetDestination(PlayerController.Instance.transform.position);
                }

                if (_followTimer >= _followPatienceDuration)
                {
                    bool singleJump = Random.Range(0, 2) == 0;
                    if (singleJump)
                    {
                        ChangeState(State.SingleJump);
                    }
                    else
                    {
                        ChangeState(State.DoubleJump);
                    }
                }

                if (_distanceFromStart > 5f * _viewDistance)
                {
                    ChangeState(State.WalkBack);
                }

                break;
            case State.SingleJump:
                break;
            case State.DoubleJump:
                break;
            case State.WalkBack:

                LookAt(_startPosition);

                _jumpTimer += Time.deltaTime;

                if (_navMeshAgent.enabled)
                {
                    _navMeshAgent.speed = EnemyCurrentMoveSpeed;
                    _navMeshAgent.SetDestination(_startPosition);
                }

                if (_distanceToPlayer < _viewDistance && IsFacingPlayer())
                {
                    ChangeState(State.FollowPlayer);
                }

                if (_distanceFromStart < _wanderRange)
                {
                    ChangeState(State.Wander);
                }

                break;
            default:
                break;
        }
    }

    private void ChangeState(State state) {
        Debug.Log(state);

        _currentState = state;

        foreach (Coroutine coroutine in _stateCoroutines)
        {
            StopCoroutine(coroutine);
        }

        _navMeshAgent.enabled = false;

        EnemyCurrentMoveSpeed = EnemyRegularMoveSpeed;
   
        _followTimer = 0f;
        _jumpTimer = 0f;

        switch (state)
        {
            case State.Wander:
                _navMeshAgent.enabled = true;
                _stateCoroutines.Add(StartCoroutine(WanderCoroutine()));
                break;
            case State.FollowPlayer:
                _navMeshAgent.enabled = true;
                break;
            case State.SingleJump:
                _stateCoroutines.Add(StartCoroutine(SingleJumpCoroutine()));
                break;
            case State.DoubleJump:
                _stateCoroutines.Add(StartCoroutine(DoubleJumpCoroutine()));
                break;
            case State.WalkBack:
                _navMeshAgent.enabled = true;
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

    private Vector3 CalculateNextJumpDestination(Vector3 dest)
    {
        Vector3 vectToDest = dest - transform.position;
        if(vectToDest.x > vectToDest.z)
        {
            return new Vector3(vectToDest.x, 0, 0) + transform.position;
        }
        else
        {
            return new Vector3(0, 0, vectToDest.z) + transform.position;
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

    IEnumerator SingleJumpCoroutine()
    {
        yield return null;
    }

    IEnumerator DoubleJumpCoroutine()
    {
        yield return null;
    }
}
