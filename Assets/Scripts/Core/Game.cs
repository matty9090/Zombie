using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    [SerializeField] private HealthBar HealthBar = null;
    [SerializeField] private XPBar XPBar = null;
    [SerializeField] private Text UIWaveText = null;
    [SerializeField] private Text UICountdownText = null;
    [SerializeField] private int BuildingTime = 90;
    [SerializeField] private List<ELevelUp> LevelUps = null;

    public Canvas Menu = null;
    public Canvas Hud = null;
    public Camera MainCamera = null;
    public Character Character = null;
    public Zombie Zombie = null;
    public GameObject HoverTile = null;
    public GameObject GameOver = null;
    public GameObject FinishedWave = null;
    public Material HoverMaterialG = null;
    public Material HoverMaterialR = null;
    public Texture2D CursorNormal = null;
    public Texture2D CursorBuild = null;
    public Texture2D CursorFight = null;
    public Color NightColour;
    public Gradient DayNightGradient;
    public List<GameObject> HarvestTools = null;
    public List<GameObject> AttackTools = null;
    public List<GameObject> Buildings = null;

    public Character CharacterInst { get; private set; }
    public Resources Resources { get; set; }
    public Environment Map { get; private set; }
    public AudioManager AudioManager { get; private set; }
    public float BuildingTimeProgress { get { return BuildingTimer / BuildingTime; } }
    public Color DayColour { get { return DayNightGradient.Evaluate(0.5f); } }
    public Vector3 InitialCamPosition;
    public Quaternion InitialCamRotation;
    public int XPGainOnBuildingPlace = 8;
    public int NumBuildingsUnlocked = 2;
    public int NumToolsUnlocked = 1;
    public int NumWeaponsUnlocked = 1;

    public UnityEvent ZombieKilled { get; private set; }
    public UnityEvent MatchStarted { get; private set; }
    public UnityEvent MatchEnded { get; private set; }
    public UnityEvent XPChanged { get; private set; }
    public UnityEvent BuildingUnlocked { get; private set; }
    public UnityEvent WeaponUnlocked { get; private set; }
    public UnityEvent ToolUnlocked { get; private set; }

    private enum EGameState { Menu, Building, Wave, FinishedWave, GameOver };
    private enum ELevelUp { Building, Tool, Weapon };
    private EGameState mGameState = EGameState.Menu;
    private int CurrentWave = 0;
    private float BuildingTimer = 0.0f;
    private bool IsPaused = false;

    private int mXP;
    public int XPLevel = 0;
    public int CurrentXPCap = 100;
    public int CurrentHarvestTool = 0;

    public int XP {
        get { return mXP; }
        set {
            mXP = value;

            if (mXP > CurrentXPCap)
                LevelUp();

            XPChanged.Invoke();
        }
    }

    private Dictionary<EGameState, IState> mStates;

    void Start()
    {
        Resources = new Resources();
        Map = GetComponentInChildren<Environment>();
        CharacterInst = Instantiate(Character, transform);
        BuildingTimer = (float)BuildingTime;
        MainCamera.GetComponent<ICamera>().SetCharacter(CharacterInst);
        AudioManager = GetComponent<AudioManager>();
        InitialCamPosition = MainCamera.transform.position;
        InitialCamRotation = MainCamera.transform.rotation;

        ZombieKilled = new UnityEvent();
        ZombieKilled.AddListener(ZombieKilledEvent);
        CharacterInst.HealthChangedEvent.AddListener(CharacterHealthChanged);

        MatchStarted = new UnityEvent();
        MatchEnded = new UnityEvent();
        XPChanged = new UnityEvent();
        BuildingUnlocked = new UnityEvent();
        WeaponUnlocked = new UnityEvent();
        ToolUnlocked = new UnityEvent();

        HealthBar.ProvideCharacter(CharacterInst);
        XPBar.ProvideGame(this);

        Cursor.SetCursor(CursorNormal, Vector2.zero, CursorMode.ForceSoftware);

        mStates = new Dictionary<EGameState, IState>
        {
            [EGameState.Menu] = new StateMenu(),
            [EGameState.Building] = new StateBuilding(),
            [EGameState.Wave] = new StateWave(),
            [EGameState.FinishedWave] = new StateFinishedWave(),
            [EGameState.GameOver] = new StateGameOver()
        };

        mStates[mGameState].OnEnter();
    }

    private void SwitchState(EGameState state)
    {
        mStates[mGameState].OnExit();
        mGameState = state;
        mStates[mGameState].OnEnter();
        Debug.Log("Switched to state " + state.ToString());
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
                StartWave();
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

    private void LevelUp()
    {
        mXP = 0;

        if (LevelUps.Count > XPLevel)
        {
            var type = LevelUps[XPLevel];

            switch (type)
            {
                case ELevelUp.Building:
                    NumBuildingsUnlocked = Mathf.Min(Buildings.Count, NumBuildingsUnlocked + 1);
                    BuildingUnlocked.Invoke();
                    break;

                case ELevelUp.Tool:
                    NumToolsUnlocked = Mathf.Min(HarvestTools.Count, NumToolsUnlocked + 1);
                    ToolUnlocked.Invoke();
                    break;

                case ELevelUp.Weapon:
                    NumWeaponsUnlocked = Mathf.Min(AttackTools.Count, NumWeaponsUnlocked + 1);
                    WeaponUnlocked.Invoke();
                    break;
            }
        }

        ++XPLevel;
        AudioManager.Play("Unlock");
    }

    private void CharacterHealthChanged()
    {
        if (CharacterInst.Health <= 0 && mGameState == EGameState.Wave)
        {
            GameOver.transform.Find("Result").GetComponent<Text>().text = "You made it to wave " + CurrentWave;
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
            MatchEnded.Invoke();
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

        GameObject.Find("StartWave").GetComponent<Button>().enabled = true;

        SwitchState(EGameState.Building);
    }

    public void StartWave()
    {
        BuildingTimer = BuildingTime;
        ++CurrentWave;
        
        UIWaveText.text = "Wave " + CurrentWave;
        UICountdownText.GetComponent<Animator>().Play("Countdown");

        GameObject.Find("StartWave").GetComponent<Button>().enabled = false;
        SwitchState(EGameState.Wave);
    }

    public void StartGame()
    {
        SwitchState(EGameState.Building);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Main");
    }

    public void PauseGame(GameObject pausePanel)
    {
        IsPaused = !IsPaused;
        pausePanel.SetActive(IsPaused);
        Time.timeScale = IsPaused ? 0.0f : 1.0f;
    }

    public void UIBuildingClicked(UIBuilding element)
    {
        var buildingState = mStates[EGameState.Building];
        
        if (mGameState == EGameState.Building)
            ((StateBuilding)buildingState).UIBuildingClicked(element);
    }

    public void UIWeaponClicked(UIWeapon element)
    {
        var waveState = mStates[EGameState.Wave];

        if (mGameState == EGameState.Wave)
            ((StateWave)waveState).UIWeaponClicked(element);
    }

    public void Generate()
    {
        Map.GenerateWorld();
    }

    public void Restart()
    {
        Map.CleanUpWorld();
        BuildingTimer = (float)BuildingTime;

        Resources.Wood = 0;
        Resources.Stone = 0;
        CurrentWave = 0;
        XP = 0;

        RenderSettings.ambientLight = DayColour;
        GameObject.Find("Directional Light").GetComponent<Light>().color = DayColour;
        GameObject.Find("StartWave").GetComponent<Button>().enabled = true;

        StopAllCoroutines();
    }

    public void Exit()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
