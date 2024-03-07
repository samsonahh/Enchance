using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private GameObject _highlight;

    [HideInInspector] public static PlayerController Instance { get; private set; }

    [HideInInspector] public Vector3 MouseWorldPosition { get; private set; }
    [HideInInspector] public Vector3 PlayerDestinationPositon { get; private set; }

    [HideInInspector] public bool IsMoving { get; private set; }
    [HideInInspector] public bool IsCasting;

    public PlayerState State;
    private List<Tile> _path;

    //Health
    public int CurrentHealth;
    public int MaxHealth;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CurrentHealth = MaxHealth;
    }

    private void Update()
    {
        GetMouseWorldPosition();
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

        foreach (Tile t in GridManager.Instance._tiles.Values)
        {
            t.Pathed = false;
        }

        switch (State)
        {
            case PlayerState.ChooseMove:

                HighlightHoveredTile();
                _path = GetAStarPath();

                if (_path == null) return;
                ShowPath();

                HandlePlayerInputForMoving(_path);

                break;
            case PlayerState.Moving:

                ShowPath();

                break;
            case PlayerState.Attack:



                break;
            case PlayerState.Dead:



                break;
            default:
                break;
        }

        if (Input.GetKeyDown(KeyCode.Space)) TakeDamage(3);
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

    private List<Tile> GetAStarPath()
    {
        Tile currTile = GridManager.Instance.GetTileAtPosition(transform.position);
        Tile targetTile = GridManager.Instance.GetTileAtPosition(MouseWorldPosition);
        List<Tile> path = GridManager.Instance.AStarPathFind(currTile, targetTile);

        if (path != null)
        {
            path.Reverse();
            return path;
        }

        return null;
    }

    private void HandlePlayerInputForMoving(List<Tile> path)
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartCoroutine(MovePlayer(path));
        }
    }

    IEnumerator MovePlayer(List<Tile> path)
    {
        State = PlayerState.Moving;

        while(path.Any())
        {
            Vector3 dir = path[0].transform.position - transform.position;
            if(Mathf.Abs(dir.x) > 0) _spriteRenderer.flipX = dir.x < 0;

            float timer = 0f;
            while (timer < 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, path[0].transform.position, 15f * Time.deltaTime);

                timer += Time.deltaTime;
                yield return null;
            }

            path.Remove(path[0]);
        }

        State = PlayerState.ChooseMove;
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
    }

    public void Heal(int hp)
    {
        CurrentHealth += hp;
    }

    private void ShowPath()
    {
        foreach (Tile t in _path)
        {
            t.Pathed = true;
        }
    }

    private void HighlightHoveredTile()
    {
        Tile t = GridManager.Instance.GetTileAtPosition(MouseWorldPosition);

        if (t == null)
        {
            _highlight.SetActive(false);
            return;
        }

        _highlight.SetActive(t.Walkable);
        _highlight.transform.position = t.transform.position;
    }
}

public enum PlayerState
{
    ChooseMove,
    Moving,
    Attack,
    Dead
}
