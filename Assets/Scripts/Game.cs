using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private Texture2D EnemySkin = null;
    [SerializeField] private Canvas Menu = null;
    [SerializeField] private Canvas Hud = null;
    [SerializeField] private Transform CharacterStart = null;
    [SerializeField] private HealthBar HealthBar = null;
    [SerializeField] private int BuildingTime = 90;

    [SerializeField] public Camera MainCamera = null;
    [SerializeField] public Character Character = null;
    [SerializeField] public GameObject HoverTile = null;
    
    public Character CharacterInst { get; private set;  }
    public Resources Resources { get; set; }
    public Environment Map { get; private set; }

    private enum EGameState { Building, Wave, GameOver };
    private RaycastHit[] mRaycastHits;
    private EGameState mGameState = EGameState.Building;
    private float BuildingTimer = 0.0f;

    private Dictionary<EGameState, IState> mStates;
    private readonly int NumberOfRaycastHits = 1;

    void Awake()
    {
        Resources = new Resources();
        mRaycastHits = new RaycastHit[NumberOfRaycastHits];
        Map = GetComponentInChildren<Environment>();
        CharacterInst = Instantiate(Character, transform);
        BuildingTimer = (float)BuildingTime;
        HealthBar.ProvideCharacter(CharacterInst);
        ShowMenu(true);

        mStates = new Dictionary<EGameState, IState>
        {
            [EGameState.Building] = new StateBuilding(),
            [EGameState.Wave]     = new StateWave()
        };

        SwitchState(EGameState.Building);
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
        if (Menu != null && Hud != null)
        {
            Menu.enabled = show;
            Hud.enabled = !show;

            if (show)
            {
                CharacterInst.transform.position = CharacterStart.position;
                CharacterInst.transform.rotation = CharacterStart.rotation;
                Map.CleanUpWorld();
            }
            else
            {
                CharacterInst.transform.position = Map.Start.Position;
                CharacterInst.transform.rotation = Quaternion.identity;
                CharacterInst.CurrentPosition = Map.Start;
            }
        }
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
