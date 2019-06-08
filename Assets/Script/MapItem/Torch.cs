using UnityEngine;
using System.Collections;

public class TeleportBlock : MapItemType
{
    public TeleportBlock Opposite;
    public override TypeEnum GetTypeFromContact(ContactPoint2D contact)
    {
        return TypeEnum.TeleportBlock;
    }
}
