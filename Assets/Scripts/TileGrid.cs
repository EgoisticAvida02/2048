using UnityEngine;

public class TileGrid : MonoBehaviour
{
    public TileRow[] rows { get; private set; }
    public TileCell[] cells { get; private set; }
    public int size => cells.Length;
    public int height => rows.Length;
    public int width => size / height;
    private void Awake()
    {
        rows = GetComponentsInChildren<TileRow>();
        cells = GetComponentsInChildren<TileCell>();
    }

    private void Start()
    {
        for (int y = 0; y < rows.Length; y++)
        {
            for (int x = 0; x < rows[y].cells.Length; x++)
            {
                rows[y].cells[x].coordinates = new Vector2Int(x, y);
            }
        }
    }

    public TileCell GetCell(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return rows[y].cells[x];
        }
        else
        {
            return null;    
        }
    }

    public TileCell GetCell(Vector2Int coordinates)
    {
        return GetCell(coordinates.x, coordinates.y);
    }

    public TileCell GetAdjacentCell(TileCell cell, Vector2Int direction)
    {
        Vector2Int coordinate = cell.coordinates;
        coordinate.x += direction.x;
        coordinate.y -= direction.y;

        return GetCell(coordinate);
    }

    public TileCell GetRandomEmptyCell()
    {
        int idx = Random.Range(0, cells.Length);
        int startingIdx = idx;

        while (cells[idx].occupied)
        {
            idx++;
            if (idx >= cells.Length)
            {
                idx = 0;
            }

            if (idx == startingIdx)
            {
                return null;
            }
        }
        return cells[idx];
    }


}
