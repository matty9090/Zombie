using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    [SerializeField] private Game Game = null;

    public void BackToMainMenu()
    {
        Game.BackToMainMenu();
    }
}
