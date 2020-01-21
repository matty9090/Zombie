using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Menu state */
public class StateMenu : IState
{
    private Game mGame = null;
    private float mFadeOutMusicTime = 1.6f;

    public StateMenu()
    {
        mGame = GameObject.Find("Game").GetComponent<Game>();
    }

    public void OnEnter()
    {
        // Show menu UI
        mGame.Menu.enabled = true;
        mGame.Hud.enabled = false;

        mGame.AudioManager.Play("MenuMusic", true);

        // Hide character
        mGame.CharacterInst.gameObject.SetActive(false);
    }

    public void OnExit()
    {
        mGame.Menu.enabled = false;
        mGame.Hud.enabled = true;

        mGame.AudioManager.FadeOutSound("MenuMusic", mFadeOutMusicTime);
        mGame.HoverTile.SetActive(true);

        // Reset character position to centre of map
        mGame.CharacterInst.gameObject.SetActive(true);
        mGame.CharacterInst.transform.position = mGame.Map.Start.Position;
        mGame.CharacterInst.transform.rotation = Quaternion.identity;
        mGame.CharacterInst.CurrentPosition = mGame.Map.Start;
    }

    public void Update()
    {
        
    }
}
