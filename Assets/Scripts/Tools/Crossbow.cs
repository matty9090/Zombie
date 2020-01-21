using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crossbow : Weapon
{
    private enum ERangeWeaponState { Loading, Cooldown, Idle };
    private ERangeWeaponState mRangeWeaponState = ERangeWeaponState.Idle;

    private readonly float mLoadTime = 1.6f;
    private readonly float mCooldownTime = 0.5f;
    private float mLoadTimer = 0.0f;
    private float mCooldownTimer = 0.0f;

    public override void HandleState(Character character)
    {
        var animator = character.CurrentWeapon.GetComponent<Animator>();
        var playerAnim = character.GetComponentInChildren<Animator>();

        if (mRangeWeaponState == ERangeWeaponState.Loading)
        {
            mLoadTimer -= Time.deltaTime;

            // If loaded and left click pressed, then shoot and go into the cooldown state
            if (mLoadTimer <= 0.0f && Input.GetMouseButtonDown(0))
            {
                mRangeWeaponState = ERangeWeaponState.Cooldown;
                mCooldownTimer = mCooldownTime;
                mLoadTimer = mLoadTime;

                animator.SetTrigger("Shoot");
                playerAnim.SetBool("Aiming", false);

                var weapon = character.CurrentWeapon;
                LaunchProjectile(weapon.Projectile, weapon.LaunchPosition, weapon.ProjectileSpeed, weapon.AttackStrength);

                GameObject.Find("Game").GetComponent<Game>().AudioManager.PlayLayered("CrossbowShoot");
            }
        }
        // If left click if pressed then load the crossbow
        else if (mRangeWeaponState == ERangeWeaponState.Idle)
        {
            if (Input.GetMouseButtonDown(0))
            {
                mLoadTimer = mLoadTime;
                mRangeWeaponState = ERangeWeaponState.Loading;
                animator.SetTrigger("Load");
                playerAnim.SetBool("Aiming", true);

                GameObject.Find("Game").GetComponent<Game>().AudioManager.PlayLayered("CrossbowLoad");
            }
        }
        // Go to idle state once cooldown has finished
        else if (mRangeWeaponState == ERangeWeaponState.Cooldown)
        {
            mCooldownTimer -= Time.deltaTime;

            if (mCooldownTimer <= 0.0f)
            {
                animator.SetTrigger("Ready");
                mRangeWeaponState = ERangeWeaponState.Idle;
            }
        }
    }    
}
