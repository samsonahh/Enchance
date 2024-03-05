using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private GridManager _gridManager;
    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;
    [SerializeField] private Collider _collider;

    [HideInInspector] public int X {get; private set;}
    [HideInInspector] public int Y {get; private set;}
    [HideInInspector] public float G;
    [HideInInspector] public float H;
    [HideInInspector] public float F => G + H;

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
        _highlight.SetActive(_collider.bounds.Contains(_gridManager.MousePosition));
    }
}