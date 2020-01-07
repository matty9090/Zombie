using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StateBuilding : IState
{
    private enum EControllerState { Idle, PlacingBuilding };
    private EControllerState mControllerState = EControllerState.Idle;

    private Game Game = null;
    private Resources Resources = null;
    private GameObject HoverTile = null;

    private RaycastHit[] mRaycastHits;
    private GameObject mSelectedBuilding = null;
    private UIBuilding mSelectedBuildingUI = null;

    private readonly int NumberOfRaycastHits = 1;
    private Coroutine DayNightCoroutine = null;

    public StateBuilding()
    {
        mRaycastHits = new RaycastHit[NumberOfRaycastHits];
        Game = GameObject.Find("Game").GetComponent<Game>();
        Resources = Game.Resources;
        HoverTile = Game.HoverTile;
    }

    public void OnEnter()
    {
        GameObject.Find("BuildUI").GetComponent<Animator>().SetTrigger("Show");
        GameObject.Find("Directional Light").GetComponent<Light>().color = Game.DayColour;
        RenderSettings.ambientLight = Game.DayColour / 1.2f;

        mControllerState = EControllerState.Idle;
        Game.CharacterInst.ResetHealth();

        Game.MainCamera.GetComponent<FollowCamera>().SetEnabled(false);
        Game.MainCamera.GetComponent<FreeRoamCamera>().SetEnabled(true);

        DayNightCoroutine = Game.StartCoroutine(DayNightCycle());
    }

    public void OnExit()
    {
        if (mSelectedBuilding != null)
            Game.Destroy(mSelectedBuilding);

        HoverTile.GetComponent<MeshRenderer>().material = Game.HoverMaterialG;
        GameObject.Find("BuildUI").GetComponent<Animator>().SetTrigger("Hide");

        Game.StopCoroutine(DayNightCoroutine);
    }

    public void Update()
    {
        HoverTile.GetComponent<MeshRenderer>().enabled = false;

        // Check to see if the player has clicked a tile and if they have, try to find a path to that 
        // tile. If we find a path then the character will move along it to the clicked tile. 
        Ray screenClick = Game.MainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.RaycastNonAlloc(screenClick, mRaycastHits) > 0)
        {
            Transform objTransform = mRaycastHits[0].transform;

            EnvironmentTile tile = objTransform.GetComponent<EnvironmentTile>();
            Harvestable harvestable = objTransform.GetComponent<Harvestable>();

            if (tile == null)
            {
                tile = objTransform.GetComponentInParent<EnvironmentTile>();
                harvestable = objTransform.GetComponentInParent<Harvestable>();
            }

            if (tile != null)
            {
                // We hovered over a tile which can be clicked on so set the hover tile position and make it visible
                HoverTile.transform.position = tile.Position;
                HoverTile.GetComponent<MeshRenderer>().enabled = true;

                if (Input.GetMouseButtonDown(0) && mControllerState == EControllerState.Idle && !EventSystem.current.IsPointerOverGameObject())
                {
                    // Harvest
                    if (harvestable != null && mControllerState == EControllerState.Idle)
                    {
                        List<EnvironmentTile> bestRoute = null;
                        float minDist = float.MaxValue;
                        int manhattan = Game.Map.ManhattanDistance(Game.CharacterInst.CurrentPosition, tile);
                        
                        if (manhattan > 10)
                        {
                            // Harvestable tiles do not have a direct path to them so find the path to the closest walkable tile
                            foreach (EnvironmentTile t in tile.Connections)
                            {
                                float dist = (int)Vector3.Distance(t.Position, Game.CharacterInst.CurrentPosition.Position);
                                var startPos = Game.CharacterInst.NextTile != null ? Game.CharacterInst.NextTile : Game.CharacterInst.CurrentPosition;
                                var route = Game.Map.Solve(startPos, t);

                                if (route != null && dist < minDist)
                                {
                                    bestRoute = route;
                                    minDist = dist;
                                }
                            }
                        }
                        else
                        {
                            MoveTask task = new MoveTask();
                            task.Type = EMoveTask.Harvest;
                            task.HarvestTarget = harvestable;

                            Game.CharacterInst.Task = task;
                            Game.CharacterInst.ExecuteHarvestTask(Game.CharacterInst.CurrentPosition.Position);
                        }

                        if (bestRoute != null && bestRoute.Count > 0)
                        {
                            MoveTask task = new MoveTask();
                            task.Type = EMoveTask.Harvest;
                            task.HarvestTarget = harvestable;

                            Game.CharacterInst.Task = task;
                            Game.CharacterInst.GoTo(bestRoute);
                        }
                        else
                        {
                            Game.AudioManager.PlayError();
                        }
                    }
                    // Walk to a tile
                    else if (tile.IsAccessible)
                    {
                        var startPos = Game.CharacterInst.NextTile != null ? Game.CharacterInst.NextTile : Game.CharacterInst.CurrentPosition;
                        List<EnvironmentTile> route = Game.Map.Solve(startPos, tile);

                        if (route != null && route.Count > 0)
                        {
                            Game.CharacterInst.GoTo(route);
                        }
                        else
                        {
                            Game.AudioManager.PlayError();
                        }
                    }
                    else
                    {
                        Game.AudioManager.PlayError();
                    }
                }
            }
        }

        switch (mControllerState)
        {
            case EControllerState.PlacingBuilding:
                StatePlacingBuilding();
                Cursor.SetCursor(Game.CursorBuild, Vector2.zero, CursorMode.ForceSoftware);
                break;

            case EControllerState.Idle:
                Cursor.SetCursor(Game.CursorNormal, Vector2.zero, CursorMode.ForceSoftware);
                break;
        }
    }

    private void StatePlacingBuilding()
    {
        var HoverTile = Game.HoverTile;

        if (HoverTile == null || mSelectedBuilding == null)
            return;

        var tile = mRaycastHits[0].transform.GetComponent<EnvironmentTile>();

        if (tile != null)
        {
            HoverTile.GetComponent<MeshRenderer>().material = tile.IsAccessible ? Game.HoverMaterialG : Game.HoverMaterialR;
        }

        bool isEnabled = HoverTile.GetComponent<MeshRenderer>().enabled;
        mSelectedBuilding.GetComponent<MeshRenderer>().enabled = isEnabled;
        mSelectedBuilding.transform.position = HoverTile.transform.position;

        if (Input.GetMouseButtonUp(1))
        {
            Game.Destroy(mSelectedBuilding);
            mControllerState = EControllerState.Idle;
            HoverTile.GetComponent<MeshRenderer>().material = Game.HoverMaterialG;
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            mSelectedBuilding.transform.Rotate(new Vector3(0.0f, 90.0f, 0.0f));
            Game.AudioManager.Play("RotateBuilding");
        }
        else if (Input.GetMouseButtonUp(0) && isEnabled && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray screenClick = Game.MainCamera.ScreenPointToRay(Input.mousePosition);
            int num = Physics.RaycastNonAlloc(screenClick, mRaycastHits);

            if (num > 0 && tile != null && tile.IsAccessible)
            {
                Resources.Wood -= mSelectedBuildingUI.Wood;
                Resources.Stone -= mSelectedBuildingUI.Stone;
                mSelectedBuilding.transform.parent = tile.transform;
                Game.AudioManager.Play("PlaceObject");

                tile.IsAccessible = false;
                mSelectedBuilding.GetComponent<Building>().Place();

                if (Input.GetKey(KeyCode.LeftShift) &&
                    Resources.Wood >= mSelectedBuildingUI.Wood &&
                    Resources.Stone >= mSelectedBuildingUI.Stone)
                {
                    mControllerState = EControllerState.PlacingBuilding;
                    mSelectedBuilding = Game.Instantiate(mSelectedBuilding);
                    mSelectedBuilding.transform.position = HoverTile.transform.position;
                }
                else
                {
                    mControllerState = EControllerState.Idle;
                    mSelectedBuilding = null;
                }
            }
        }
    }

    public void UIBuildingClicked(UIBuilding element)
    {
        if (Resources.Wood >= element.Wood && Resources.Stone >= element.Stone)
        {
			if (mSelectedBuilding != null)
				Game.Destroy(mSelectedBuilding);	
  
            mControllerState = EControllerState.PlacingBuilding;
            mSelectedBuilding = Game.Instantiate(element.Object);
            mSelectedBuildingUI = element;
        }
        else
        {
            Game.AudioManager.PlayError();
        }
    }

    private IEnumerator DayNightCycle()
    {
        var dirLight = GameObject.Find("Directional Light").GetComponent<Light>();

        while (Game.BuildingTimeProgress > 0.0f)
        {
            Color col = Game.DayNightGradient.Evaluate(Game.BuildingTimeProgress);
            RenderSettings.ambientLight = col;
            dirLight.color = col;
            yield return null;
        }
    }
}
