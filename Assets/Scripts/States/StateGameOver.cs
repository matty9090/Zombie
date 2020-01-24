using UnityEngine;
using UnityEngine.UI;

/* Game over state */
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
        // Destroy character
        Object.Destroy(mGame.CharacterInst);

        // Fade in game over screen
        mAnim = mGame.GameOver.GetComponentInChildren<Animator>();
        mGame.GameOver.SetActive(true);

        // Make sure submit score box is hidden initially
        mGame.CloseScoreSubmit();

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
