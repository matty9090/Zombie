using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMenu : IState
{
    private Game Game = null;
    private float FadeOutMusicTime = 1.6f;

    public StateMenu()
    {
        Game = GameObject.Find("Game").GetComponent<Game>();
    }

    public void OnEnter()
    {
        Game.Menu.enabled = true;
        Game.Hud.enabled = false;

        Game.AudioManager.Play("MenuMusic", true);
        Game.CharacterInst.gameObject.SetActive(false);
    }

    public void OnExit()
    {
        Game.Menu.enabled = false;
        Game.Hud.enabled = true;

        Game.AudioManager.FadeOutSound("MenuMusic", FadeOutMusicTime);
        Game.CharacterInst.gameObject.SetActive(true);
        Game.CharacterInst.transform.position = Game.Map.Start.Position;
        Game.CharacterInst.transform.rotation = Quaternion.identity;
        Game.CharacterInst.CurrentPosition = Game.Map.Start;
        Game.HoverTile.SetActive(true);
    }

    public void Update()
    {
        
    }
}
