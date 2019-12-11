using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    [SerializeField] private HealthBar HealthBar = null;
    [SerializeField] private Text UIWaveText = null;
    [SerializeField] private Text UICountdownText = null;
    [SerializeField] private int BuildingTime = 90;

    public Canvas Menu = null;
    public Canvas Hud = null;
    public Camera MainCamera = null;
    public Character Character = null;
    public Zombie Zombie = null;
    public GameObject HoverTile = null;
    public GameObject GameOver = null;
    public GameObject FinishedWave = null;
    public Transform CharacterStart = null;
    public Material HoverMaterialG = null;
    public Material HoverMaterialR = null;
    public Texture2D CursorNormal = null;
    public Texture2D CursorBuild = null;
    public Texture2D CursorFight = null;
    public Color DayColour;
    public Color NightColour;

    public Character CharacterInst { get; private set;  }
    public Resources Resources { get; set; }
    public Environment Map { get; private set; }
    public AudioManager AudioManager { get; private set; }
    public UnityEvent ZombieKilled { get; private set; }

    private enum EGameState { Menu, Building, Wave, FinishedWave, GameOver };
    private EGameState mGameState = EGameState.Menu;
    private int CurrentWave = 0;
    private float BuildingTimer = 0.0f;

    private Dictionary<EGameState, IState> mStates;

    void Start()
    {
        Resources = new Resources();
        Map = GetComponentInChildren<Environment>();
        CharacterInst = Instantiate(Character, transform);
        BuildingTimer = (float)BuildingTime;
        HealthBar.ProvideCharacter(CharacterInst);
        MainCamera.GetComponent<ICamera>().SetCharacter(CharacterInst);
        AudioManager = GetComponent<AudioManager>();

        ZombieKilled = new UnityEvent();
        ZombieKilled.AddListener(ZombieKilledEvent);
        CharacterInst.HealthChangedEvent.AddListener(CharacterHealthChanged);

        Cursor.SetCursor(CursorNormal, Vector2.zero, CursorMode.ForceSoftware);

        mStates = new Dictionary<EGameState, IState>
        {
            [EGameState.Menu]         = new StateMenu(),
            [EGameState.Building]     = new StateBuilding(),
            [EGameState.Wave]         = new StateWave(),
            [EGameState.FinishedWave] = new StateFinishedWave(),
            [EGameState.GameOver]     = new StateGameOver()
        };

        mStates[mGameState].OnEnter();
    }

    private void SwitchState(EGameState state)
    {
        mStates[mGameState].OnExit();
        mGameState = state;
        mStates[mGameState].OnEnter();
    }

    void Update()
    {
        if (mGameState == EGameState.Building)
        {
            BuildingTimer -= Time.deltaTime;
            UICountdownText.text = FormatTime(BuildingTimer);

            if (BuildingTimer < 10.0f)
                UICountdownText.GetComponent<Animator>().Play("FlashText");

            if (BuildingTimer <= 0.0f)
            {
                BuildingTimer = BuildingTime;
                ++CurrentWave;
                UIWaveText.text = "Wave " + CurrentWave;
                UICountdownText.GetComponent<Animator>().Play("Countdown");
                SwitchState(EGameState.Wave);
            }
        }

        mStates[mGameState].Update();
    }

    private string FormatTime(float time)
    {
        int t = Mathf.CeilToInt(time);
        int mins = t / 60;
        t -= mins * 60;

        string res = (mins < 10) ? "0" : "";
        res += mins + ":";
        res += (t < 10) ? "0" : "";

        return res + t;
    }

    private void CharacterHealthChanged()
    {
        if (CharacterInst.Health <= 0 && mGameState == EGameState.Wave)
        {
            SwitchState(EGameState.GameOver);
        }
    }

    private void ZombieKilledEvent()
    {
        foreach (var z in FindObjectsOfType<Zombie>())
        {
            if (z != null && z.Health > 0)
            {
                return;
            }
        }

        if (mGameState == EGameState.Wave && CharacterInst.Health > 0)
        {
            SwitchState(EGameState.FinishedWave);
            StartCoroutine(FinishedWaveTimer());
        }
    }

    private IEnumerator FinishedWaveTimer()
    {
        float timer = 4.0f;

        while (timer >= 0.0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        SwitchState(EGameState.Building);
    }

    public void ShowMenu(bool show)
    {
        SwitchState(show ? EGameState.Menu : EGameState.Building);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("Main");
    }

    public void UIBuildingClicked(UIBuilding element)
    {
        var BuildingState = mStates[EGameState.Building];
        ((StateBuilding)BuildingState).UIBuildingClicked(element);
    }

    public void Generate()
    {
        Map.GenerateWorld();
    }

    public void Exit()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
