using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour
{
    [SerializeField] private GameObject HealthBar = null;

    public string BuildingName;
    public string BuildingDesc;

    public int MaxHealth = 100;
    private int mHealth = 100;

    public int Health {
        get { return mHealth; } 
        set { mHealth = value; UpdateHealth(); }
    }

    private GameObject mHealthBarInst;

    private void Start()
    {
        var game = GameObject.Find("Game").GetComponent<Game>();
        game.MatchEnded.AddListener(OnMatchEnded);
        
        Health = MaxHealth;
    }

    private void OnDestroy()
    {
        var game = GameObject.Find("Game");

        if (game)
        {
            game.GetComponent<Game>().MatchEnded.RemoveListener(OnMatchEnded);
            game.GetComponent<Game>().AudioManager.Play("Rubble");
        }
    }

    public void Place()
    {
        // Create health bar
        mHealthBarInst = Instantiate(HealthBar);
        mHealthBarInst.SetActive(false);

        if(mHealthBarInst != null)
            mHealthBarInst.transform.position = transform.position + Vector3.up * 9.4f;
    }

    /* Don't show health bar when the wave has ended */
    private void OnMatchEnded()
    {
        mHealthBarInst.SetActive(false);
    }

    private void UpdateHealth()
    {
        if (mHealthBarInst == null)
            return;
        
        float frac = (float)mHealth / MaxHealth;
        mHealthBarInst.GetComponentInChildren<Image>().transform.localScale = new Vector3(Mathf.Clamp(frac, 0.0f, 1.0f), 1, 1);

        if (frac <= 0.0f)
        {
            // Destroy health bar and building if health <= 0
            Destroy(mHealthBarInst);
            Destroy(gameObject);
        }
        else if (frac < 1.0f)
        {
            // Only show health bar if building is damaged
            mHealthBarInst.SetActive(true);
        }
    }
}
