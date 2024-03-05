using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [SerializeField] private int _width, _height;
    [SerializeField] private Tile _tilePrefab;

    [HideInInspector] public Vector3 MousePosition { get; private set; }

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
    public Tile GetTileAtPosition(int x, int y)
    {
        if (_tiles.TryGetValue(new Vector2(x, y), out var tile))
        {
            return tile;
        }

        return null;
    }

    public List<Tile> AStarPathFind(int startX, int startY, int targetX, int targetY)
    {
        List<Tile> toSearch = new List<Tile>() { GetTileAtPosition(startX, startY) };
        List<Tile> processed = new List<Tile>();

        while (toSearch.Any())
        {
            Tile current = toSearch[0];
            foreach(Tile t in toSearch)
            {
                if(t.F < current.F || t.F == current.F)
            }

            processed.Add(current);
            toSearch.Remove(current);

    
        }

        return new List<Tile>();
    }

    private List<Tile> FindNeighbors(Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();

        Tile right = GetTileAtPosition(tile.X + 1, tile.Y);
        Tile left = GetTileAtPosition(tile.X - 1, tile.Y);
        Tile up = GetTileAtPosition(tile.X, tile.Y + 1);
        Tile down = GetTileAtPosition(tile.X, tile.Y - 1);

        if (right != null) neighbors.Add(right);
        if (left != null) neighbors.Add(left);
        if (up != null) neighbors.Add(up);
        if (down != null) neighbors.Add(down);

        return neighbors;
    }

    private void CalculateAStarParameters(Tile t, int startX, int startY, int targetX, int targetY)
    {
        t.G = Vector2.Distance(new Vector2(t.X, t.Y), new Vector2(startX, startY));
        t.H = Vector2.Distance(new Vector2(t.X, t.Y), new Vector2(targetX, targetY));
    }
}
