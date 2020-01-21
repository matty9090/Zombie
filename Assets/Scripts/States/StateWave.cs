using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Wave state */
public class StateWave : IState
{
    private Game mGame = null;
    private GameObject mHoverTile = null;
    private GameObject mWeapon = null;
    private List<Zombie> mEnemies;
    private int mNumEnemies = 6;
    private readonly float mAttackSpeed = 0.32f;
    private float mAttackTimer = 0.0f;
    private readonly float mZombieTimer = 6.4f;
    private readonly float mMoveSpeed = 16.6f;
    private readonly float mCharSpotlightIntensity = 8.0f;
    private readonly float mCharFacelightIntensity = 0.34f;

    public StateWave()
    {
        mGame = GameObject.Find("Game").GetComponent<Game>();
        mHoverTile = mGame.HoverTile;
    }

    public void OnEnter()
    {
        mEnemies = new List<Zombie>();
        mNumEnemies += 2;

        mGame.StartCoroutine(SpawnEnemies());
        mGame.StartCoroutine(SmoothCentreCamera());

        // Freeze character until spotlight has been turned on
        mGame.CharacterInst.Frozen = true;

        // Hide hover tile
        mHoverTile.GetComponent<MeshRenderer>().enabled = false;

        Cursor.SetCursor(mGame.CursorNormal, Vector2.zero, CursorMode.ForceSoftware);

        // Set all tiles to have blocking collision
        foreach (var tile in Object.FindObjectsOfType<EnvironmentTile>())
        {
            var collider = tile.GetComponent<BoxCollider>();

            if (collider != null)
                collider.isTrigger = false;
        }

        // Create attack tool
        mWeapon = Object.Instantiate(mGame.AttackTools[mGame.NumWeaponsUnlocked - 1]);
        var scale = mWeapon.transform.localScale;
        mWeapon.transform.SetParent(mGame.CharacterInst.ToolSocket.transform, false);
        mWeapon.transform.localPosition = Vector3.zero;
        mWeapon.transform.localScale = scale;
        mGame.CharacterInst.CurrentWeapon = mWeapon.GetComponent<Weapon>();

        var hud = mGame.Hud.GetComponent<HUD>();
        hud.WaveUI.SetActive(true);
    }

    public void OnExit()
    {
        mGame.StopAllCoroutines();
        GameObject.Find("WaveUI").GetComponent<Animator>().Play("FadeOut");

        // Reset tiles to be triggers
        foreach (var tile in Object.FindObjectsOfType<EnvironmentTile>())
        {
            var collider = tile.GetComponent<BoxCollider>();

            if (collider != null)
                collider.isTrigger = true;
        }

        // Destroy all enemies (player might have died before killing them all)
        foreach (var enemy in mEnemies)
        {
            if (enemy != null)
                Object.Destroy(enemy.gameObject);
        }

        // Reset the lights attached to the player
        var charLights = mGame.CharacterInst.GetComponentsInChildren<Light>();
        charLights[0].enabled = false;
        charLights[0].intensity = mCharSpotlightIntensity;
        charLights[1].enabled = false;
        charLights[1].intensity = mCharFacelightIntensity;

        // Destroy the weapon
        Object.Destroy(mWeapon);

        var hud = mGame.Hud.GetComponent<HUD>();
        hud.BuildUI.SetActive(true);
    }

    /* Helper coroutine to call a function every frame for a duration */
    private IEnumerator TimerHelper(float timer, System.Action<float> func = null)
    {
        while (timer >= 0.0f)
        {
            timer -= Time.deltaTime;
            func?.Invoke(timer);
            yield return null;
        }
    }

    /* Coroutine to move the camera to the player smoothly */
    private IEnumerator SmoothCentreCamera()
    {
        var cam = mGame.MainCamera.GetComponent<FreeRoamCamera>();
        var initial = cam.transform.position;
        var final = mGame.CharacterInst.transform.position + mGame.MainCamera.GetComponent<FollowCamera>().Offset;
        var dir = final - initial;

        yield return TimerHelper(1.2f, (float t) => cam.transform.position = initial + dir * Mathf.SmoothStep(0.0f, 1.0f, 1.0f - t));

        cam.enabled = false;
        mGame.MainCamera.GetComponent<FollowCamera>().enabled = true;
        mGame.MainCamera.GetComponent<FollowCamera>().SetCharacter(mGame.CharacterInst);

        yield return FadeLights();
    }

