using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapItemType : MonoBehaviour
{
    public enum TypeEnum
    {
        Null,       // For Error
        StoneWall, //石墙
        MapBorder, //地图边缘
        GrassWall, //草墙
        DirtBlock,
        TeleportBlock,
    }
    public abstract TypeEnum GetTypeFromContact(ContactPoint2D contact);
}
