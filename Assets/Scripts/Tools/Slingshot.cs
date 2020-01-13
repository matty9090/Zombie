using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : Weapon
{
    private enum ERangeWeaponState { Aiming, Cooldown, Idle };
    private ERangeWeaponState RangeWeaponState = ERangeWeaponState.Idle;

    private readonly float AimTime = 0.88f;
    private readonly float CooldownTime = 0.5f;
    private float AimTimer = 0.0f;
    private float CooldownTimer = 0.0f;

    public override void HandleState(Character character)
    {
        var animator = character.CurrentWeapon.GetComponent<Animator>();
        var playerAnim = character.GetComponentInChildren<Animator>();

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
                AimTimer = AimTime;
                
                animator.SetTrigger("Shoot");
                playerAnim.SetBool("Aiming", false);

                var weapon = character.CurrentWeapon;
                LaunchProjectile(weapon.Projectile, weapon.LaunchPosition, weapon.ProjectileSpeed, weapon.AttackStrength);

                GameObject.Find("Game").GetComponent<Game>().AudioManager.PlayLayered("SlingshotFire");
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

                GameObject.Find("Game").GetComponent<Game>().AudioManager.PlayLayered("SlingshotLoad");
            }
        }
        else if (RangeWeaponState == ERangeWeaponState.Cooldown)
        {
            CooldownTimer -= Time.deltaTime;

            if (CooldownTimer <= 0.0f)
                RangeWeaponState = ERangeWeaponState.Idle;
        }
    }
}
