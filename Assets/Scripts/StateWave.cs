using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateWave : IState
{
    private Game Game = null;

    public StateWave()
    {
        Game = GameObject.Find("Game").GetComponent<Game>();
    }

    public void OnEnter()
    {

    }

    public void OnExit()
    {

    }

    public void Update()
    {

    }
}
