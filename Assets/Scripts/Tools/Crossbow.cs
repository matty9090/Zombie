using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crossbow : Weapon
{
    private enum ERangeWeaponState { Loading, Cooldown, Idle };
    private ERangeWeaponState RangeWeaponState = ERangeWeaponState.Idle;

    private readonly float LoadTime = 1.6f;
    private readonly float CooldownTime = 0.5f;
    private float LoadTimer = 0.0f;
    private float CooldownTimer = 0.0f;

    public override void HandleState(Character character)
    {
        var animator = character.CurrentWeapon.GetComponent<Animator>();
        var playerAnim = character.GetComponentInChildren<Animator>();

        if (RangeWeaponState == ERangeWeaponState.Loading)
        {
            LoadTimer -= Time.deltaTime;

            if (LoadTimer <= 0.0f && Input.GetMouseButtonDown(0))
            {
                RangeWeaponState = ERangeWeaponState.Cooldown;
                CooldownTimer = CooldownTime;
                LoadTimer = LoadTime;

                animator.SetTrigger("Shoot");
                playerAnim.SetBool("Aiming", false);

                var weapon = character.CurrentWeapon;
                LaunchProjectile(weapon.Projectile, weapon.LaunchPosition, weapon.ProjectileSpeed, weapon.AttackStrength);

                GameObject.Find("Game").GetComponent<Game>().AudioManager.PlayLayered("CrossbowShoot");
            }
        }
        else if (RangeWeaponState == ERangeWeaponState.Idle)
        {
            if (Input.GetMouseButtonDown(0))
            {
                LoadTimer = LoadTime;
                RangeWeaponState = ERangeWeaponState.Loading;
                animator.SetTrigger("Load");
                playerAnim.SetBool("Aiming", true);

                GameObject.Find("Game").GetComponent<Game>().AudioManager.PlayLayered("CrossbowLoad");
            }
        }
        else if (RangeWeaponState == ERangeWeaponState.Cooldown)
        {
            CooldownTimer -= Time.deltaTime;

            if (CooldownTimer <= 0.0f)
            {
                animator.SetTrigger("Ready");
                RangeWeaponState = ERangeWeaponState.Idle;
            }
        }
    }    
}
