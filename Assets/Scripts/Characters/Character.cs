﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* Move task, currently only harvesting */
public enum EMoveTask { Harvest };

public class MoveTask
{
    public EMoveTask Type { get; set; }
    public Harvestable HarvestTarget { get; set; }
}

/* Character class, controlled by the player */
public class Character : MonoBehaviour
{
    [SerializeField] private float SingleNodeMoveTime = 0.5f;
    [SerializeField] private PlayerAttack AttackCollision = null; // Helper class to track zombies in range
    [SerializeField] public int MaxHealth = 100;
    [SerializeField] private float HarvestSoundTime = 0.5f;
    [SerializeField] private float FootstepTime = 0.28f;
    [SerializeField] private Transform HitPoint = null; // Where to spawn the attack particle effect
    [SerializeField] private GameObject AttackEffect = null;
    [SerializeField] public GameObject ToolSocket = null; // Where to attach the tool to
    [SerializeField] private GameObject UnlockParticleEffect = null;

    public int Health { get; private set; }
    public bool Frozen { get; set; } // Used to freeze the characters movement when switching to wave state
    public MoveTask Task { get; set; }
    public EnvironmentTile CurrentPosition { get; set; }
    public UnityEvent HealthChangedEvent;
    public EnvironmentTile NextTile = null;
    public enum EState { Idle, Moving, Harvesting };
    public HarvestTool CurrentTool = null;
    public Weapon CurrentWeapon = null;
    public Harvestable HarvestTarget = null;

    private EState State = EState.Idle;
    private float HarvestTimeRemaining;
    private float HarvestSoundTimeRemaining = 0.0f;
    private float FootstepTimeRemaining = 0.0f;
    private Animator mAnimator;

    struct LastMove
    {
        public float t;
        public Vector3 start;
        public EnvironmentTile destination;
    }

    private LastMove mLastMove;

    private void Start()
    {
        Health = MaxHealth;
        Task = null;
        mAnimator = GetComponentInChildren<Animator>();
    }

    public void ResetHealth()
    {
        Health = MaxHealth;
        HealthChangedEvent.Invoke();
    }

    private IEnumerator DoMove(Vector3 position, Vector3 destination, bool accessible, float overrideT = 0.0f)
    {
        // Move between the two specified positions over the specified amount of time
        if (position != destination && accessible)
        {
            transform.rotation = Quaternion.LookRotation(destination - position, Vector3.up);
            float t = overrideT;

            while (t < SingleNodeMoveTime && !Frozen)
            {
                t += Time.deltaTime;
                mLastMove.t = t;
                transform.position = Vector3.Lerp(position, destination, t / SingleNodeMoveTime);
                State = EState.Moving;
                yield return null;
            }
        }
    }

    private IEnumerator DoGoTo(List<EnvironmentTile> route, bool finishLastMove)
    {
        mAnimator.SetBool("IsHarvesting", false);

        // If the player switches the tile to move to, finish the last move off first
        if (finishLastMove)
        {
            yield return DoMove(mLastMove.start, mLastMove.destination.Position, true, mLastMove.t);
            CurrentPosition = mLastMove.destination;
        }

        // Move through each tile in the given route
        if (route != null)
        {
            Vector3 position = CurrentPosition.Position;

            for (int count = 0; count < route.Count; ++count)
            {
                Vector3 next = route[count].Position;
                NextTile = route[count];
                mLastMove = new LastMove { start = position, destination = route[count], t = 0.0f };
                yield return DoMove(position, next, route[count].IsAccessible);
                CurrentPosition = route[count];
                position = next;
            }

            // Finished path, check task
            if (Task != null)
            {
                switch (Task.Type)
                {
                    case EMoveTask.Harvest: ExecuteHarvestTask(position); break;
                }

                Task = null;
            }
            else
            {
                State = EState.Idle;
            }
        }
    }

    public void ExecuteHarvestTask(Vector3 position)
    {
        HarvestTarget = Task.HarvestTarget;

        if (HarvestTarget != null && !HarvestTarget.Equals(null))
        {
            State = EState.Harvesting;
            HarvestTimeRemaining = CurrentTool.HarvestTime;

            Vector3 target = HarvestTarget.GetComponent<EnvironmentTile>().Position;
            transform.LookAt(new Vector3(target.x, position.y, target.z)); // Look at the tile we are harvesting
            mAnimator.SetBool("IsHarvesting", true);
        }
        else
            HarvestTarget = null;
    }

