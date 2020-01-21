using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : Weapon
{
    private enum ERangeWeaponState { Aiming, Cooldown, Idle };
    private ERangeWeaponState mRangeWeaponState = ERangeWeaponState.Idle;

    private readonly float mAimTime = 0.88f;
    private readonly float mCooldownTime = 0.5f;
    private float mAimTimer = 0.0f;
    private float mCooldownTimer = 0.0f;

    public override void HandleState(Character character)
    {
        var animator = character.CurrentWeapon.GetComponent<Animator>();
        var playerAnim = character.GetComponentInChildren<Animator>();

        if (mRangeWeaponState == ERangeWeaponState.Aiming)
        {
            mAimTimer -= Time.deltaTime;

            // Cancel aiming
            if (!Input.GetMouseButton(0))
            {
                mRangeWeaponState = ERangeWeaponState.Idle;
                animator.SetBool("Aiming", false);
                playerAnim.SetBool("Aiming", false);
            }
            // Shoot if holding down left click and ready
            else if (mAimTimer <= 0.0f)
            {
                mRangeWeaponState = ERangeWeaponState.Cooldown;
                mCooldownTimer = mCooldownTime;
                mAimTimer = mAimTime;
                
                animator.SetTrigger("Shoot");
                playerAnim.SetBool("Aiming", false);

                var weapon = character.CurrentWeapon;
                LaunchProjectile(weapon.Projectile, weapon.LaunchPosition, weapon.ProjectileSpeed, weapon.AttackStrength);

                GameObject.Find("Game").GetComponent<Game>().AudioManager.PlayLayered("SlingshotFire");
            }
        }
        // Start aiming if left click is down
        else if (mRangeWeaponState == ERangeWeaponState.Idle)
        {
            animator.SetBool("Aiming", false);

            if (Input.GetMouseButton(0))
            {
                mAimTimer = mAimTime;
                mRangeWeaponState = ERangeWeaponState.Aiming;
                animator.SetBool("Aiming", true);
                playerAnim.SetBool("Aiming", true);

                GameObject.Find("Game").GetComponent<Game>().AudioManager.PlayLayered("SlingshotLoad");
            }
        }
        // Go to idle state when cooldown has finished
        else if (mRangeWeaponState == ERangeWeaponState.Cooldown)
        {
            mCooldownTimer -= Time.deltaTime;

            if (mCooldownTimer <= 0.0f)
                mRangeWeaponState = ERangeWeaponState.Idle;
        }
    }
}
