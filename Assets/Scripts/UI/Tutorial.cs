using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    [System.Serializable]
    public struct Tut
    {
        public string Title;
        public string Desc;
    }

    [SerializeField] private Game Game;
    [SerializeField] private GameObject Panel;
    [SerializeField] private List<Tut> Tutorials = null;
    [SerializeField] private Text Title;
    [SerializeField] private Text Desc;

    private bool IsHidden = false;
    private int CurrentTut = 0;

    void Start()
    {
        ShowTut(0);
    }

    void Update()
    {
        
    }

    private void ShowTut(int id)
    {
        Title.text = Tutorials[id].Title + $" ({id + 1}/{Tutorials.Count})";
        Desc.text = Tutorials[id].Desc;
    }

    public void Next()
    {
        if (CurrentTut < Tutorials.Count - 1)
            ++CurrentTut;
        else
            Game.AudioManager.PlayError();

        ShowTut(CurrentTut);
    }

    public void Prev()
    {
        if (CurrentTut >= 1)
            --CurrentTut;
        else
            Game.AudioManager.PlayError();

        ShowTut(CurrentTut);
    }

    public void TogglePanel()
    {
        IsHidden = !IsHidden;
        Panel.SetActive(!IsHidden);
    }
}
