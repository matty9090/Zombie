using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    [SerializeField] private HealthBar HealthBar = null;
    [SerializeField] private int BuildingTime = 90;

    public Canvas Menu = null;
    public Canvas Hud = null;
    public Camera MainCamera = null;
    public Character Character = null;
    public Zombie Zombie = null;
    public GameObject HoverTile = null;
    public GameObject GameOver = null;
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

    private enum EGameState { Menu, Building, Wave, GameOver };
    private EGameState mGameState = EGameState.Menu;
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

        CharacterInst.HealthChangedEvent.AddListener(CharacterHealthChanged);

        Cursor.SetCursor(CursorNormal, Vector2.zero, CursorMode.ForceSoftware);

        mStates = new Dictionary<EGameState, IState>
        {
            [EGameState.Menu]     = new StateMenu(),
            [EGameState.Building] = new StateBuilding(),
            [EGameState.Wave]     = new StateWave(),
            [EGameState.GameOver] = new StateGameOver()
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

            if (BuildingTimer <= 0.0f)
            {
                SwitchState(EGameState.Wave);
            }
        }

        mStates[mGameState].Update();
    }

    private void CharacterHealthChanged()
    {
        if (CharacterInst.Health <= 0 && mGameState == EGameState.Wave)
        {
            SwitchState(EGameState.GameOver);
        }
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
