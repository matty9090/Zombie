using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private Camera MainCamera = null;
    [SerializeField] private Character Character = null;
    [SerializeField] private Canvas Menu = null;
    [SerializeField] private Canvas Hud = null;
    [SerializeField] private Transform CharacterStart = null;
    [SerializeField] private GameObject HoverTile = null;
    [SerializeField] private HealthBar HealthBar = null;
    
    public Resources Resources { get; set; }

    private RaycastHit[] mRaycastHits;
    private Character mCharacter;
    private Environment mMap;

    private readonly int NumberOfRaycastHits = 1;

    void Awake()
    {
        Resources = new Resources();
        mRaycastHits = new RaycastHit[NumberOfRaycastHits];
        mMap = GetComponentInChildren<Environment>();
        mCharacter = Instantiate(Character, transform);
        HealthBar.ProvideCharacter(mCharacter);
        ShowMenu(true);
    }

    void Update()
    {
        HoverTile.GetComponent<MeshRenderer>().enabled = false;

        // Check to see if the player has clicked a tile and if they have, try to find a path to that 
        // tile. If we find a path then the character will move along it to the clicked tile. 
        Ray screenClick = MainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.RaycastNonAlloc(screenClick, mRaycastHits) > 0)
        {
            Transform objTransform = mRaycastHits[0].transform;

            EnvironmentTile tile = objTransform.GetComponent<EnvironmentTile>();
            Harvestable harvestable = objTransform.GetComponent<Harvestable>();

            if (tile != null)
            {
                // We hovered over a tile which can be clicked on so set the hover tile position and make it visible
                HoverTile.transform.position = tile.Position;
                HoverTile.GetComponent<MeshRenderer>().enabled = true;

                if (Input.GetMouseButtonDown(0))
                {
                    if (harvestable != null)
                    {
                        List<EnvironmentTile> bestRoute = null;
                        float minDist = float.MaxValue;

                        // Harvestable tiles do not have a direct path to them so find the path to the closest walkable tile
                        foreach (EnvironmentTile t in tile.Connections)
                        {
                            int dist = (int)Vector3.Distance(t.Position, mCharacter.CurrentPosition.Position);
                            var route = mMap.Solve(mCharacter.CurrentPosition, t);

                            if(route != null && dist < minDist)
                            {
                                bestRoute = route;
                                minDist = dist;
                            }
                        }

                        // Found path
                        if(bestRoute != null)
                        {
                            MoveTask task = new MoveTask();
                            task.Type = EMoveTask.Harvest;
                            task.HarvestTarget = harvestable;

                            mCharacter.Task = task;
                            mCharacter.GoTo(bestRoute);
                        }
                    }
                    else
                    {
                        List<EnvironmentTile> route = mMap.Solve(mCharacter.CurrentPosition, tile);
                        mCharacter.GoTo(route);
                    }
                }
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
                mCharacter.transform.position = CharacterStart.position;
                mCharacter.transform.rotation = CharacterStart.rotation;
                mMap.CleanUpWorld();
            }
            else
            {
                mCharacter.transform.position = mMap.Start.Position;
                mCharacter.transform.rotation = Quaternion.identity;
                mCharacter.CurrentPosition = mMap.Start;
            }
        }
    }

    public void Generate()
    {
        mMap.GenerateWorld();
    }

    public void Exit()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
