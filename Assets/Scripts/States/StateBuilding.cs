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

    public StateBuilding()
    {
        mRaycastHits = new RaycastHit[NumberOfRaycastHits];
        Game = GameObject.Find("Game").GetComponent<Game>();
        Resources = Game.Resources;
        HoverTile = Game.HoverTile;
    }

    public void OnEnter()
    {

    }

    public void OnExit()
    {
        if (mSelectedBuilding != null)
            Game.Destroy(mSelectedBuilding);

        HoverTile.GetComponent<MeshRenderer>().material = Game.HoverMaterialG;
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

            if (tile != null)
            {
                // We hovered over a tile which can be clicked on so set the hover tile position and make it visible
                HoverTile.transform.position = tile.Position;
                HoverTile.GetComponent<MeshRenderer>().enabled = true;

                if (Input.GetMouseButtonDown(0) && mControllerState == EControllerState.Idle)
                {
                    if (harvestable != null)
                    {
                        List<EnvironmentTile> bestRoute = null;
                        float minDist = float.MaxValue;

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

                        // Found path
                        if (bestRoute != null)
                        {
                            MoveTask task = new MoveTask();
                            task.Type = EMoveTask.Harvest;
                            task.HarvestTarget = harvestable;

                            Game.CharacterInst.Task = task;
                            Game.CharacterInst.GoTo(bestRoute);
                        }
                    }
                    else if (tile.IsAccessible)
                    {
                        var startPos = Game.CharacterInst.NextTile != null ? Game.CharacterInst.NextTile : Game.CharacterInst.CurrentPosition;
                        List<EnvironmentTile> route = Game.Map.Solve(startPos, tile);
                        Game.CharacterInst.GoTo(route);
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
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            mSelectedBuilding.transform.Rotate(new Vector3(0.0f, 90.0f, 0.0f));
        }
        else if (Input.GetMouseButtonUp(0) && isEnabled && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray screenClick = Game.MainCamera.ScreenPointToRay(Input.mousePosition);
            int num = Physics.RaycastNonAlloc(screenClick, mRaycastHits);

            if (num > 0 && tile != null && tile.IsAccessible)
            {
                Resources.Wood -= mSelectedBuildingUI.Wood;
                Resources.Stone -= mSelectedBuildingUI.Stone;
                mSelectedBuilding = null;
                tile.IsAccessible = false;
                mControllerState = EControllerState.Idle;
            }
        }
    }

    public void UIBuildingClicked(UIBuilding element)
    {
        if (Resources.Wood >= element.Wood && Resources.Stone >= element.Stone)
        {
            mControllerState = EControllerState.PlacingBuilding;
            mSelectedBuilding = Game.Instantiate(element.Object);
            mSelectedBuildingUI = element;
        }
    }
}
