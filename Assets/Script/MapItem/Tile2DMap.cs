using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class Tile2DMap : MapItemType
{
    public Tilemap debugTilemap;
    public TileBase debugTile;
    private void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log(collision.contactCount);
        var tilemap = GetComponent<Tilemap>();
        var tilePos = tilemap.WorldToCell(collision.transform.position);
        var tile = tilemap.GetTile(tilePos);
        //Debug.Log(tilePos);
        debugTilemap.SetTile(tilePos, debugTile);
        debugTilemap.RefreshAllTiles();
        for(var i=0;i<collision.contactCount;i++)
        {
            var contact = collision.GetContact(i);
            Debug.DrawLine(contact.point, contact.point + contact.normal, Color.cyan);
        }
    }

    public MapItemType.TypeEnum GetTileType(ContactPoint2D contact)
    {
        var tilemap = GetComponent<UnityEngine.Tilemaps.Tilemap>();
        var normal = -contact.normal;
        var tilePos = tilemap.WorldToCell(contact.point);
        tilePos += new Vector3Int(Mathf.RoundToInt(normal.x), Mathf.RoundToInt(normal.y), 0);
        var tile = tilemap.GetTile<TypedTile_Legacy>(tilePos);
        return tile == null ? MapItemType.TypeEnum.Null : tile.Type;
    }

    public override TypeEnum GetTypeFromContact(ContactPoint2D contact)
    {
        return GetTileType(contact);
    }
}
