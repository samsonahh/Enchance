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
        GenerateGrid();
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
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
            {
                foreach (Transform c in transform)
                {
                    Destroy(c.gameObject);
                }
            }
            else
            {
                foreach (Transform c in transform)
                {
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        DestroyImmediate(c.gameObject);
                    };
                }
            }
#else
                foreach (Transform c in transform)
                {
                    Destroy(c.gameObject);
                }
#endif
        }

        _tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                int newX = 2 * (x - _width / 2);
                int newY = 2 * (y - _height / 2);

                Tile spawnedTile = Instantiate(_tilePrefab, Vector3.zero, Quaternion.Euler(90, 0, 0), transform);
                spawnedTile.transform.localPosition = new Vector3(newX, 0, newY);
                spawnedTile.name = $"{newX},{newY}";

                spawnedTile.Init(newX, newY);

                _tiles[new Vector2(newX, newY)] = spawnedTile;
            }
        }
    }

    void CalculateMousePosition()
    {
        if (Camera.main == null) return;

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
        Vector3 localPos = pos - transform.position;

        if (_tiles.TryGetValue(new Vector2(Mathf.Round(localPos.x/2)*2, Mathf.Round(localPos.z/2)*2), out var tile))
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
        if (startTile == null) return new List<Tile>();
        if (targetTile == null) return new List<Tile>();

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

        Tile topRight = GetTileAtPosition(tile.X + 2, tile.Y + 2);
        Tile topLeft = GetTileAtPosition(tile.X - 2, tile.Y + 2);
        Tile botRight = GetTileAtPosition(tile.X + 2, tile.Y - 2);
        Tile botLeft = GetTileAtPosition(tile.X - 2, tile.Y - 2);

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
        if (topRight != null)
        {
            if (topRight.Walkable && !processed.Contains(topRight))
                neighbors.Add(topRight);
        }
        if (topLeft != null)
        {
            if (topLeft.Walkable && !processed.Contains(topLeft))
                neighbors.Add(topLeft);
        }
        if (botRight != null)
        {
            if (botRight.Walkable && !processed.Contains(botRight))
                neighbors.Add(botRight);
        }
        if (botLeft != null)
        {
            if (botLeft.Walkable && !processed.Contains(botLeft))
                neighbors.Add(botLeft);
        }

        return neighbors;
    }

    public void ClearPath()
    {
        foreach (Tile t in _tiles.Values)
        {
            t.Pathed = false;
            t.Burning = false;
        }
    }

    public void PathTiles(List<Tile> path)
    {
        ClearPath();

        foreach(Tile t in path)
        {
            t.Pathed = true;
        }
    }

    public Tile GetRandomTile()
    {
        int randIndex = Random.Range(0, _tiles.Values.Count);
        return _tiles.Values.ToList()[randIndex];
    }

    public Tile GetRandomTileAwayFromPlayer(float distance)
    {
        List<Tile> validTiles = new List<Tile>();

        foreach(Tile t in _tiles.Values)
        {
            if(Vector3.Distance(t.transform.position, PlayerController.Instance.transform.position) > distance)
            {
                validTiles.Add(t);
            }
        }

        int randIndex = Random.Range(0, validTiles.Count);
        return validTiles[randIndex];
    }

    public bool IsPathStraight(List<Tile> path)
    {
        if (path == null) return false;
        if (path.Count == 0) return false;

        bool hStraight = false;
        bool vStraight = false;

        Tile firstTile = path[0];

        // check vertical
        foreach(Tile t in path)
        {
            if(firstTile.X == t.X)
            {
                if(firstTile.Y != t.Y)
                {
                    vStraight = true;
                }
            }
            else
            {
                vStraight = false;
            }
        }

        // check horizontal
        foreach (Tile t in path)
        {
            if (firstTile.Y == t.Y)
            {
                if (firstTile.X != t.X)
                {
                    hStraight = true;
                }
            }
            else
            {
                hStraight = false;
            }
        }

        return hStraight || vStraight;
    }

    public bool IsPathDiagonal(List<Tile> path)
    {
        if (path == null) return false;
        if (path.Count == 0) return false;

        Tile current = path[0];

        for(int i = 1; i < path.Count; i++)
        {
            if(path[i].X != current.X && path[i].Y != current.Y)
            {
                current = path[i];
                continue;
            }
            return false;
        }

        return true;
    }

    public List<Tile> PathCrossPattern(Tile t)
    {
        List<Tile> crossedTiles = new List<Tile>();

        for (int x = 0; x < _height; x++)
        {
            int newX = 2 * (x - _width / 2);

            _tiles[new Vector2(newX, t.Y)].Pathed = true;

            crossedTiles.Add(_tiles[new Vector2(newX, t.Y)]);
        }

        for (int y = 0; y < _width; y++)
        {
            int newY = 2 * (y - _height / 2);

            _tiles[new Vector2(t.X, newY)].Pathed = true;

            crossedTiles.Add(_tiles[new Vector2(t.X, newY)]);
        }

        return crossedTiles;
    }

    public List<Tile> BurnCrossPattern(Tile t)
    {
        List<Tile> crossedTiles = new List<Tile>();

        for (int x = 0; x < _height; x++)
        {
            int newX = 2 * (x - _width / 2);

            crossedTiles.Add(_tiles[new Vector2(newX, t.Y)]);
        }

        for (int y = 0; y < _width; y++)
        {
            int newY = 2 * (y - _height / 2);

            crossedTiles.Add(_tiles[new Vector2(t.X, newY)]);
        }

        return crossedTiles;
    }

    public List<Tile> PathDiagonalPattern(Tile t)
    {
        List<Tile> diagonalTiles = new List<Tile>();

        int x = 0;
        int y = 0;
        while (_tiles.ContainsKey(new Vector2(t.X + x, t.Y + y)))
        {
            if (!diagonalTiles.Contains(_tiles[new Vector2(t.X + x, t.Y + y)]))
            {
                _tiles[new Vector2(t.X + x, t.Y + y)].Pathed = true;
                diagonalTiles.Add(_tiles[new Vector2(t.X + x, t.Y + y)]);
            }

            x += 2;
            y += 2;
        }

        x = 0;
        y = 0;
        while (_tiles.ContainsKey(new Vector2(t.X + x, t.Y + y)))
        {
            if (!diagonalTiles.Contains(_tiles[new Vector2(t.X + x, t.Y + y)]))
            {
                _tiles[new Vector2(t.X + x, t.Y + y)].Pathed = true;
                diagonalTiles.Add(_tiles[new Vector2(t.X + x, t.Y + y)]);
            }

            x -= 2;
            y -= 2;
        }

        x = 0;
        y = 0;
        while (_tiles.ContainsKey(new Vector2(t.X + x, t.Y + y)))
        {
            if (!diagonalTiles.Contains(_tiles[new Vector2(t.X + x, t.Y + y)]))
            {
                _tiles[new Vector2(t.X + x, t.Y + y)].Pathed = true;
                diagonalTiles.Add(_tiles[new Vector2(t.X + x, t.Y + y)]);
            }

            x += 2;
            y -= 2;
        }

        x = 0;
        y = 0;
        while (_tiles.ContainsKey(new Vector2(t.X + x, t.Y + y)))
        {
            if (!diagonalTiles.Contains(_tiles[new Vector2(t.X + x, t.Y + y)]))
            {
                _tiles[new Vector2(t.X + x, t.Y + y)].Pathed = true;
                diagonalTiles.Add(_tiles[new Vector2(t.X + x, t.Y + y)]);
            }

            x -= 2;
            y += 2;
        }

        return diagonalTiles;
    }

    public List<Tile> BurnDiagonalPattern(Tile t)
    {
        List<Tile> diagonalTiles = new List<Tile>();

        int x = 0;
        int y = 0;
        while (_tiles.ContainsKey(new Vector2(t.X + x, t.Y + y)))
        {
            if (!diagonalTiles.Contains(_tiles[new Vector2(t.X + x, t.Y + y)]))
            {
                diagonalTiles.Add(_tiles[new Vector2(t.X + x, t.Y + y)]);
            }

            x += 2;
            y += 2;
        }

        x = 0;
        y = 0;
        while (_tiles.ContainsKey(new Vector2(t.X + x, t.Y + y)))
        {
            if (!diagonalTiles.Contains(_tiles[new Vector2(t.X + x, t.Y + y)]))
            {
                diagonalTiles.Add(_tiles[new Vector2(t.X + x, t.Y + y)]);
            }

            x -= 2;
            y -= 2;
        }

        x = 0;
        y = 0;
        while (_tiles.ContainsKey(new Vector2(t.X + x, t.Y + y)))
        {
            if (!diagonalTiles.Contains(_tiles[new Vector2(t.X + x, t.Y + y)]))
            {
                diagonalTiles.Add(_tiles[new Vector2(t.X + x, t.Y + y)]);
            }

            x += 2;
            y -= 2;
        }

        x = 0;
        y = 0;
        while (_tiles.ContainsKey(new Vector2(t.X + x, t.Y + y)))
        {
            if (!diagonalTiles.Contains(_tiles[new Vector2(t.X + x, t.Y + y)]))
            {
                diagonalTiles.Add(_tiles[new Vector2(t.X + x, t.Y + y)]);
            }

            x -= 2;
            y += 2;
        }

        return diagonalTiles;
    }

    public List<Tile> BurnCheckerBoard(Tile bossTile, bool black)
    {
        List<Tile> checkerTiles = new List<Tile>();

        if (black)
        {
            foreach (Tile t in _tiles.Values)
            {
                if (t.Black)
                {
                    checkerTiles.Add(t);
                }
            }
        }
        else
        {
            foreach (Tile t in _tiles.Values)
            {
                if (!t.Black)
                {
                    checkerTiles.Add(t);
                }
            }
        }

        checkerTiles = checkerTiles.OrderBy(tile => tile.GetDistance(bossTile)).ToList();

        return checkerTiles;
    }

    public List<Tile> PathCheckerBoard(bool black)
    {
        List<Tile> checkerTiles = new List<Tile>();

        if (black)
        {
            foreach (Tile t in _tiles.Values)
            {
                if (t.Black)
                {
                    t.Pathed = true;
                    checkerTiles.Add(t);
                }
            }
        }
        else
        {
            foreach (Tile t in _tiles.Values)
            {
                if (!t.Black)
                {
                    t.Pathed = true;
                    checkerTiles.Add(t);
                }
            }
        }

        return checkerTiles;
    }
}