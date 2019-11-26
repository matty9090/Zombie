using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateWave : IState
{
    private Game Game = null;
    private GameObject HoverTile = null;
    private List<Zombie> Enemies;
    private int NumEnemies = 10;
    private float AttackSpeed = 0.32f;
    private float AttackTimer = 0.0f;

    private readonly int NumberOfRaycastHits = 1;

    public StateWave()
    {
        Game = GameObject.Find("Game").GetComponent<Game>();
        HoverTile = Game.HoverTile;
    }

    public void OnEnter()
    {
        Enemies = new List<Zombie>();

        var env = GameObject.Find("Environment").GetComponent<Environment>();
        var startTiles = env.GetAvailableEdgeTiles();

        Cursor.SetCursor(Game.CursorNormal, Vector2.zero, CursorMode.ForceSoftware);

        for (int i = 0; i < NumEnemies; ++i)
        {
            var startTile = startTiles[Random.Range(0, startTiles.Count - 1)];
            Vector3 s = startTile.Position;

            var enemy = Game.Instantiate(Game.Zombie);
            enemy.transform.position = s;
            enemy.GoTo(Game.CharacterInst);

            Enemies.Add(enemy);
        }

        Cursor.SetCursor(Game.CursorNormal, Vector2.zero, CursorMode.ForceSoftware);
    }

    public void OnExit()
    {

    }

    public void Update()
    {
        HoverTile.GetComponent<MeshRenderer>().enabled = false;

        Ray screenClick = Game.MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit HitTerrainInfo;

        if (Physics.Raycast(screenClick, out HitTerrainInfo, 1000.0f, LayerMask.GetMask("Default")))
        {
            Transform objTransform = HitTerrainInfo.transform;
            EnvironmentTile tile = objTransform.GetComponent<EnvironmentTile>();

            if (tile != null)
            {
                // We hovered over a tile which can be clicked on so set the hover tile position and make it visible
                HoverTile.transform.position = tile.Position;
                HoverTile.GetComponent<MeshRenderer>().enabled = true;

                if (Input.GetMouseButtonDown(0))
                {
                    if (tile.IsAccessible)
                    {
                        var startPos = Game.CharacterInst.NextTile != null ? Game.CharacterInst.NextTile : Game.CharacterInst.CurrentPosition;
                        List<EnvironmentTile> route = Game.Map.Solve(startPos, tile);
                        Game.CharacterInst.GoTo(route);
                    }
                }
            }
        }

        AttackTimer -= Time.deltaTime;

        if(Input.GetMouseButtonDown(1) && AttackTimer < 0.0f)
        {
            AttackTimer = AttackSpeed;
            Game.CharacterInst.GetComponentInChildren<Animator>().SetTrigger("Attack");
            Game.CharacterInst.Attack();
        }
    }
}
