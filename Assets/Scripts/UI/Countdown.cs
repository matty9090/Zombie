using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    [SerializeField] private Game Game = null;

    void PlayTick()
    {
        Game.AudioManager.Play("Tick");
    }
}
