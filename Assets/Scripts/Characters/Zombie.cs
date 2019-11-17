using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    [SerializeField] private float DistanceThreshold = 12.0f;
    [SerializeField] private float ZombieMoveSpeed = 10.0f;
    [SerializeField] private int DamageAmount = 1;
    [SerializeField] private float DamageTime = 0.8f;
    [SerializeField] private float HarvestTime = 1.4f;
    [SerializeField] public int MaxHealth = 100;

    public enum EState { Moving, Attacking, Harvesting };
    private EState State = EState.Moving;

    private Character AttackTarget = null;
    private Harvestable HarvestTarget = null;

    public int Health { get; private set; }
    private float DamageTimeRemaining;
    private float HarvestTimeRemaining;

    void Start()
    {
        Health = MaxHealth;
    }

    private IEnumerator DoGoTo(Character player)
    {
        if (player != null)
        {
            while (Vector3.Distance(transform.position, player.transform.position) > DistanceThreshold)
            {
                var delta = player.transform.position - transform.position;
                transform.rotation = Quaternion.LookRotation(delta, Vector3.up);
                transform.position += transform.forward * ZombieMoveSpeed * Time.deltaTime;
                yield return null;
            }

            State = EState.Attacking;
        }
    }

    public void GoTo(Character player)
    {
        State = EState.Moving;
        AttackTarget = player;
        StopAllCoroutines();
        StartCoroutine(DoGoTo(player));
    }

    void Update()
    {
        GetComponentInChildren<Animator>().SetFloat("Speed", State == EState.Moving ? 1.0f : 0.0f);
        GetComponentInChildren<Animator>().SetBool("IsHarvesting", State == EState.Harvesting || State == EState.Attacking);

        if (State == EState.Attacking)
        {
            var dist = Vector3.Distance(AttackTarget.transform.position, transform.position);

            if (dist > DistanceThreshold)
            {
                GoTo(AttackTarget);
            }
            else
            {
                DamageTimeRemaining -= Time.deltaTime;

                if (DamageTimeRemaining <= 0)
                {
                    DamageTimeRemaining = DamageTime;
                    AttackTarget.Damage(DamageAmount);
                }
            }
        }
        else if (State == EState.Harvesting)
        {
            HarvestTimeRemaining -= Time.deltaTime;

            if (HarvestTimeRemaining < 0)
            {
                if (HarvestTarget != null)
                {
                    var env = GameObject.Find("Environment").GetComponent<Environment>();
                    env.Harvest(HarvestTarget.GetComponent<Harvestable>());
                }

                GoTo(AttackTarget);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Harvestable>())
        {
            StopAllCoroutines();
            HarvestTarget = other.GetComponent<Harvestable>();
            HarvestTimeRemaining = HarvestTime;
            State = EState.Harvesting;
        }
    }
}
