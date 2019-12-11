using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveUI : MonoBehaviour
{
    [SerializeField] private Game Game = null;

    public void FinishedFade()
    {
        Game.FinishedWave.SetActive(false);
    }
}
