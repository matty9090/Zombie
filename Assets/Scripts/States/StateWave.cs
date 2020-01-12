using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateWave : IState
{
    private Game Game = null;
    private GameObject HoverTile = null;
    private GameObject Tool = null;
    private List<Zombie> Enemies;
    private int NumEnemies = 6;
    private float AttackSpeed = 0.32f;
    private float AttackTimer = 0.0f;
    private float ZombieTimer = 6.4f;
    private float MoveSpeed = 16.6f;

    private enum ERangeWeaponState { Aiming, Cooldown, Idle };
    private ERangeWeaponState RangeWeaponState = ERangeWeaponState.Idle;

    private readonly float AimTime = 0.88f;
    private readonly float CooldownTime = 0.5f;
    private float AimTimer = 0.0f;
    private float CooldownTimer = 0.0f;

    public StateWave()
    {
        Game = GameObject.Find("Game").GetComponent<Game>();
        HoverTile = Game.HoverTile;
    }

    public void OnEnter()
    {
        Enemies = new List<Zombie>();
        NumEnemies += 2;

        Game.StartCoroutine(SpawnEnemies());
        Game.StartCoroutine(SmoothCentreCamera());

        Game.CharacterInst.Frozen = true;

        HoverTile.GetComponent<MeshRenderer>().enabled = false;
        Cursor.SetCursor(Game.CursorNormal, Vector2.zero, CursorMode.ForceSoftware);

        foreach (var tile in Object.FindObjectsOfType<EnvironmentTile>())
        {
            var collider = tile.GetComponent<BoxCollider>();

            if (collider != null)
                collider.isTrigger = false;
        }

        Tool = Object.Instantiate(Game.AttackTools[Game.NumWeaponsUnlocked - 1]);
        var scale = Tool.transform.localScale;
        Tool.transform.SetParent(Game.CharacterInst.ToolSocket.transform, false);
        Tool.transform.localPosition = Vector3.zero;
        Tool.transform.localScale = scale;
        Game.CharacterInst.CurrentWeapon = Tool.GetComponent<Weapon>();
    }

    public void OnExit()
    {
        Game.StopAllCoroutines();
        GameObject.Find("WaveUI").GetComponent<Animator>().Play("FadeOut");

        foreach (var tile in Object.FindObjectsOfType<EnvironmentTile>())
        {
            var collider = tile.GetComponent<BoxCollider>();

            if (collider != null)
                collider.isTrigger = true;
        }

        foreach (var enemy in Enemies)
        {
            if (enemy != null)
                Object.Destroy(enemy.gameObject);
        }

        var charLights = Game.CharacterInst.GetComponentsInChildren<Light>();
        charLights[0].enabled = false;
        charLights[0].intensity = 8.0f;
        charLights[1].enabled = false;
        charLights[1].intensity = 0.34f;

        Object.Destroy(Tool);
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
        GameObject.Find("WaveUI").GetComponent<Animator>().Play("Fade");

        var charLights = Game.CharacterInst.GetComponentsInChildren<Light>();
        var maxIntensity = charLights[0].intensity;

        yield return TimerHelper(0.3f, (float t) => {
            foreach (var l in charLights)
            {
                l.enabled = true;
                l.intensity = Mathf.SmoothStep(0.0f, maxIntensity, 1.0f - t);
            }
        });

        Game.CharacterInst.Frozen = false;
        Game.MatchStarted.Invoke();
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
        if (Input.GetKey(KeyCode.W))
            Game.CharacterInst.Move(new Vector3(0.0f, 0.0f, MoveSpeed * Time.deltaTime));

        if (Input.GetKey(KeyCode.A))
            Game.CharacterInst.Move(new Vector3(-MoveSpeed * Time.deltaTime, 0.0f, 0.0f));

        if (Input.GetKey(KeyCode.S))
            Game.CharacterInst.Move(new Vector3(0.0f, 0.0f, -MoveSpeed * Time.deltaTime));

        if (Input.GetKey(KeyCode.D))
            Game.CharacterInst.Move(new Vector3(MoveSpeed * Time.deltaTime, 0.0f, 0.0f));

        if (Game.CharacterInst.CurrentWeapon.WeaponType == Weapon.EWeaponType.Melee)
        {
            AttackTimer -= Time.deltaTime;

            if (Input.GetMouseButtonDown(0) && AttackTimer < 0.0f)
            {
                AttackTimer = AttackSpeed;
                Game.CharacterInst.GetComponentInChildren<Animator>().SetTrigger("Attack");
                Game.CharacterInst.Attack();
            }
        }
        else if (Game.CharacterInst.CurrentWeapon.WeaponType == Weapon.EWeaponType.Range)
        {
            var animator = Game.CharacterInst.CurrentWeapon.GetComponent<Animator>();
            var playerAnim = Game.CharacterInst.GetComponentInChildren<Animator>();

            if (RangeWeaponState == ERangeWeaponState.Aiming)
            {
                AimTimer -= Time.deltaTime;

                if (!Input.GetMouseButton(0))
                {
                    RangeWeaponState = ERangeWeaponState.Idle;
                    animator.SetBool("Aiming", false);
                    playerAnim.SetBool("Aiming", false);
                }
                else if (AimTimer <= 0.0f)
                {
                    RangeWeaponState = ERangeWeaponState.Cooldown;
                    CooldownTimer = CooldownTime;
                    animator.SetTrigger("Shoot");
                    playerAnim.SetBool("Aiming", false);
                    AimTimer = AimTime;

                    var weapon = Game.CharacterInst.CurrentWeapon;
                    LaunchProjectile(weapon.Projectile, weapon.LaunchPosition, weapon.ProjectileSpeed, weapon.AttackStrength);
                }
            }
            else if (RangeWeaponState == ERangeWeaponState.Idle)
            {
                animator.SetBool("Aiming", false);

                if (Input.GetMouseButton(0))
                {
                    AimTimer = AimTime;
                    RangeWeaponState = ERangeWeaponState.Aiming;
                    animator.SetBool("Aiming", true);
                    playerAnim.SetBool("Aiming", true);
                }
            }
            else if (RangeWeaponState == ERangeWeaponState.Cooldown)
            {
                CooldownTimer -= Time.deltaTime;

                if (CooldownTimer <= 0.0f)
                    RangeWeaponState = ERangeWeaponState.Idle;
            }
        }

        // Face direction of cursor
        Ray screenClick = Game.MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit HitTerrainInfo;
        Physics.Raycast(screenClick, out HitTerrainInfo, 1000.0f, LayerMask.GetMask("Default"));
        Vector3 cursorDir = HitTerrainInfo.point - Game.CharacterInst.transform.position;
        cursorDir.y = 0.0f;
        Game.CharacterInst.transform.rotation = Quaternion.LookRotation(cursorDir, Vector3.up);
    }

    private void LaunchProjectile(GameObject obj, Transform launchPoint, float speed, int damage)
    {
        var proj = Object.Instantiate(obj, launchPoint.position, launchPoint.rotation);
        proj.GetComponent<Rigidbody>().velocity = launchPoint.forward * speed;
        proj.GetComponent<Projectile>().Damage = damage;
    }

    public void UIWeaponClicked(UIWeapon element)
    {
        Object.Destroy(Tool);

        Tool = Object.Instantiate(element.WeaponObject);
        var scale = Tool.transform.localScale;
        Tool.transform.SetParent(Game.CharacterInst.ToolSocket.transform, false);
        Tool.transform.localPosition = Vector3.zero;
        Tool.transform.localScale = scale;
        Game.CharacterInst.CurrentWeapon = Tool.GetComponent<Weapon>();
    }
}
