using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateWave : IState
{
    private Game Game = null;
    private GameObject HoverTile = null;
    private List<Character> Enemies;
    private int NumEnemies = 10;

    private readonly int NumberOfRaycastHits = 1;
    private RaycastHit[] mRaycastHits;

    public StateWave()
    {
        mRaycastHits = new RaycastHit[NumberOfRaycastHits];
        Game = GameObject.Find("Game").GetComponent<Game>();
        HoverTile = Game.HoverTile;
    }

    public void OnEnter()
    {
        Enemies = new List<Character>();

        var env = GameObject.Find("Environment").GetComponent<Environment>();
        var startTiles = env.GetAvailableEdgeTiles();

        MoveTask task = new MoveTask();
        task.Type = EMoveTask.Attack;
        task.AttackTarget = Game.CharacterInst;

        for (int i = 0; i < NumEnemies; ++i)
        {
            var startTile = startTiles[Random.Range(0, startTiles.Count - 1)];
            Vector3 s = startTile.Position;

            var enemy = Game.Instantiate(Game.Character);
            enemy.transform.position = s;
            enemy.CurrentPosition = startTile;
            //enemy.Task = task;
            enemy.GoToAsEnemy(Game.CharacterInst);

            Enemies.Add(enemy);
        }
    }

    public void OnExit()
    {

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

                if (Input.GetMouseButtonDown(0))
                {
                    if (tile.IsAccessible)
                    {
                        List<EnvironmentTile> route = Game.Map.Solve(Game.CharacterInst.CurrentPosition, tile);
                        Game.CharacterInst.GoTo(route);
                    }
                }
            }
        }
    }
}
