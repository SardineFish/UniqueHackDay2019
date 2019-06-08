using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class Tile2DMap : MonoBehaviour
{
    public Tilemap debugTilemap;
    public TileBase debugTile;
    private void OnCollisionStay2D(Collision2D collision)
    {
        var tilemap = GetComponent<Tilemap>();
        var tilePos = tilemap.WorldToCell(collision.transform.position);
        var tile = tilemap.GetTile(tilePos);
        Debug.Log(tilePos);
        debugTilemap.SetTile(tilePos, debugTile);
        debugTilemap.RefreshAllTiles();
    }
}
