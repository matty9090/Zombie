using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPBar : MonoBehaviour
{
    private Game mGame;

    public void ProvideGame(Game game)
    {
        mGame = game;
        mGame.XPChanged.AddListener(XPChanged);
    }

    private void XPChanged()
    {
        Vector3 scale = transform.localScale;
        scale.x = (float)mGame.XP / (float)mGame.CurrentXPCap;
        transform.localScale = scale;
    }
}
