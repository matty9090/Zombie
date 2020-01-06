using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] private GameObject HealthBar = null;
    private GameObject HealthBarInst;

    private void Start()
    {
        var game = GameObject.Find("Game").GetComponent<Game>();

        game.MatchStarted.AddListener(OnMatchStarted);
        game.MatchEnded.AddListener(OnMatchEnded);
    }

    private void OnDestroy()
    {
        var game = GameObject.Find("Game");

        if (game)
        {
            game.GetComponent<Game>().MatchStarted.RemoveListener(OnMatchStarted);
            game.GetComponent<Game>().MatchEnded.RemoveListener(OnMatchEnded);
        }
    }

    public void Place()
    {
        HealthBarInst = Instantiate(HealthBar);
        HealthBarInst.SetActive(false);

        if(HealthBarInst != null)
            HealthBarInst.transform.position = transform.position + Vector3.up * 9.4f;
    }

    private void OnMatchStarted()
    {
        HealthBarInst.SetActive(true);
    }

    private void OnMatchEnded()
    {
        HealthBarInst.SetActive(false);
    }
}
