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

    [SerializeField] private Game Game = null;
    [SerializeField] private GameObject Panel = null;
    [SerializeField] private List<Tut> Tutorials = null;
    [SerializeField] private Text Title = null;
    [SerializeField] private Text Desc = null;

    private bool IsHidden = false;
    private int CurrentTut = 0;

    void Start()
    {
        // Default to first tutorial
        ShowTut(0);
    }

    void Update()
    {
        
    }

    /* Helper function to show the tutorial in the UI */
    private void ShowTut(int id)
    {
        Title.text = Tutorials[id].Title + $" ({id + 1}/{Tutorials.Count})";
        Desc.text = Tutorials[id].Desc;
    }

    /* Next tutorial button */
    public void Next()
    {
        if (CurrentTut < Tutorials.Count - 1)
            ++CurrentTut;
        else
            Game.AudioManager.PlayError();

        ShowTut(CurrentTut);
    }

    /* Previous tutorial button */
    public void Prev()
    {
        if (CurrentTut >= 1)
            --CurrentTut;
        else
            Game.AudioManager.PlayError();

        ShowTut(CurrentTut);
    }

    /* Toggle the tutorial UI */
    public void TogglePanel()
    {
        IsHidden = !IsHidden;
        Panel.SetActive(!IsHidden);
    }
}
