using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [HideInInspector] public static PlayerController Instance { get; private set; }

    [HideInInspector] public Vector3 MouseWorldPosition { get; private set; }
    [HideInInspector] public Vector3 PlayerDestinationPositon { get; private set; }

    [HideInInspector] public bool IsMoving { get; private set; }
    [HideInInspector] public bool IsCasting;

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
        HandlePlayerMoving();
        ManagePlayerHealth();

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

    private void HandlePlayerMoving()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Tile currTile = GridManager.Instance.GetTileAtPosition(transform.position);
            Tile targetTile = GridManager.Instance.GetTileAtPosition(MouseWorldPosition);
            List<Tile> path = GridManager.Instance.AStarPathFind(currTile, targetTile);

            StopCoroutine(MovePlayer(path));
            StartCoroutine(MovePlayer(path));
        }
        */
    }

    IEnumerator MovePlayer(List<Tile> path)
    {
        foreach(Tile t in path)
        {
            transform.position = t.transform.position;

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ManagePlayerHealth()
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

        //HANDLE PLAYER DEATH
        if(CurrentHealth <= 0)
        {

        }
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
    }

    public void Heal(int hp)
    {
        CurrentHealth += hp;
    }
}
