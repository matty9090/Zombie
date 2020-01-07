using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour
{
    [SerializeField] private GameObject HealthBar = null;

    public int MaxHealth = 100;
    private int mHealth = 100;

    public int Health {
        get { return mHealth; } 
        set { mHealth = value; UpdateHealth(); }
    }

    private GameObject HealthBarInst;

    private void Start()
    {
        var game = GameObject.Find("Game").GetComponent<Game>();

        game.MatchStarted.AddListener(OnMatchStarted);
        game.MatchEnded.AddListener(OnMatchEnded);
        
        Health = MaxHealth;
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
        
    }

    private void OnMatchEnded()
    {
        HealthBarInst.SetActive(false);
    }

    private void UpdateHealth()
    {
        if (HealthBarInst == null)
            return;
        
        float frac = (float)mHealth / MaxHealth;
        HealthBarInst.GetComponentInChildren<Image>().transform.localScale = new Vector3(Mathf.Clamp(frac, 0.0f, 1.0f), 1, 1);

        if (frac <= 0.0f)
        {
            Destroy(gameObject);
        }
        else if (frac < 1.0f)
        {
            HealthBarInst.SetActive(true);
        }
    }
}
