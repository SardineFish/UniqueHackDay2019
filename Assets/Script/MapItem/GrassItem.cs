using UnityEngine;
using System.Collections;

public class GrassItem : MapItem
{
    public GameObject Fire;
    public override void OnPlayerTouch()
    {
        Instantiate(Fire, transform.position, Quaternion.identity);
    }
}
