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

    public Dictionary<Vector2, Tile> _tiles { get; private set; }

    public bool GenerateGridButton = false;

    private void Awake()
    {
        Instance = this;

        _tiles = new Dictionary<Vector2, Tile>();
        foreach (Transform child in transform)
        {
            string name = child.name;
            int x = int.Parse(name.Split(",")[0]);
            int y = int.Parse(name.Split(",")[1]);
            _tiles[new Vector2(x, y)] = child.GetComponent<Tile>();
        }
    }

    private void OnValidate()
    {
        if (GenerateGridButton)
        {
            Debug.Log("Generating Grid");

            GenerateGrid();

            GenerateGridButton = false;
        }
    }

    private void Start()
    {

    }

    private void Update()
    {
        CalculateMousePosition();
    }

    void GenerateGrid()
    {
        if(_tiles != null)
        {
            foreach(Transform c in transform)
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    DestroyImmediate(c.gameObject);
                };
            }
        }

        _tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                int newX = 2 * (x - _width / 2);
                int newY = 2 * (y - _height / 2);

                Tile spawnedTile = Instantiate(_tilePrefab, new Vector3(newX, 0, newY), Quaternion.Euler(90, 0, 0), transform);
                spawnedTile.name = $"{newX},{newY}";

                spawnedTile.Init(newX, newY);

                _tiles[new Vector2(newX, newY)] = spawnedTile;
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

    public Tile GetTileAtPosition(Vector3 pos)
    {
        if (_tiles.TryGetValue(new Vector2(Mathf.Round(pos.x/2)*2, Mathf.Round(pos.z/2)*2), out var tile))
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

    public List<Tile> AStarPathFind(Tile startTile, Tile targetTile)
    {
        List<Tile> toSearch = new List<Tile>() { startTile };
        List<Tile> processed = new List<Tile>();

        while (toSearch.Any())
        {
            Tile current = toSearch[0];
            foreach (Tile t in toSearch)
            {
                if (t.F < current.F || t.F == current.F && t.H < current.H)
                {
                    current = t;
                }
            }

            processed.Add(current);
            toSearch.Remove(current);

            if (current == targetTile)
            {
                Tile currentPathTile = targetTile;
                List<Tile> path = new List<Tile>();
                while (currentPathTile != startTile)
                {
                    path.Add(currentPathTile);
                    currentPathTile = currentPathTile.Connection;
                }

                path?.Reverse();
                return path;
            }

            foreach (Tile neighbor in FindWalkableNeighbors(current, processed))
            {
                bool inSearch = toSearch.Contains(neighbor);

                float costToNeighbor = current.G + current.GetDistance(neighbor);

                if (!inSearch || costToNeighbor < neighbor.G)
                {
                    neighbor.G = costToNeighbor;
                    neighbor.Connection = current;

                    if (!inSearch)
                    {
                        neighbor.H = neighbor.GetDistance(targetTile);
                        toSearch.Add(neighbor);
                    }
                }
            }
        }
        return null;
    }

    private List<Tile> FindWalkableNeighbors(Tile tile, List<Tile> processed)
    {
        List<Tile> neighbors = new List<Tile>();

        Tile right = GetTileAtPosition(tile.X + 2, tile.Y);
        Tile left = GetTileAtPosition(tile.X - 2, tile.Y);
        Tile up = GetTileAtPosition(tile.X, tile.Y + 2);
        Tile down = GetTileAtPosition(tile.X, tile.Y - 2);

        if (right != null)
        {
            if (right.Walkable && !processed.Contains(right))
                neighbors.Add(right);
        }
        if (left != null)
        {
            if (left.Walkable && !processed.Contains(left))
                neighbors.Add(left);
        }
        if (up != null)
        {
            if (up.Walkable && !processed.Contains(up))
                neighbors.Add(up);
        }
        if (down != null)
        {
            if (down.Walkable && !processed.Contains(down))
                neighbors.Add(down);
        }

        return neighbors;
    }

    public void PathTiles(List<Tile> path)
    {
        foreach(Tile t in _tiles.Values)
        {
            t.Pathed = false;
        }

        foreach(Tile t in path)
        {
            t.Pathed = true;
        }
    }
}