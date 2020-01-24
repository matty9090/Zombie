using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private Transform Content = null;
    [SerializeField] private LeaderboardResult ResultPrefab = null;

    public UnityEvent RefreshEvent = new UnityEvent();

    public void Clear()
    {
        for (int i = 0; i < Content.childCount; ++i)
            Destroy(Content.GetChild(i).gameObject);
    }

    public void AddRow(int id, string name, int score)
    {
        var result = Instantiate(ResultPrefab);
        result.GetComponent<LeaderboardResult>().Name.text = id + " " + name;
        result.GetComponent<LeaderboardResult>().Score.text = "Wave " + score;
        result.transform.SetParent(Content);
        result.transform.SetAsLastSibling();
    }

    public void RequestRefresh()
    {
        RefreshEvent.Invoke();
    }
}
