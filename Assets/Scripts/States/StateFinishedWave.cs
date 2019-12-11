﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateFinishedWave : IState
{
    private Game mGame = null;

    public void OnEnter()
    {
        mGame = GameObject.Find("Game").GetComponent<Game>();
        mGame.FinishedWave.SetActive(true);
        mGame.FinishedWave.GetComponent<Animator>().Play("Fade");
    }

    public void OnExit()
    {
        mGame.Map.CleanUpWorld();
        mGame.Map.GenerateWorld();

        mGame.CharacterInst.transform.position = mGame.Map.Start.Position;
        mGame.CharacterInst.transform.rotation = Quaternion.identity;
        mGame.CharacterInst.CurrentPosition = mGame.Map.Start;

        mGame.FinishedWave.GetComponent<Animator>().Play("FadeOut");
    }

    public void Update()
    {
        
    }
}
