using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
struct LeaderboardResultData
{
    public int id;
    public string name;
    public int score;
    public int date;
}

[System.Serializable]
struct LeaderboardResponseData
{
    public bool success;
    public string message;
    public List<LeaderboardResultData> scores;
}

/* Menu state */
public class StateLeaderboard : IState
{
    private Game mGame = null;
    private Leaderboard mLeaderboard = null;

    public StateLeaderboard()
    {
        mGame = GameObject.Find("Game").GetComponent<Game>();
        mLeaderboard = mGame.Leaderboard.GetComponent<Leaderboard>();
        mLeaderboard.RefreshEvent.AddListener(OnRefresh);
    }

    public void OnEnter()
    {
        // Show leaderboard UI
        mGame.Menu.enabled = false;
        mGame.Hud.enabled = false;
        mGame.Leaderboard.gameObject.SetActive(true);

        mGame.MainCamera.enabled = false;
        mGame.RotationCamera.enabled = true;

        // Hide character
        mGame.CharacterInst.gameObject.SetActive(false);
        mGame.StartCoroutine(Refresh());
    }

    public void OnExit()
    {
        mGame.Menu.enabled = false;
        mGame.Hud.enabled = true;
        mGame.RotationCamera.enabled = false;
        mGame.MainCamera.enabled = true;
        mGame.HoverTile.SetActive(true);

        // Reset character position to centre of map
        mGame.CharacterInst.gameObject.SetActive(true);
        mGame.CharacterInst.transform.position = mGame.Map.Start.Position;
        mGame.CharacterInst.transform.rotation = Quaternion.identity;
        mGame.CharacterInst.CurrentPosition = mGame.Map.Start;
    }

    public void Update()
    {
        
    }

    private void OnRefresh()
    {
        mGame.StartCoroutine(Refresh());
    }

    private IEnumerator Refresh()
    {
        mLeaderboard.Clear();

        var data = new List<IMultipartFormSection>();

        UnityWebRequest www = UnityWebRequest.Post(mGame.ApiURL + "leaderboard", data);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            var response = Encoding.UTF8.GetString(www.downloadHandler.data);
            var json = JsonUtility.FromJson<LeaderboardResponseData>(response);

            if (json.success)
            {
                Debug.Log($"Received {json.scores.Count} leaderboard results");
                int rank = 1;

                foreach (var result in json.scores)
                {
                    mLeaderboard.AddRow(rank, result.name, result.score);
                    ++rank;
                }
            }
            else
            {
                Debug.LogWarning($"Error reading leaderboard results (" + json.message + ")");
            }
        }
    }
}
