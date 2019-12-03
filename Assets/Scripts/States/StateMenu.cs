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
        ShowMenu(true);
    }

    public void OnExit()
    {
        ShowMenu(false);
    }

    public void Update()
    {
        
    }

    private void ShowMenu(bool show)
    {
        if (Game.Menu != null && Game.Hud != null)
        {
            Game.Menu.enabled = show;
            Game.Hud.enabled = !show;

            if (show)
            {
                Game.AudioManager.Play("MenuMusic");
                Game.MainCamera.GetComponent<ICamera>().SetEnabled(false);
                Game.CharacterInst.transform.position = Game.CharacterStart.position;
                Game.CharacterInst.transform.rotation = Game.CharacterStart.rotation;
                Game.Map.CleanUpWorld();
            }
            else
            {
                Game.AudioManager.FadeOutSound("MenuMusic", FadeOutMusicTime);
                Game.MainCamera.GetComponent<ICamera>().SetEnabled(true);
                Game.CharacterInst.transform.position = Game.Map.Start.Position;
                Game.CharacterInst.transform.rotation = Quaternion.identity;
                Game.CharacterInst.CurrentPosition = Game.Map.Start;
            }
        }
    }
}
