using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TileBoard : MonoBehaviour
{
    public GameManager gameManager;
    public Tile tilePrefab;
    public TileState[] tileStates;
    private TileGrid grid;
    private List<Tile> tiles;
    private bool isWaiting;
    private void Awake()
    {
        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>(16);
    }

    private void Update()
    {
        if (!isWaiting)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveTiles(Vector2Int.up, 0, 1, 1, 1);
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveTiles(Vector2Int.down, 0, 1, grid.height - 2, -1);
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveTiles(Vector2Int.left, 1, 1, 0, 1);
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveTiles(Vector2Int.right, grid.width - 2, -1, 0, 1);
            }
        }
    }

    public void CreateTile()
    {
        Tile tile = Instantiate(tilePrefab, grid.transform);
        tile.SetState(tileStates[0], 2);
        tile.Spawm(grid.GetRandomEmptyCell());
        tiles.Add(tile);
    }

    public void ClearBoard()
    {
        foreach (var cell in grid.cells)
        {
            cell.tile = null;
        }
        foreach (var tile in tiles)
        {
            Destroy(tile.gameObject);
        }

        tiles.Clear();
    }

    private void MoveTiles(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
    {
        bool changed = false;

        for (int x = startX; x >= 0 && x < grid.width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < grid.height; y += incrementY)
            {
                TileCell cell = grid.GetCell(x, y);
                if (cell.occupied)
                {
                    changed |= MoveTile(cell.tile, direction);
                }
            }
        }

        if (changed)
        {
            StartCoroutine(WaitForChange());
        }
    }

    private bool MoveTile(Tile tile, Vector2Int direction)
    {
        TileCell newCell = null;
        TileCell adjacent = grid.GetAdjacentCell(tile.cell, direction);

        while (adjacent != null)
        {
            if (adjacent.occupied)
            {
                if (isMergeable(tile, adjacent.tile))
                {
                    Merge(tile, adjacent.tile);
                    return true;
                }
                break;
            }

            newCell = adjacent;
            adjacent = grid.GetAdjacentCell(adjacent, direction);
        }

        if (newCell != null)
        {
            tile.MoveTo(newCell);
            return true;
        }
        return false;
    }

    private bool isMergeable(Tile a, Tile b)
    {
        return a.number == b.number && !b.locked;
    }

    private void Merge(Tile a, Tile b)
    {
        tiles.Remove(a);
        a.Merge(b.cell);

        int idx = Mathf.Clamp(IdxOf(b.state) + 1, 0, tileStates.Length - 1);
        int number = b.number * 2;
        b.SetState(tileStates[idx], number);

        gameManager.IncreaseScore(number);
    }

    private int IdxOf(TileState state)
    {
        for (int i = 0; i < tileStates.Length; i++)
        {
            if (state == tileStates[i])
            {
                return i;
            }
        }
        return -1;
    }

    private IEnumerator WaitForChange()
    {
        isWaiting = true;
        yield return new WaitForSeconds(0.075f);
        isWaiting = false;

        foreach (var tile in tiles)
        {
            tile.locked = false;
        }
        if (tiles.Count != grid.size)
        {
            CreateTile();
        }

        if (CheckForGameOver())
        {
            gameManager.GameOver();
        }
    }

    public bool CheckForGameOver()
    {
        if (tiles.Count != grid.size)
        {
            return false;
        }

        foreach (var tile in tiles)
        {
            TileCell up = grid.GetAdjacentCell(tile.cell, Vector2Int.up);
            TileCell down = grid.GetAdjacentCell(tile.cell, Vector2Int.down);
            TileCell left = grid.GetAdjacentCell(tile.cell, Vector2Int.left);
            TileCell right = grid.GetAdjacentCell(tile.cell, Vector2Int.right);

            if (up != null && isMergeable(tile, up.tile))
            {
                return false;
            }
            else if (down != null && isMergeable(tile, down.tile))
            {
                return false;
            }
            else if (left != null && isMergeable(tile, left.tile))
            {
                return false;
            }
            else if (right != null && isMergeable(tile, right.tile))
            {
                return false;
            }
        }

        return true;
    }
}
