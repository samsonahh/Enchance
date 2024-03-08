using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [SerializeField] private int _width, _height;
    [SerializeField] private Tile _tilePrefab;

    [HideInInspector] public Vector3 MousePosition;

    private Dictionary<Vector2, Tile> _tiles;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GenerateGrid();
    }

    private void Update()
    {
        CalculateMousePosition();
    }

    void GenerateGrid()
    {
        _tiles = new Dictionary<Vector2, Tile>();
        for(int x = 0; x < _width; x++)
        {
            for(int y = 0; y < _height; y++)
            {
                Tile spawnedTile = Instantiate(_tilePrefab, new Vector3(x, 0, y), Quaternion.Euler(90, 0, 0), transform);
                spawnedTile.name = $"Tile {x},{y}";

                bool isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(x, y);

                _tiles[new Vector2(x, y)] = spawnedTile;
            }
        }
    }

    void CalculateMousePosition()
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            MousePosition = ray.GetPoint(distance);
        }
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        if(_tiles.TryGetValue(pos, out var tile))
        {
            return tile;
        }

        return null;
    }
}
