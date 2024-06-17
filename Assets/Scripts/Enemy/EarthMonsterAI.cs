using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthMonsterAI : EnemyController
{
    [Header("EarthMonsterAI")]
    [SerializeField] private EarthMonsterState _currentState;
    private List<Coroutine> _stateCoroutines = new List<Coroutine>();
    private enum EarthMonsterState
    {
        Idle,
        GettingUp,
        ReadyingSlam,
        Slam,
        FollowPlayer,
        WalkBack
    }

    [SerializeField] private int _contactDamage = 1;
    [SerializeField] private float _stunDuration = 0.5f;
    [SerializeField] private float _pushStartVelocity = 3f;

    #region Idle
    [Header("Idle Variables")]
    [SerializeField] private float _activateRange = 2f;
    [SerializeField] private float _deactivateRange = 5f;
    #endregion

    #region FollowPlayer
    [Header("Follow Player Variables")]
    private float _followTimer = 0f;
    [SerializeField] private float _followPatienceDuration = 4f;
    #endregion

    #region GettingUp
    [Header("Getting Up Variables")]
    [SerializeField] private float _gettingUpDuration = 2f;
    #endregion

    #region ReadyingSlam
    [Header("Readying Slam Variables")]
    [SerializeField] private float _readyingSlamDuration = 1.5f;
    #endregion

    #region Slam
    [Header("Slam Variables")]
    [SerializeField] private int _slamDamage = 4;
    [SerializeField] private float _slamDuration = 1f;
    [SerializeField] private float _slamCooldown = 1.5f;
    [SerializeField] private float _triggerSlamRadius = 4f;
    [SerializeField] private float _slamMaxRadius = 7.5f;
    [SerializeField] private GameObject _slamPrefab;
    [SerializeField] private AudioClip _slamSFX;
    #endregion

    protected override void OnStart()
    {
        base.OnStart();

        ChangeState(EarthMonsterState.Idle);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        EarthMonsterStateMachine();
    }

    protected override void OnDeath()
    {
        base.OnDeath();

        Destroy(gameObject);
    }

    protected override void OnDamaged()
    {
        base.OnDamaged();

        if(_currentState == EarthMonsterState.Idle)
        {
            ChangeState(EarthMonsterState.GettingUp);
        }

        if (_currentState == EarthMonsterState.WalkBack)
        {
            ChangeState(EarthMonsterState.FollowPlayer);
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
        CustomGizmos.DrawWireDisk(transform.position, _activateRange, Color.green);

        CustomGizmos.DrawDisk(transform.position, _slamMaxRadius, Color.red);
        CustomGizmos.DrawWireDisk(transform.position, _triggerSlamRadius, Color.red);
    }

    private void EarthMonsterStateMachine()
    {
        if (IsStunned)
        {
            ChangeState(EarthMonsterState.FollowPlayer);
            return;
        }

        switch (_currentState)
        {
            case EarthMonsterState.Idle:

                _animator.Play("Idle");

                HideHealth();

                if (_distanceToPlayer < _activateRange)
                {
                    ChangeState(EarthMonsterState.GettingUp);
                }

                break;
            case EarthMonsterState.GettingUp:
                break;
            case EarthMonsterState.ReadyingSlam:

                LookAtPlayer();

                break;
            case EarthMonsterState.Slam:
                break;
            case EarthMonsterState.WalkBack:

                _animator.Play("Follow");

                LookAt(_startPosition);

                if (_navMeshAgent.enabled)
                {
                    _navMeshAgent.speed = EnemyCurrentMoveSpeed;
                    _navMeshAgent.SetDestination(_startPosition);
                }

                if(CheckNavMeshPathFinished())
                {
                    transform.position = _startPosition;
                    ChangeState(EarthMonsterState.Idle);
                }

                if (_distanceToPlayer < _triggerSlamRadius)
                {
                    ChangeState(EarthMonsterState.ReadyingSlam);
                }

                break;
            case EarthMonsterState.FollowPlayer:

                _followTimer += Time.deltaTime;

                if (PlayerController.Instance.IsInvincible)
                {
                    _navMeshAgent.speed = 0;
                    _animator.Play("Still");
                    return;
                }

                LookAtPlayer();

                if (_navMeshAgent.enabled)
                {
                    _navMeshAgent.speed = EnemyCurrentMoveSpeed;
                    _navMeshAgent.SetDestination(PlayerController.Instance.transform.position);
                }

                if (_followTimer >= _followPatienceDuration)
                {
                    ChangeState(EarthMonsterState.WalkBack);
                }

                if (_distanceToPlayer > _deactivateRange)
                {
                    ChangeState(EarthMonsterState.WalkBack);
                }

                if(_distanceToPlayer < _triggerSlamRadius && _followTimer > _slamCooldown)
                {
                    ChangeState(EarthMonsterState.ReadyingSlam);
                }

                break;
            default:
                break;
        }
    }

    private void ChangeState(EarthMonsterState state)
    {
        _currentState = state;

        foreach (Coroutine coroutine in _stateCoroutines)
        {
            StopCoroutine(coroutine);
        }

        _navMeshAgent.enabled = false;

        _followTimer = 0f;

        ShowHealth();

        switch (state)
        {
            case EarthMonsterState.Idle:
                break;
            case EarthMonsterState.Slam:
                _stateCoroutines.Add(StartCoroutine(SlamCoroutine()));
                break;
            case EarthMonsterState.FollowPlayer:
                _animator.CrossFadeInFixedTime("Follow", 0.1f);
                _navMeshAgent.enabled = true;
                break;
            case EarthMonsterState.WalkBack:
                _navMeshAgent.enabled = true;
                break;
            case EarthMonsterState.GettingUp:
                _animator.Play("GettingUp");
                LookAtPlayer();
                _stateCoroutines.Add(StartCoroutine(GettingUpCouroutine()));
                break;
            case EarthMonsterState.ReadyingSlam:
                _animator.Play("ReadyingSlam");
                _stateCoroutines.Add(StartCoroutine(ReadyingSlamCouroutine()));
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

    IEnumerator GettingUpCouroutine()
    {
        yield return new WaitForSeconds(_gettingUpDuration);

        ChangeState(EarthMonsterState.ReadyingSlam);
    }

    IEnumerator ReadyingSlamCouroutine()
    {
        yield return new WaitForSeconds(_readyingSlamDuration);

        ChangeState(EarthMonsterState.Slam);
    }

    IEnumerator SlamCoroutine()
    {
        CameraShake.Instance.Shake(0.3f, 0.15f);
        AudioSource.PlayClipAtPoint(_slamSFX, transform.position);
        _animator.Play("Slam");

        GameObject slam = Instantiate(_slamPrefab, transform.position, Quaternion.identity);
        slam.GetComponent<EarthMonsterSlamEffect>().Init(_slamMaxRadius * 2f, _slamDuration);

        bool wasHit = false;

        Collider[] hits = Physics.OverlapSphere(transform.position, _slamMaxRadius);
        if (hits != null)
        {
            foreach (Collider hit in hits)
            {
                if (hit.TryGetComponent(out PlayerController player))
                {
                    player.TakeDamage(_slamDamage);
                    player.StunPlayer(_readyingSlamDuration);

                    wasHit = true;
                    break;
                }
            }
        }

        yield return new WaitForSeconds(_slamDuration);

        if (!wasHit)
        {
            hits = Physics.OverlapSphere(transform.position, _slamMaxRadius);
            if (hits != null)
            {
                foreach (Collider hit in hits)
                {
                    if (hit.TryGetComponent(out PlayerController player))
                    {
                        player.TakeDamage(_slamDamage);
                        player.StunPlayer(_readyingSlamDuration);

                        wasHit = true;
                        break;
                    }
                }
            }
        }

        yield return new WaitForSeconds(_readyingSlamDuration);

        ChangeState(EarthMonsterState.FollowPlayer);
    }
}
