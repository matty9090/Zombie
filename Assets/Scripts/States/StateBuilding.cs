using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/* Building state */
public class StateBuilding : IState
{
    private enum EControllerState { Idle, PlacingBuilding };
    private EControllerState mControllerState = EControllerState.Idle;

    private Game mGame = null;
    private Resources mResources = null;
    private GameObject mHoverTile = null;
    private GameObject mTool = null;

    private RaycastHit[] mRaycastHits;
    private GameObject mSelectedBuilding = null;
    private UIBuilding mSelectedBuildingUI = null;

    private readonly int mNumberOfRaycastHits = 1;
    private Coroutine mDayNightCoroutine = null;

    public StateBuilding()
    {
        mRaycastHits = new RaycastHit[mNumberOfRaycastHits];
        mGame = GameObject.Find("Game").GetComponent<Game>();
        mResources = mGame.Resources;
        mHoverTile = mGame.HoverTile;

        mGame.ToolUnlocked.AddListener(UnlockTool);
    }

    public void OnEnter()
    {
        // Hide wave UI and show build UI
        var hud = mGame.Hud.GetComponent<HUD>();
        hud.WaveUI.SetActive(false);
        hud.BuildUI.GetComponent<Animator>().SetTrigger("Show");

        // Set light to day colour
        GameObject.Find("Directional Light").GetComponent<Light>().color = mGame.DayColour;
        RenderSettings.ambientLight = mGame.DayColour / 1.2f;

        mControllerState = EControllerState.Idle;
        mGame.CharacterInst.ResetHealth();

        // Use free roam camera
        mGame.MainCamera.GetComponent<FollowCamera>().SetEnabled(false);
        mGame.MainCamera.GetComponent<FreeRoamCamera>().SetEnabled(true);

        mDayNightCoroutine = mGame.StartCoroutine(DayNightCycle());

        InitTool();
    }

    public void OnExit()
    {
        if (mSelectedBuilding != null)
            Object.Destroy(mSelectedBuilding);

        mHoverTile.GetComponent<MeshRenderer>().material = mGame.HoverMaterialG;
        
        var hud = mGame.Hud.GetComponent<HUD>();
        hud.BuildUI.GetComponent<Animator>().SetTrigger("Hide");

        mGame.StopCoroutine(mDayNightCoroutine);
        Object.Destroy(mTool);
    }

