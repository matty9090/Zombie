﻿using System.Collections;
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
    private float ZombieTimer = 6.4f;

    public StateWave()
    {
        Game = GameObject.Find("Game").GetComponent<Game>();
        HoverTile = Game.HoverTile;
    }

    public void OnEnter()
    {
        Enemies = new List<Zombie>();

        Game.StartCoroutine(SpawnEnemies());
        Game.StartCoroutine(SmoothCentreCamera());

        Cursor.SetCursor(Game.CursorNormal, Vector2.zero, CursorMode.ForceSoftware);
    }

    public void OnExit()
    {

    }

    private IEnumerator TimerHelper(float timer, System.Action<float> func = null)
    {
        while (timer >= 0.0f)
        {
            timer -= Time.deltaTime;
            func?.Invoke(timer);
            yield return null;
        }
    }

    private IEnumerator SmoothCentreCamera()
    {
        var cam = Game.MainCamera.GetComponent<FreeRoamCamera>();
        var initial = cam.transform.position;
        var final = Game.CharacterInst.transform.position + Game.MainCamera.GetComponent<FollowCamera>().Offset;
        var dir = final - initial;

        yield return TimerHelper(1.2f, (float t) => cam.transform.position = initial + dir * Mathf.SmoothStep(0.0f, 1.0f, 1.0f - t));

        cam.enabled = false;
        Game.MainCamera.GetComponent<FollowCamera>().enabled = true;
        Game.MainCamera.GetComponent<FollowCamera>().SetCharacter(Game.CharacterInst);

        yield return FadeLights();
    }

    private IEnumerator FadeLights()
    {
        var dirLight = GameObject.Find("Directional Light").GetComponent<Light>();
        var col1 = RenderSettings.ambientLight;
        var col2 = dirLight.color;

        yield return TimerHelper(1.8f, (float t) =>
        {
            t = (1.0f - t);

            RenderSettings.ambientLight = new Color(
                Mathf.Lerp(col1.r, Game.NightColour.r, t),
                Mathf.Lerp(col1.g, Game.NightColour.g, t),
                Mathf.Lerp(col1.b, Game.NightColour.b, t)
            );

            dirLight.color = new Color(
                Mathf.Lerp(col2.r, Game.NightColour.r, t),
                Mathf.Lerp(col2.g, Game.NightColour.g, t),
                Mathf.Lerp(col2.b, Game.NightColour.b, t)
            );
        });

        yield return TimerHelper(0.6f);

        Game.AudioManager.Play("Torch");

        var charLight = Game.CharacterInst.GetComponentInChildren<Light>();
        var maxIntensity = charLight.intensity;
        charLight.enabled = true;

        yield return TimerHelper(0.3f, (float t) => charLight.intensity = Mathf.SmoothStep(0.0f, maxIntensity, 1.0f - t));
    }

    private IEnumerator SpawnEnemies()
    {
        yield return TimerHelper(ZombieTimer);

        var env = GameObject.Find("Environment").GetComponent<Environment>();
        var startTiles = env.GetAvailableEdgeTiles();

        for (int i = 0; i < NumEnemies; ++i)
        {
            var startTile = startTiles[Random.Range(0, startTiles.Count - 1)];
            Vector3 s = startTile.Position;

            var enemy = Game.Instantiate(Game.Zombie);
            enemy.transform.position = s;
            enemy.GoTo(Game.CharacterInst);

            Enemies.Add(enemy);
        }
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

        AttackTimer -= Time.deltaTime;

        if(Input.GetMouseButtonDown(1) && AttackTimer < 0.0f)
        {
            AttackTimer = AttackSpeed;
            Game.CharacterInst.GetComponentInChildren<Animator>().SetTrigger("Attack");
            Game.CharacterInst.Attack();
        }

        // Face direction of cursor
        Vector3 cursorDir = HitTerrainInfo.point - Game.CharacterInst.transform.position;
        cursorDir.y = 0.0f;
        Game.CharacterInst.transform.rotation = Quaternion.LookRotation(cursorDir, Vector3.up);
    }
}
