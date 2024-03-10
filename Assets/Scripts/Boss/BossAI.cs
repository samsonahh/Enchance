using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour
{
    private PlayerController _playerController;
    private GridManager _gridManager;

    public BossState _currentState;

    private float _distanceToPlayer;

    private Action<Tile> OnBossStepOnNewTile;
    private Tile _currentTile;
    private Tile _playerTile;
    private List<Tile> _path;

    private List<GameState> _phaseOneStates;
    private List<GameState> _phaseTwoStates;
    private List<GameState> _phaseThreeStates;

    #region IdleVariables
    [Header("Idle Variables")]
    [SerializeField] private float _activateDistance = 10f;
    #endregion

    #region FollowPlayerVariables
    [Header("Follow Player Variables")]
    [SerializeField] private float _followPlayerWaitTime = 1f;
    private float _followPlayerTimer = 0f;
    #endregion

    #region PushPlayerVariables
    [Header("Push Player Variables")]
    [SerializeField] private float _pushStartVelocity = 1f;
    [SerializeField] private float _stunDuration = 2f;
    private bool _pushPlayerCoroutineStarted = false;
    #endregion

    #region SuckVariables
    [Header("Suck Variables")]
    [SerializeField] private float _suckDuration = 5f;
    private float _suckTimer = 0f;
    [SerializeField] private float _suckStrength = 10f;
    #endregion

    private void Awake()
    {
        PlayerController.OnPlayerStepOnNewTile += PlayerController_OnPlayerStepOnNewTile;
        OnBossStepOnNewTile += BossAI_OnBossStepOnNewTile;
    }

    private void OnDestroy()
    {
        PlayerController.OnPlayerStepOnNewTile -= PlayerController_OnPlayerStepOnNewTile;
        OnBossStepOnNewTile -= BossAI_OnBossStepOnNewTile;
    }

    private void Start()
    {
        _playerController = PlayerController.Instance;
        _gridManager = GridManager.Instance;

        _currentTile = GridManager.Instance._tiles[new Vector2(0, 14)];
        transform.position = _currentTile.transform.position;
    }

    private void Update()
    {
        CalculatePlayerDistance();
        HandleTileChange();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeBossState(BossState.Suck);
        }

        switch (_currentState)
        {
            case BossState.Idle:

                if(_distanceToPlayer < _activateDistance)
                {
                    ChangeBossState(BossState.FollowPlayer);
                }

                break;
            case BossState.FollowPlayer:

                _gridManager.PathTiles(_path);

                _followPlayerTimer += Time.deltaTime;

                if (_path.Count == 0) _followPlayerTimer = 0f;

                if(_followPlayerTimer > _followPlayerWaitTime)
                {
                    IEnumerator moving = MoveBoss();
                    StopCoroutine(moving);
                    StartCoroutine(moving);
                    _followPlayerTimer = 0f;
                }

                if(_currentTile == _playerTile)
                {
                    ChangeBossState(BossState.PushAway);
                }

                break;
            case BossState.PushAway:

                if (!_pushPlayerCoroutineStarted)
                {
                    _pushPlayerCoroutineStarted = true;
                    StartCoroutine(PushPlayer());
                }

                break;
            case BossState.SlamOntoPlayer:



                break;
            case BossState.FloorIsLava:
                break;
            case BossState.PawnSpawn:
                break;
            case BossState.TPTiles:
                break;
            case BossState.Root:
                break;
            case BossState.Suck:

                _suckTimer += Time.deltaTime;

                _playerController.transform.position = Vector3.MoveTowards(_playerController.transform.position, transform.position, _suckStrength * Time.deltaTime);

                if(_suckTimer > _suckDuration)
                {
                    ChangeBossState(BossState.FollowPlayer);
                }

                if (_currentTile == _playerTile)
                {
                    ChangeBossState(BossState.PushAway);
                }

                break;
            case BossState.FireTornados:
                break;
            case BossState.BombingRun:



                break;
            default:
                break;
        }
    }

    private void ChangeBossState(BossState state)
    {
        _currentState = state;
        switch (state)
        {
            case BossState.Idle:
                break;
            case BossState.FollowPlayer:
                _followPlayerTimer = 0f;
                break;
            case BossState.PushAway:
                _pushPlayerCoroutineStarted = false;
                break;
            case BossState.SlamOntoPlayer:
                break;
            case BossState.FloorIsLava:
                break;
            case BossState.PawnSpawn:
                break;
            case BossState.TPTiles:
                break;
            case BossState.Root:
                break;
            case BossState.Suck:
                _suckTimer = 0f;
                break;
            case BossState.FireTornados:
                break;
            case BossState.BombingRun:
                break;
            default:
                break;
        }
    }

    private void CalculatePlayerDistance()
    {
        _distanceToPlayer = Vector3.Distance(_playerController.transform.position, transform.position);
    }

    private void HandleTileChange()
    {
        Tile t = GridManager.Instance.GetTileAtPosition(transform.position);
        if (_currentTile != t)
        {
            _currentTile = t;
            OnBossStepOnNewTile?.Invoke(_currentTile);
        }
    }

    private void PlayerController_OnPlayerStepOnNewTile(Tile t)
    {
        _playerTile = t;
        _path = _gridManager.AStarPathFind(_currentTile, _playerTile);
    }

    private void BossAI_OnBossStepOnNewTile(Tile t)
    {
        _path = _gridManager.AStarPathFind(_currentTile, _playerTile);
    }

    IEnumerator MoveBoss()
    {
        List<Tile> path = new List<Tile>(_path);

        Vector3 dir = path[0].transform.position - transform.position;
        if (Mathf.Abs(dir.x) > 0) transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = dir.x < 0;

        float timer = 0f;
        while(timer < 0.5f)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Slerp(transform.position, path[0].transform.position, 10f * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator PushPlayer()
    {
        Vector3 dir = (_playerController.transform.position - transform.position).normalized;

        _playerController.IsStunned = true;
        _playerController.TakeDamage(5);

        float currVel = _pushStartVelocity;
        float timer = _stunDuration;

        while(timer > 0)
        {
            _playerController.transform.Translate(currVel * Time.deltaTime * dir);

            currVel = (timer/_stunDuration) * _pushStartVelocity;
            timer -= Time.deltaTime;

            yield return null;
        }

        _playerController.IsStunned = false;
        ChangeBossState(BossState.FollowPlayer);
    }
}

public enum BossState
{
    Idle,
    FollowPlayer,
    PushAway,
    SlamOntoPlayer,
    FloorIsLava,
    PawnSpawn,
    TPTiles,
    Root,
    Suck,
    FireTornados,
    BombingRun
}
