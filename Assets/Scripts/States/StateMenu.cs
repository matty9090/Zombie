using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMenu : IState
{
    private Game Game = null;

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
                Game.MainCamera.GetComponent<FollowCamera>().enabled = false;
                Game.CharacterInst.transform.position = Game.CharacterStart.position;
                Game.CharacterInst.transform.rotation = Game.CharacterStart.rotation;
                Game.Map.CleanUpWorld();
            }
            else
            {
                Game.MainCamera.GetComponent<FollowCamera>().enabled = true;
                Game.CharacterInst.transform.position = Game.Map.Start.Position;
                Game.CharacterInst.transform.rotation = Quaternion.identity;
                Game.CharacterInst.CurrentPosition = Game.Map.Start;
            }
        }
    }
}
