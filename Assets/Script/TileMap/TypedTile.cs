using UnityEngine;
using System.Collections;

public class TypedTile : MapItemType
{
    public TypeEnum Type;

    public override TypeEnum GetTypeFromContact(ContactPoint2D contact)
    {
        return Type;
    }
}
