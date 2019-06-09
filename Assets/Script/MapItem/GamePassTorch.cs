using UnityEngine;
using System.Collections;

public class GamePassTorch : MapItem
{
    public GamePassTorch()
    {
    }

    public override void OnPlayerTouch()
    {
        MainUI.Instance.Pass();
    }
}
