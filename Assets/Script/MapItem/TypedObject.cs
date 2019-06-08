using UnityEngine;
using System.Collections;

public class TypedObject : MapItemType
{
    public TypeEnum Type;
    public override TypeEnum GetTypeFromContact(ContactPoint2D contact)
    {
        return Type;
    }
}
