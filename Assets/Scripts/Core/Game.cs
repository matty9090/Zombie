using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/* Game manager class */
public class Game : MonoBehaviour
{
    [SerializeField] private HealthBar HealthBar = null;
    [SerializeField] private XPBar XPBar = null;
    [SerializeField] private Text UIWaveText = null;
    [SerializeField] private Text UICountdownText = null;
    [SerializeField] private int BuildingTime = 90;
    [SerializeField] private List<ELevelUp> LevelUps = null;
    [SerializeField] private GameObject PausePanel;
    [SerializeField] private GameObject ConfirmExitBox;

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
    public float BuildingTimeProgress { get { return mBuildingTimer / BuildingTime; } }
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
    private int mCurrentWave = 0;
    private float mBuildingTimer = 0.0f;
    private bool mIsPaused = false;

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
        mBuildingTimer = (float)BuildingTime;
        MainCamera.GetComponent<ICamera>().SetCharacter(CharacterInst);
        AudioManager = GetComponent<AudioManager>();
        InitialCamPosition = MainCamera.transform.position;
        InitialCamRotation = MainCamera.transform.rotation;

        ZombieKilled = new UnityEvent();
        ZombieKilled.AddListener(OnZombieKilled);
        CharacterInst.HealthChangedEvent.AddListener(OnCharacterHealthChanged);

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
        // Update timer for the building state and start the wave if time is up
        // The building state could have been the one controlling the timer and starting the wave
        // but I decided to handle all state changes in the game manager class to keep all
        // state changes together.
        if (mGameState == EGameState.Building)
        {
            mBuildingTimer -= Time.deltaTime;
            UICountdownText.text = FormatTime(mBuildingTimer);

            if (mBuildingTimer < 10.0f)
                UICountdownText.GetComponent<Animator>().Play("FlashText");

            if (mBuildingTimer <= 0.0f)
                StartWave();
        }

        // Update the current game state
        mStates[mGameState].Update();
    }

    /* Helper to format seconds into a time in the format mm:ss */
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

        // Don't invoke an unlock if there isn't anything left to unlock
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

    /* Game over when character died, invoked from a health changed event */
    private void OnCharacterHealthChanged()
    {
        if (CharacterInst.Health <= 0 && mGameState == EGameState.Wave)
        {
            GameOver.transform.Find("Result").GetComponent<Text>().text = "You made it to wave " + mCurrentWave;
            SwitchState(EGameState.GameOver);
        }
    }

    /* When all zombies are killed, the wave has finished */
    private void OnZombieKilled()
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

    /* Intermediate state between wave and building which tells the user they finished the wave */
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
        mBuildingTimer = BuildingTime;
        ++mCurrentWave;
        
        UIWaveText.text = "Wave " + mCurrentWave;
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
        Time.timeScale = 1.0f; // Game might be paused so reset the time scale
        SceneManager.LoadScene("Main");
    }

    public void PauseGame()
    {
        // Disallow unpausing of the game if the user is about to exit
        if (ConfirmExitBox.activeInHierarchy)
            return;

        mIsPaused = !mIsPaused;
        PausePanel.SetActive(mIsPaused);
        Time.timeScale = mIsPaused ? 0.0f : 1.0f;
    }

    public void ShowExitMenu()
    {
        ConfirmExitBox.SetActive(true);
        PausePanel.SetActive(true);
        Time.timeScale = 0.0f;
    }

    public void ConfirmNoExit()
    {
        ConfirmExitBox.SetActive(false);
        PausePanel.SetActive(false);
        Time.timeScale = 1.0f;
    }

    public void ConfirmExit()
    {
        ConfirmExitBox.SetActive(false);
        PausePanel.SetActive(false);
        BackToMainMenu();
    }

    /* Route building UI button click to the building state (TODO: replace with event?) */
    public void UIBuildingClicked(UIBuilding element)
    {
        var buildingState = mStates[EGameState.Building];
        
        if (mGameState == EGameState.Building)
            ((StateBuilding)buildingState).UIBuildingClicked(element);
    }

    /* Route building UI weapon click to the wave state (TODO: replace with event?) */
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

    public void Exit()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
