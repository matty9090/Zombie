using UnityEngine;
using UnityEngine.UI;

public class StateGameOver : IState
{
    private Game mGame = null;
    private Animator mAnim = null;

    public StateGameOver()
    {
        mGame = GameObject.Find("Game").GetComponent<Game>();
    }

    public void OnEnter()
    {
        mAnim = mGame.GameOver.GetComponentInChildren<Animator>();
        mGame.GameOver.SetActive(true);

        GameObject.Find("GameOverBackBtn").GetComponent<Button>().onClick.AddListener(BackClicked);
    }

    public void OnExit()
    {
        
    }

    public void Update()
    {
        
    }

    public void BackClicked()
    {
        mAnim.SetTrigger("Close");
    }
}
