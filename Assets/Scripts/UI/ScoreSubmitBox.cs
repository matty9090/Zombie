using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;

[System.Serializable]
struct LeaderboardSubmitResponseData
{
    public bool success;
    public string message;
}

public class ScoreSubmitBox : MonoBehaviour
{
    [SerializeField] InputField Input = null;
    [SerializeField] Button Btn = null;
    [SerializeField] Text BtnText = null;
    [SerializeField] Game Game = null;

    public void SubmitScore()
    {
        StartCoroutine(PostScore(Input.text, Game.CurrentWave));
    }

    private IEnumerator PostScore(string name, int score)
    {
        WWWForm form = new WWWForm();
        form.AddField("name", name);
        form.AddField("score", score);

        UnityWebRequest www = UnityWebRequest.Post(Game.ApiURL + "submit", form);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            BtnText.text = "Error";
        }
        else
        {
            var response = Encoding.UTF8.GetString(www.downloadHandler.data);
            var json = JsonUtility.FromJson<LeaderboardSubmitResponseData>(response);

            if (json.success)
            {
                Btn.interactable = false;
                Input.interactable = false;
                BtnText.text = "Submitted";
            }
            else
            {
                Debug.LogWarning("Error submitting (" + json.message + ")");
                BtnText.text = "Error";
            }
        }
    }
}
