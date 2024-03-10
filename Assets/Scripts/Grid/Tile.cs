using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private GridManager _gridManager;
    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;

    [HideInInspector] public int X { get; private set; }
    [HideInInspector] public int Y { get; private set; }
    [HideInInspector] public float G;
    [HideInInspector] public float H;
    [HideInInspector] public float F => G + H;
    [HideInInspector] public Tile Connection;
    public bool Walkable = true;
    public bool Pathed = false;

    private void Start()
    {
        _gridManager = GridManager.Instance;

        X = (int)transform.position.x;
        Y = (int)transform.position.z;

        Walkable = true;
    }

    public void Init(int x, int y)
    {
        bool isOffset = Mathf.Abs(x / 2 + y / 2) % 2 == 1;
        _renderer.color = isOffset ? _offsetColor : _baseColor;
    }

    private void Update()
    {
        _highlight.SetActive(false);
        _highlight.SetActive(Pathed);
    }

    public float GetDistance(Tile t)
    {
        return Vector3.Distance(transform.position, t.transform.position);
    }

    public void PrintTile()
    {
        Debug.Log($"{X}, {Y}");
    }
}