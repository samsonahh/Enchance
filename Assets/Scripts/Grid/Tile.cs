using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private GridManager _gridManager;
    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;
    [SerializeField] private GameObject _pathed;

    [HideInInspector] public int X { get; private set; }
    [HideInInspector] public int Y { get; private set; }
    [HideInInspector] public float G;
    [HideInInspector] public float H;
    [HideInInspector] public float F => G + H;
    [HideInInspector] public Tile Connection;
    public bool Walkable;

    public void Init(int x, int y)
    {
        _gridManager = GridManager.Instance;

        X = x;
        Y = y;

        bool isOffset = (x + y) % 2 == 1;
        _renderer.color = isOffset ? _offsetColor : _baseColor;
    }

    private void Update()
    {

    }

    public float GetDistance(Tile t)
    {
        return Vector3.Distance(new Vector3(X, 0, Y), transform.position);
    }

    public void PrintTile()
    {
        Debug.Log($"{X}, {Y}");
    }
}