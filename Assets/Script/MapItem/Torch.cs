using UnityEngine;

public class Torch : MapItemType
{
    public Torch Opposite;
    public override TypeEnum GetTypeFromContact(ContactPoint2D contact)
    {
        return TypeEnum.TeleportBlock;
    }
}
