using UnityEngine;
using System.Collections;

public class GrassItem : MapItem
{
    public GameObject Fire;
    public Sprite BurntGrass;
    public Sprite NormalGrass;
    public float BurnTime = 2;
    public float GrowTime = 2;
    bool burnt = false;
    public override void OnPlayerTouch()
    {
        if (burnt)
            return;
        burnt = true;
        StartCoroutine(UnnamedFunc());
    }
    IEnumerator UnnamedFunc()
    {
        var fx = Instantiate(Fire, transform.position, Quaternion.identity);
        foreach(var t in Utility.Timer(BurnTime))
        {
            yield return null;
        }
        GetComponentInChildren<SpriteRenderer>().sprite = BurntGrass;
        Destroy(fx);
        foreach (var t in Utility.Timer(GrowTime))
        {
            yield return null;
        }
        GetComponentInChildren<SpriteRenderer>().sprite = NormalGrass;
        burnt = false;
    }
}
