using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private Character Character = null;
    [SerializeField] private Canvas Menu = null;
    [SerializeField] private Canvas Hud = null;
    [SerializeField] private Transform CharacterStart = null;
    [SerializeField] private HealthBar HealthBar = null;

    [SerializeField] public Camera MainCamera = null;
    [SerializeField] public GameObject HoverTile = null;
    
    public Character CharacterInst { get; private set;  }
    public Resources Resources { get; set; }
    public Environment Map { get; private set; }

    private enum EGameState { Building, Wave };

    private RaycastHit[] mRaycastHits;
    private EGameState mGameState = EGameState.Building;

    private Dictionary<EGameState, IState> mStates;

    private readonly int NumberOfRaycastHits = 1;

    void Awake()
    {
        Resources = new Resources();
        mRaycastHits = new RaycastHit[NumberOfRaycastHits];
        Map = GetComponentInChildren<Environment>();
        CharacterInst = Instantiate(Character, transform);
        HealthBar.ProvideCharacter(CharacterInst);
        ShowMenu(true);

        mStates = new Dictionary<EGameState, IState>
        {
            [EGameState.Building] = new StateBuilding(),
            [EGameState.Wave]     = new StateWave()
        };
    }

    void Update()
    {
        mStates[mGameState].Update();
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
