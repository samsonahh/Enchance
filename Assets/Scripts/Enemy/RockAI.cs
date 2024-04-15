using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockAI : EnemyController
{
    [Header("RockAI")]
    [SerializeField] private RockState _currentState;
    private List<Coroutine> _stateCoroutines = new List<Coroutine>();
    private enum RockState
    {
        Idle,
        Startled,
        FollowPlayer,
        RollToPlayer,
        RecoverFromRoll
    }

    [SerializeField] private int _contactDamage = 1;
    [SerializeField] private float _stunDuration = 0.5f;
    [SerializeField] private float _pushStartVelocity = 3f;

    #region Idle
    [Header("Idle Variables")]
    [SerializeField] private float _activateRange = 7.5f;
    [SerializeField] private float _deactivateRange = 20f;
    #endregion

    #region Startled
    [Header("Startled Variables")]
    [SerializeField] private float _startledDuration = 0.5f;
    #endregion

    #region FollowPlayer
    [Header("Follow Player Variables")]
    private float _followTimer = 0f;
    [SerializeField] private float _followPatienceDuration = 4f;
    #endregion

    #region RollToPlayer
    [Header("Roll To Player Variables")]
    [SerializeField] private float _rollSpeed = 10f;
    #endregion

    #region RecoverFromRoll
    [Header("Recover From Roll Variables")]
    [SerializeField] private float _recoverDuration = 1f;
    #endregion

    protected override void OnStart()
    {
        base.OnStart();

        ChangeState(RockState.Idle);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        RockStateStateMachine();
    }

    protected override void OnDeath()
    {
        Destroy(gameObject);
    }

    protected override void HandleAnimations()
    {
        if (IsStunned)
        {
            _spriteRenderer.transform.parent.localRotation = Quaternion.Lerp(_spriteRenderer.transform.parent.localRotation, Quaternion.Euler(90f, 0, 0), 20f * Time.deltaTime);
        }
        else
        {
            _spriteRenderer.transform.parent.localRotation = Quaternion.Lerp(_spriteRenderer.transform.parent.localRotation, Quaternion.Euler(45f, 0, 0), 20f * Time.deltaTime);
        }
    }

    private void OnDrawGizmos()
    {
        CustomGizmos.DrawWireDisk(transform.position, _activateRange, Color.red);
        CustomGizmos.DrawDisk(transform.position, _activateRange, Color.red);
    }

    private void RockStateStateMachine()
    {
        if(_distanceToPlayer > _deactivateRange)
        {
            ChangeState(RockState.Idle);
        }

        if (IsStunned)
        {
            _animator.Play("RockIdle");
            ChangeState(RockState.FollowPlayer);
            return;
        }

        switch (_currentState)
        {
            case RockState.Idle:

                if(_distanceToPlayer < _activateRange)
                {
                    ChangeState(RockState.Startled);
                }
                
                if (CurrentHealth < MaxHealth)
                {
                    ChangeState(RockState.Startled);
                }

                break;
            case RockState.Startled:
                break;
            case RockState.FollowPlayer:

                if (PlayerController.Instance.IsInvincible)
                {
                    _animator.Play("RockIdle");
                    return;
                }

                _animator.Play("RockRoll");

                LookAtPlayer();

                _followTimer += Time.deltaTime;

                transform.position = Vector3.MoveTowards(transform.position, PlayerController.Instance.transform.position, _enemyCurrentMoveSpeed * Time.deltaTime);

                if(_followTimer >= _followPatienceDuration)
                {
                    ChangeState(RockState.RollToPlayer);
                }

                break;
            case RockState.RollToPlayer:
                break;
            case RockState.RecoverFromRoll:
                break;
            default:
                break;
        }
    }

    private void ChangeState(RockState state)
    {
        _currentState = state;

        foreach (Coroutine coroutine in _stateCoroutines)
        {
            StopCoroutine(coroutine);
        }

        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        _followTimer = 0f;

        switch (state)
        {
            case RockState.Idle:
                _animator.Play("RockIdle");
                break;
            case RockState.Startled:
                _animator.Play("RockIdle");
                _stateCoroutines.Add(StartCoroutine(StartledCoroutine()));
                break;
            case RockState.FollowPlayer:
                break;
            case RockState.RollToPlayer:
                _animator.Play("RockRollFast");
                _stateCoroutines.Add(StartCoroutine(RollToPlayerCoroutine()));
                break;
            case RockState.RecoverFromRoll:
                _animator.Play("RockIdle");
                _stateCoroutines.Add(StartCoroutine(RecoverFromRollCoroutine()));
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

            _followTimer = 0f;

            if (_currentState == RockState.FollowPlayer)
            {
                ChangeState(RockState.Startled);
            }
        }

        if(collision.gameObject.tag == "Wall")
        {
            if (_currentState == RockState.RollToPlayer)
            {
                ChangeState(RockState.RecoverFromRoll);
            }
        }
    }

    IEnumerator StartledCoroutine()
    {
        LookAtPlayer();

        yield return new WaitForSeconds(_startledDuration);

        ChangeState(RockState.FollowPlayer);
    }

    IEnumerator RollToPlayerCoroutine()
    {
        LookAtPlayer();

        Vector3 dirToPlayer = PlayerController.Instance.transform.position - transform.position;
        Vector3 destToRoll = (dirToPlayer + 2.5f * dirToPlayer.normalized) + transform.position;

        while(Vector3.Distance(transform.position, destToRoll) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destToRoll, _rollSpeed * Time.deltaTime);
            yield return null;
        }

        ChangeState(RockState.RecoverFromRoll);
    }

    IEnumerator RecoverFromRollCoroutine()
    {
        yield return new WaitForSeconds(_recoverDuration);

        ChangeState(RockState.FollowPlayer);
    }
}