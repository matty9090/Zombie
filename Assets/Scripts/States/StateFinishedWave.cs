using System.Collections;
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
        mGame.FinishedWave.GetComponent<Animator>().Play("FadeOut");
    }

    public void Update()
    {
        
    }
}
