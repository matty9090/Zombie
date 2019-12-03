using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum EMoveTask { Harvest };

public class MoveTask
{
    public EMoveTask Type { get; set; }
    public Character AttackTarget { get; set; }
    public Harvestable HarvestTarget { get; set; }
}

public class Character : MonoBehaviour
{
    [SerializeField] private int DamageAmount = 40;
    [SerializeField] private float SingleNodeMoveTime = 0.5f;
    [SerializeField] private PlayerAttack AttackCollision = null;
    [SerializeField] public int MaxHealth = 100;
    [SerializeField] public float HarvestTime = 1.8f;
    [SerializeField] public float HarvestSoundTime = 0.5f;
    [SerializeField] public float FootstepTime = 0.28f;

    public int Health { get; private set; }
    public MoveTask Task { get; set; }
    public EnvironmentTile CurrentPosition { get; set; }
    public UnityEvent HealthChangedEvent;
    public EnvironmentTile NextTile = null;
    public enum EState { Idle, Moving, Harvesting, Attacking };

    private EState State = EState.Idle;
    private Harvestable HarvestTarget = null;
    private float HarvestTimeRemaining;
    private float HarvestSoundTimeRemaining = 0.0f;
    private float FootstepTimeRemaining = 0.0f;

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
    }

    private IEnumerator DoMove(Vector3 position, Vector3 destination, bool accessible, float overrideT = 0.0f)
    {
        // Move between the two specified positions over the specified amount of time
        if (position != destination && accessible)
        {
            transform.rotation = Quaternion.LookRotation(destination - position, Vector3.up);
            float t = overrideT;

            while (t < SingleNodeMoveTime)
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
                    case EMoveTask.Harvest:
                        State = EState.Harvesting;
                        HarvestTarget = Task.HarvestTarget;
                        HarvestTimeRemaining = HarvestTime;

                        Vector3 target = HarvestTarget.GetComponent<EnvironmentTile>().Position;
                        transform.LookAt(new Vector3(target.x, position.y, target.z));

                        break;
                }

                Task = null;
            }
            else
            {
                State = EState.Idle;
            }
        }
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
        GetComponentInChildren<Animator>().SetFloat("Speed", State == EState.Moving ? 1.0f : 0.0f);
        GetComponentInChildren<Animator>().SetBool("IsHarvesting", State == EState.Harvesting || State == EState.Attacking);

        switch (State)
        {
            case EState.Harvesting:
                StateHarvesting();
                break;

            case EState.Attacking:
                StateAttacking();
                break;

            case EState.Moving:
                StateMoving();
                break;
        }
    }

    private void StateHarvesting()
    {
        HarvestSoundTimeRemaining -= Time.deltaTime;

        if (HarvestSoundTimeRemaining < 0.0f)
        {
            HarvestSoundTimeRemaining = HarvestSoundTime;
            GameObject.Find("Game").GetComponent<Game>().AudioManager.Play(HarvestTarget.GatherSound);
        }

        HarvestTimeRemaining -= Time.deltaTime;

        if (HarvestTimeRemaining <= 0.0f)
        {
            var res = GameObject.Find("Game").GetComponent<Game>().Resources;
            var environment = GameObject.Find("Environment").GetComponent<Environment>();

            if (HarvestTarget.Type == EResource.Wood)
                res.Wood += HarvestTarget.Amount;
            else if (HarvestTarget.Type == EResource.Stone)
                res.Stone += HarvestTarget.Amount;

            environment.Harvest(HarvestTarget);

            State = EState.Idle;
        }
    }

    private void StateAttacking()
    {
        
    }

    private void StateMoving()
    {
        FootstepTimeRemaining -= Time.deltaTime;

        if (FootstepTimeRemaining < 0.0f)
        {
            FootstepTimeRemaining = FootstepTime;
            GameObject.Find("Game").GetComponent<Game>().AudioManager.Play("Walk");
        }
    }

    public void Attack()
    {
        foreach (var obj in AttackCollision.Colliders)
        {
            if(obj && obj.transform.GetComponent<Zombie>())
            {
                obj.transform.GetComponent<Zombie>().Damage(DamageAmount);
            }
        }

        GameObject.Find("Game").GetComponent<Game>().AudioManager.Play("Punch");
    }

    public void Damage(int Amount)
    {
        Health -= Amount;
        Health = Mathf.Clamp(Health, 0, MaxHealth);
        HealthChangedEvent.Invoke();

        GameObject.Find("Game").GetComponent<Game>().AudioManager.PlayLayered("ZombieBite");
    }
}
