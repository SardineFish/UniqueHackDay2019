using UnityEngine;
using System.Collections;

public class TileMap : MonoBehaviour
{
    public float TileSize = 1;
    public void GetTile(Vector2Int position)
    {

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