    public void Update()
    {
        mHoverTile.GetComponent<MeshRenderer>().enabled = false;

        // Check to see if the player has clicked a tile and if they have, try to find a path to that 
        // tile. If we find a path then the character will move along it to the clicked tile. 
        Ray screenClick = mGame.MainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.RaycastNonAlloc(screenClick, mRaycastHits) > 0)
        {
            Transform objTransform = mRaycastHits[0].transform;

            EnvironmentTile tile = objTransform.GetComponent<EnvironmentTile>();
            Harvestable harvestable = objTransform.GetComponent<Harvestable>();

            // Some prefabs are structured with the environment tile / harvestable component at the top level
            if (tile == null)
            {
                tile = objTransform.GetComponentInParent<EnvironmentTile>();
                harvestable = objTransform.GetComponentInParent<Harvestable>();
            }

            if (tile != null)
            {
                // We hovered over a tile which can be clicked on so set the hover tile position and make it visible
                mHoverTile.transform.position = tile.Position;
                mHoverTile.GetComponent<MeshRenderer>().enabled = true;

                if (Input.GetMouseButtonDown(0) && mControllerState == EControllerState.Idle && !EventSystem.current.IsPointerOverGameObject())
                {
                    // Send player to harvest a tile
                    if (harvestable != null && mControllerState == EControllerState.Idle)
                    {
                        if (harvestable == mGame.CharacterInst.HarvestTarget)
                            return;

                        int manhattan = mGame.Map.ManhattanDistance(mGame.CharacterInst.CurrentPosition, tile);
                        
                        // A tile is 10 units across, anything less then the player is right next to the tile
                        if (manhattan > 10)
                        {
                            List<EnvironmentTile> bestRoute = null;
                            float minDist = float.MaxValue;

                            // Harvestable tiles do not have a direct path to them so find the path to the closest walkable tile
                            foreach (EnvironmentTile t in tile.Connections)
                            {
                                float dist = (int)Vector3.Distance(t.Position, mGame.CharacterInst.CurrentPosition.Position);
                                var startPos = mGame.CharacterInst.NextTile != null ? mGame.CharacterInst.NextTile : mGame.CharacterInst.CurrentPosition;
                                var route = mGame.Map.Solve(startPos, t);

                                if (route != null && dist < minDist)
                                {
                                    bestRoute = route;
                                    minDist = dist;
                                }
                            }

                            if (bestRoute != null && bestRoute.Count > 0)
                            {
                                MoveTask task = new MoveTask
                                {
                                    Type = EMoveTask.Harvest,
                                    HarvestTarget = harvestable
                                };

                                mGame.CharacterInst.Task = task;
                                mGame.CharacterInst.GoTo(bestRoute);
                            }
                            else
                            {
                                mGame.AudioManager.PlayError();
                            }
                        }
                        else
                        {
                            // Execute harvest task instantly as the player is at the resource tile already
                            MoveTask task = new MoveTask
                            {
                                Type = EMoveTask.Harvest,
                                HarvestTarget = harvestable
                            };

                            mGame.CharacterInst.Task = task;
                            mGame.CharacterInst.ExecuteHarvestTask(mGame.CharacterInst.CurrentPosition.Position);
                        }
                    }
                    // Walk to a tile
                    else if (tile.IsAccessible)
                    {
                        var startPos = mGame.CharacterInst.NextTile != null ? mGame.CharacterInst.NextTile : mGame.CharacterInst.CurrentPosition;
                        List<EnvironmentTile> route = mGame.Map.Solve(startPos, tile);

                        if (route != null && route.Count > 0)
                            mGame.CharacterInst.GoTo(route);
                        else
                            mGame.AudioManager.PlayError();
                    }
                    else
                    {
                        mGame.AudioManager.PlayError();
                    }
                }
            }
        }

