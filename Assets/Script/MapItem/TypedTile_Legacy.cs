using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Tile", menuName = "CustomTile/TypedTile")]
public class TypedTile_Legacy : UnityEngine.Tilemaps.Tile
{
    public MapItemType.TypeEnum Type;
}
