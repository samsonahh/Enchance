using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour
{
    private PlayerController _playerController;
    private GridManager _gridManager;
    private Rigidbody _rigidBody;

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
    [SerializeField] private float _playerStraightThresholdToSlam = 1.5f;
    private float _followPlayerTimer = 0f;
    private float _playerStraightPathTimer = 0f;
    #endregion

    #region PushPlayerVariables
    [Header("Push Player Variables")]
    [SerializeField] private float _pushStartVelocity = 1f;
    [SerializeField] private float _pushStunDuration = 2f;
    private bool _pushPlayerCoroutineStarted = false;
    #endregion

    #region SlamVariables
    [Header("Slam Variables")]
    [SerializeField] private float _slamDuration = 1f;
    [SerializeField] private float _slamJumpHeight = 2f;
    private bool _slamCoroutineStarted = false;
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
        _rigidBody = GetComponent<Rigidbody>();

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
            Debug.Log("Suck");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeBossState(BossState.SlamOntoPlayer);
            Debug.Log("Slam");
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

                _gridManager.ClearPath();
                Debug.Log(_playerStraightPathTimer);
                if (_gridManager.IsPathStraight(_path))
                {
                    _gridManager.PathTiles(_path);
                    _playerStraightPathTimer += Time.deltaTime;
                }
                else
                {
                    _playerStraightPathTimer = 0f;
                }

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

                if(_playerStraightPathTimer > _playerStraightThresholdToSlam)
                {
                    ChangeBossState(BossState.SlamOntoPlayer);
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

                if (!_slamCoroutineStarted)
                {
                    _slamCoroutineStarted = true;
                    StartCoroutine(SlamPlayer(_playerTile));
                }

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

        StopAllCoroutines();
        _gridManager.ClearPath();
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        _pushPlayerCoroutineStarted = false;
        _slamCoroutineStarted = false;

        switch (state)
        {
            case BossState.Idle:

                break;
            case BossState.FollowPlayer:
                _followPlayerTimer = 0f;
                _playerStraightPathTimer = 0f;
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
                _playerController.StopPlayer();
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
        CameraShake.Instance.Shake(0.25f, 0.25f);

        Vector3 dir = (_playerController.transform.position - transform.position).normalized;

        _playerController.TakeDamage(5);
        _playerController.PushPlayer(dir, _pushStunDuration, _pushStartVelocity);

        yield return null;

        ChangeBossState(BossState.FollowPlayer);
    }

    IEnumerator SlamPlayer(Tile t)
    {
        Tile[] dangerTiles = new Tile[13];
        dangerTiles[0] = _gridManager.GetTileAtPosition(t.X + 2, t.Y + 2);
        dangerTiles[1] = _gridManager.GetTileAtPosition(t.X + 2, t.Y);
        dangerTiles[2] = _gridManager.GetTileAtPosition(t.X + 2, t.Y - 2);
        dangerTiles[3] = _gridManager.GetTileAtPosition(t.X, t.Y - 2);
        dangerTiles[4] = t;
        dangerTiles[5] = _gridManager.GetTileAtPosition(t.X, t.Y + 2);
        dangerTiles[6] = _gridManager.GetTileAtPosition(t.X - 2, t.Y + 2);
        dangerTiles[7] = _gridManager.GetTileAtPosition(t.X - 2, t.Y);
        dangerTiles[8] = _gridManager.GetTileAtPosition(t.X - 2, t.Y - 2);
        dangerTiles[9] = _gridManager.GetTileAtPosition(t.X, t.Y - 4);
        dangerTiles[10] = _gridManager.GetTileAtPosition(t.X, t.Y + 4);
        dangerTiles[11] = _gridManager.GetTileAtPosition(t.X - 4, t.Y);
        dangerTiles[12] = _gridManager.GetTileAtPosition(t.X + 4, t.Y);

        foreach(Tile dangerTile in dangerTiles)
        {
            if(dangerTile != null)
            {
                dangerTile.Pathed = true;
            }
        }

        Vector3 abovePlayerTile = t.transform.position + _slamJumpHeight * Vector3.up;

        float timer = 0f;
        while (timer < _slamDuration)
        {
            transform.position = Vector3.Lerp(transform.position, abovePlayerTile, 10f * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0f;
        while(timer < 0.2f)
        {
            transform.position = Vector3.Lerp(transform.position, t.transform.position, 20f * Time.deltaTime);
            timer += Time.deltaTime;

            if(Vector3.Distance(transform.position, t.transform.position) <= 0.1f)
            {
                break;
            }

            yield return null;
        }

        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        CameraShake.Instance.Shake(0.25f, 0.25f);

        foreach (Tile dangerTile in dangerTiles)
        {
            if (dangerTile == null) continue;

            if(_playerTile == dangerTile)
            {
                _playerController.TakeDamage(5);
                _playerController.StunPlayer(_slamDuration + 0.5f);
                break;
            }
        }

        _gridManager.ClearPath();
        yield return new WaitForSeconds(0.5f);

        Tile randTile = _gridManager.GetRandomTileAwayFromPlayer(7.5f);
        //Vector3 aboveRandTile = randTile.transform.position + _slamJumpHeight * Vector3.up;
        Vector3 aboveRandTile = randTile.transform.position;

        timer = 0f;
        while (Vector3.Distance(transform.position, aboveRandTile) >= 0.1f && timer < 2f)
        {
            transform.position = Vector3.Lerp(transform.position, aboveRandTile, 5f * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

/*        timer = 0f;
        while (timer < 0.2f)
        {
            transform.position = Vector3.Lerp(transform.position, randTile.transform.position, 20f * Time.deltaTime);
            timer += Time.deltaTime;

            if (Vector3.Distance(transform.position, randTile.transform.position) <= 0.1f)
            {
                break;
            }

            yield return null;
        }*/

        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        _slamCoroutineStarted = false;
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