        switch (mControllerState)
        {
            case EControllerState.PlacingBuilding:
                StatePlacingBuilding();
                Cursor.SetCursor(mGame.CursorBuild, Vector2.zero, CursorMode.ForceSoftware);
                break;

            case EControllerState.Idle:
                Cursor.SetCursor(mGame.CursorNormal, Vector2.zero, CursorMode.ForceSoftware);
                break;
        }
    }

    private void StatePlacingBuilding()
    {
        var HoverTile = mGame.HoverTile;

        if (HoverTile == null || mSelectedBuilding == null)
            return;

        var tile = mRaycastHits[0].transform.GetComponent<EnvironmentTile>();

        // If a raycast hit a tile, check whether it's accessible so we can place it on or not
        if (tile != null)
        {
            HoverTile.GetComponent<MeshRenderer>().material = tile.IsAccessible ? mGame.HoverMaterialG : mGame.HoverMaterialR;
        }

        bool isEnabled = HoverTile.GetComponent<MeshRenderer>().enabled;
        
        // Don't render building if hover tile isn't enabled (hover tile is off the edge of the map)
        foreach (var renderer in mSelectedBuilding.GetComponentsInChildren<MeshRenderer>())
            renderer.enabled = isEnabled;

        // Set position of the selected building to where the mouse cursor is
        mSelectedBuilding.transform.position = HoverTile.transform.position;

        if (Input.GetMouseButtonUp(1))
        {
            // Cancel building placement
            Object.Destroy(mSelectedBuilding);
            mControllerState = EControllerState.Idle;
            HoverTile.GetComponent<MeshRenderer>().material = mGame.HoverMaterialG;
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            // Rotate building
            mSelectedBuilding.transform.Rotate(new Vector3(0.0f, 90.0f, 0.0f));
            mGame.AudioManager.Play("RotateBuilding");
        }
        else if (Input.GetMouseButtonUp(0) && isEnabled && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray screenClick = mGame.MainCamera.ScreenPointToRay(Input.mousePosition);
            int num = Physics.RaycastNonAlloc(screenClick, mRaycastHits);

            // Place building if not over character and we're over an accessible tile
            if (num > 0 && tile != null && tile.IsAccessible && mGame.CharacterInst.CurrentPosition != tile)
            {
                // Subtract resources
                mResources.Wood -= mSelectedBuildingUI.Wood;
                mResources.Stone -= mSelectedBuildingUI.Stone;

                // Parent building to the tile we're placing on
                mSelectedBuilding.transform.parent = tile.transform;
                
                // Gain XP when place buildings
                mGame.XP += mGame.XPGainOnBuildingPlace;
                
                // Play building place sound
                mGame.AudioManager.Play("PlaceObject");

                // Tile isn't accessible anymore
                tile.IsAccessible = false;

                mSelectedBuilding.GetComponent<Building>().Place();

                // User might holding down shift to quickly place a building of the same type again
                if (Input.GetKey(KeyCode.LeftShift) &&
                    mResources.Wood >= mSelectedBuildingUI.Wood &&
                    mResources.Stone >= mSelectedBuildingUI.Stone)
                {
                    mControllerState = EControllerState.PlacingBuilding;
                    mSelectedBuilding = Object.Instantiate(mSelectedBuilding); // Clone previous building
                    mSelectedBuilding.transform.position = HoverTile.transform.position;
                }
                else
                {
                    mControllerState = EControllerState.Idle;
                    mSelectedBuilding = null;
                }
            }
            else
                mGame.AudioManager.PlayError();
        }
    }

    /* User wants to create a new building */
    public void UIBuildingClicked(UIBuilding element)
    {
        if (mResources.Wood >= element.Wood && mResources.Stone >= element.Stone)
        {
			if (mSelectedBuilding != null)
                Object.Destroy(mSelectedBuilding);	
  
            mControllerState = EControllerState.PlacingBuilding;
            mSelectedBuilding = Object.Instantiate(element.Object);
            mSelectedBuilding.transform.position = mHoverTile.transform.position;
            mSelectedBuildingUI = element; // Store this so we have access to the resource amounts
        }
        else
        {
            // Not enough resources
            mGame.AudioManager.PlayError();
        }
    }

    /* Coroutine for day/night cycle, using the gradient specified by a designer */
    private IEnumerator DayNightCycle()
    {
        var dirLight = GameObject.Find("Directional Light").GetComponent<Light>();

        while (mGame.BuildingTimeProgress > 0.0f)
        {
            Color col = mGame.DayNightGradient.Evaluate(mGame.BuildingTimeProgress);
            RenderSettings.ambientLight = col;
            dirLight.color = col;
            yield return null;
        }
    }

    /* Give the character the best tool unlocked and parent it to the socket on the hand */
    private void InitTool()
    {
        mTool = Object.Instantiate(mGame.HarvestTools[mGame.CurrentHarvestTool]);
        var scale = mTool.transform.localScale;
        mTool.transform.SetParent(mGame.CharacterInst.ToolSocket.transform, false);
        mTool.transform.localPosition = Vector3.zero;
        mTool.transform.localScale = scale;
        mGame.CharacterInst.CurrentTool = mTool.GetComponent<HarvestTool>();
    }

    /* Set the current tool to the best tool and reinit the tool instance */
    private void UnlockTool()
    {
        mGame.CurrentHarvestTool = mGame.NumToolsUnlocked - 1;
        Object.Destroy(mTool);
        InitTool();

        mGame.CharacterInst.DisplayUnlockToolParticleEffect();
    }
}
