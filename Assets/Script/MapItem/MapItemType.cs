using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapItemType : MonoBehaviour
{
    public enum TypeEnum
    {
        Null,       // For Error
        StoneWall, //石墙
        MapBorder, //地图边缘
        GrassWall, //草墙
        DirtBlock,
    }
    public TypeEnum type;
}