    public void GoTo(List<EnvironmentTile> route)
    {
        // Clear all coroutines before starting the new route so 
        // that clicks can interupt any current route animation
        StopAllCoroutines();
        StartCoroutine(DoGoTo(route, State == EState.Moving));
        State = EState.Moving;
    }

    private void Update()
    {
        switch (State)
        {
            case EState.Harvesting:
                StateHarvesting();
                break;

            case EState.Moving:
                StateMoving();
                break;
        }

        // Play footsteps every x seconds when moving
        if (mAnimator.GetFloat("Speed") > 0.5f)
        {
            FootstepTimeRemaining -= Time.deltaTime;

            if (FootstepTimeRemaining < 0.0f)
            {
                FootstepTimeRemaining = FootstepTime;
                GameObject.Find("Game").GetComponent<Game>().AudioManager.Play("Walk");
            }
        }
    }

    private void LateUpdate()
    {
        mAnimator.SetFloat("Speed", 0.0f);
    }

    public void Move(Vector3 v)
    {
        if (Frozen)
            return;

        transform.position += v;
        mAnimator.SetFloat("Speed", 1.0f);
    }

    private void StateHarvesting()
    {
        // Play harvest sounds every x seconds
        HarvestSoundTimeRemaining -= Time.deltaTime;

        if (HarvestSoundTimeRemaining < 0.0f)
        {
            HarvestSoundTimeRemaining = HarvestSoundTime;
            GameObject.Find("Game").GetComponent<Game>().AudioManager.Play(HarvestTarget.GatherSound);
        }

        HarvestTimeRemaining -= Time.deltaTime;

        if (HarvestTimeRemaining <= 0.0f)
        {
            // Harvest tiles in a radius
            var game = GameObject.Find("Game").GetComponent<Game>();
            var res = game.GetComponent<Game>().Resources;
            var environment = GameObject.Find("Environment").GetComponent<Environment>();
            var tiles = environment.GetHarvestableTilesInRadius(HarvestTarget.GetComponent<EnvironmentTile>(), CurrentTool.HarvestRadius);
            int totalWood = 0, totalStone = 0;

            foreach (var t in tiles)
            {
                if (t.Type == EResource.Wood)
                    totalWood += t.Amount;
                else if(t.Type == EResource.Stone)
                    totalStone += t.Amount;

                environment.Harvest(t);
            }

            // Add total resources
            res.Wood += totalWood;
            res.Stone += totalStone;

            game.AudioManager.Play("Rubble");

            // Reset states
            mAnimator.SetBool("IsHarvesting", false);
            State = EState.Idle;
            HarvestTarget = null;
        }
    }

    private void StateMoving()
    {
        mAnimator.SetFloat("Speed", 1.0f);
    }

    public void Attack()
    {
        bool didHit = false;

        // Damage all zombies inside the player's collider
        foreach (var obj in AttackCollision.Colliders)
        {
            if(obj && obj.transform.GetComponent<Zombie>())
            {
                didHit = true;
                obj.transform.GetComponent<Zombie>().Damage(CurrentWeapon.AttackStrength);
            }
        }

        GameObject.Find("Game").GetComponent<Game>().AudioManager.Play("Punch");

        // Player an attack effect particle system is we hit a zombie
        if (didHit)
        {
            var effect = Instantiate(AttackEffect);
            effect.transform.position = HitPoint.position;
            Destroy(effect, effect.GetComponent<ParticleSystem>().main.duration);
        }
    }

    // Damage the player
    public void Damage(int Amount)
    {
        Health -= Amount;
        Health = Mathf.Clamp(Health, 0, MaxHealth);
        HealthChangedEvent.Invoke();

        GameObject.Find("Game").GetComponent<Game>().AudioManager.PlayLayered("ZombieBite");
    }

    public void DisplayUnlockToolParticleEffect()
    {
        var effect = Instantiate(UnlockParticleEffect);
        effect.transform.position = ToolSocket.transform.position;
        Destroy(effect, 5.0f);
    }
}
