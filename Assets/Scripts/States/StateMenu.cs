using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMenu : IState
{
    private Game Game = null;
    private float FadeOutMusicTime = 1.6f;
    private ICamera CachedCamera = null;

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
                foreach (ICamera cam in Game.MainCamera.GetComponents<ICamera>())
                {
                    if (cam.IsEnabled())
                    {
                        CachedCamera = cam;
                        break;
                    }
                }

                Game.AudioManager.Play("MenuMusic");
                CachedCamera.SetEnabled(false);
                Game.CharacterInst.transform.position = Game.CharacterStart.position;
                Game.CharacterInst.transform.rotation = Game.CharacterStart.rotation;
                Game.MainCamera.transform.position = Game.InitialCamPosition;
                Game.MainCamera.transform.rotation = Game.InitialCamRotation;
                Game.Restart();
                Game.HoverTile.SetActive(false);
            }
            else
            {
                Game.AudioManager.FadeOutSound("MenuMusic", FadeOutMusicTime);
                CachedCamera.SetEnabled(true);
                Game.CharacterInst.transform.position = Game.Map.Start.Position;
                Game.CharacterInst.transform.rotation = Quaternion.identity;
                Game.CharacterInst.CurrentPosition = Game.Map.Start;
                Game.HoverTile.SetActive(true);
            }
        }
    }
}