    /* Coroutine to dim the scene lights and turn on the character spotlight */
    private IEnumerator FadeLights()
    {
        // Fade in scene lights
        var dirLight = GameObject.Find("Directional Light").GetComponent<Light>();
        var col1 = RenderSettings.ambientLight;
        var col2 = dirLight.color;

        yield return TimerHelper(1.8f, (float t) =>
        {
            t = (1.0f - t);

            RenderSettings.ambientLight = new Color(
                Mathf.Lerp(col1.r, mGame.NightColour.r, t),
                Mathf.Lerp(col1.g, mGame.NightColour.g, t),
                Mathf.Lerp(col1.b, mGame.NightColour.b, t)
            );

            dirLight.color = new Color(
                Mathf.Lerp(col2.r, mGame.NightColour.r, t),
                Mathf.Lerp(col2.g, mGame.NightColour.g, t),
                Mathf.Lerp(col2.b, mGame.NightColour.b, t)
            );
        });
        
        yield return new WaitForSeconds(0.6f);

        mGame.AudioManager.Play("Torch");
        
        // Fade in the wave UI
        var hud = mGame.Hud.GetComponent<HUD>();
        hud.BuildUI.SetActive(false);
        hud.WaveUI.GetComponent<Animator>().Play("Fade");

        var charLights = mGame.CharacterInst.GetComponentsInChildren<Light>();
        var maxIntensity = charLights[0].intensity;

        // Turn on the spotlight
        yield return TimerHelper(0.3f, (float t) => {
            foreach (var l in charLights)
            {
                l.enabled = true;
                l.intensity = Mathf.SmoothStep(0.0f, maxIntensity, 1.0f - t);
            }
        });

        // Allow the player to start moving about
        mGame.CharacterInst.Frozen = false;
        mGame.MatchStarted.Invoke();
    }

    /* Spawn the enemies after a duration */
    private IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(mZombieTimer);

        var env = GameObject.Find("Environment").GetComponent<Environment>();

        // Spawn at the edge of the island
        var startTiles = env.GetAvailableEdgeTiles();

        for (int i = 0; i < mNumEnemies; ++i)
        {
            var startTile = startTiles[Random.Range(0, startTiles.Count - 1)];
            Vector3 s = startTile.Position;

            var enemy = Game.Instantiate(mGame.Zombie);
            enemy.transform.position = s;
            enemy.GoTo(mGame.CharacterInst);

            mEnemies.Add(enemy);
        }
    }

    public void Update()
    {
        // Character movement
        if (Input.GetKey(KeyCode.W))
            mGame.CharacterInst.Move(new Vector3(0.0f, 0.0f, mMoveSpeed * Time.deltaTime));

        if (Input.GetKey(KeyCode.A))
            mGame.CharacterInst.Move(new Vector3(-mMoveSpeed * Time.deltaTime, 0.0f, 0.0f));

        if (Input.GetKey(KeyCode.S))
            mGame.CharacterInst.Move(new Vector3(0.0f, 0.0f, -mMoveSpeed * Time.deltaTime));

        if (Input.GetKey(KeyCode.D))
            mGame.CharacterInst.Move(new Vector3(mMoveSpeed * Time.deltaTime, 0.0f, 0.0f));

        // Handle attacking
        if (mGame.CharacterInst.CurrentWeapon.WeaponType == Weapon.EWeaponType.Melee)
        {
            mAttackTimer -= Time.deltaTime;

            if (Input.GetMouseButtonDown(0) && mAttackTimer < 0.0f)
            {
                mAttackTimer = mAttackSpeed;
                mGame.CharacterInst.GetComponentInChildren<Animator>().SetTrigger("Attack");
                mGame.CharacterInst.Attack();
            }
        }
        else if (mGame.CharacterInst.CurrentWeapon.WeaponType == Weapon.EWeaponType.Range)
        {
            mGame.CharacterInst.CurrentWeapon.HandleState(mGame.CharacterInst);
        }

        // Face direction of cursor
        Ray screenClick = mGame.MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit HitTerrainInfo;
        Physics.Raycast(screenClick, out HitTerrainInfo, 1000.0f, LayerMask.GetMask("Default"));
        Vector3 cursorDir = HitTerrainInfo.point - mGame.CharacterInst.transform.position;
        cursorDir.y = 0.0f;
        mGame.CharacterInst.transform.rotation = Quaternion.LookRotation(cursorDir, Vector3.up);
    }

    /* User selected a weapon on the UI */
    public void UIWeaponClicked(UIWeapon element)
    {
        Object.Destroy(mWeapon);

        mWeapon = Object.Instantiate(element.WeaponObject);
        var scale = mWeapon.transform.localScale;
        mWeapon.transform.SetParent(mGame.CharacterInst.ToolSocket.transform, false);
        mWeapon.transform.localPosition = Vector3.zero;
        mWeapon.transform.localScale = scale;
        mGame.CharacterInst.CurrentWeapon = mWeapon.GetComponent<Weapon>();
    }
}
