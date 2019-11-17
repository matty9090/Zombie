using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private HealthBar HealthBar = null;
    [SerializeField] private int BuildingTime = 90;

    [SerializeField] public Canvas Menu = null;
    [SerializeField] public Canvas Hud = null;
    [SerializeField] public Camera MainCamera = null;
    [SerializeField] public Character Character = null;
    [SerializeField] public Zombie Zombie = null;
    [SerializeField] public GameObject HoverTile = null;
    [SerializeField] public Transform CharacterStart = null;
    [SerializeField] public Material HoverMaterialG = null;
    [SerializeField] public Material HoverMaterialR = null;

    public Character CharacterInst { get; private set;  }
    public Resources Resources { get; set; }
    public Environment Map { get; private set; }

    private enum EGameState { Menu, Building, Wave, GameOver };
    private EGameState mGameState = EGameState.Building;
    private float BuildingTimer = 0.0f;

    private Dictionary<EGameState, IState> mStates;

    void Awake()
    {
        Resources = new Resources();
        Map = GetComponentInChildren<Environment>();
        CharacterInst = Instantiate(Character, transform);
        BuildingTimer = (float)BuildingTime;
        HealthBar.ProvideCharacter(CharacterInst);

        mStates = new Dictionary<EGameState, IState>
        {
            [EGameState.Menu]     = new StateMenu(),
            [EGameState.Building] = new StateBuilding(),
            [EGameState.Wave]     = new StateWave()
        };

        SwitchState(EGameState.Menu);
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

        // Transition conditions
        if(mGameState == EGameState.Wave)
        {
            if(CharacterInst.Health <= 0)
            {
                // Game over
            }
        }
    }

    public void ShowMenu(bool show)
    {
        SwitchState(show ? EGameState.Menu : EGameState.Building);
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
