using UnityEngine;
using System.Collections;

public class TileMap : Singleton<TileMap>
{
    public float TileSize = 1;
    public TypedTile GetTile(Vector2Int position)
    {
        foreach(var tile in GetComponentsInChildren<TypedTile>())
        {
            if (WorldToGrid(tile.transform.position) == position)
                return tile;
        }
        return null;
    }

    public Vector2Int WorldToGrid(Vector2 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
    }

    public Vector2 GridToWorld(Vector2Int position)
    {
        return new Vector2(position.x, position.y);
    }

    public Vector2 GridCenter(Vector2Int position)
        => new Vector2(position.x, position.y) + new Vector2(TileSize / 2, TileSize / 2);
}
