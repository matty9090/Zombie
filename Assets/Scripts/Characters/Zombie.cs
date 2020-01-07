using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    [SerializeField] private float DistanceThreshold = 12.0f;
    [SerializeField] private float ZombieMoveSpeed = 10.0f;
    [SerializeField] private int DamageAmount = 1;
    [SerializeField] private float DamageTime = 0.8f;
    [SerializeField] private float HarvestTime = 0.4f;
    [SerializeField] private RangeInt SoundTime = new RangeInt(3, 18);
    [SerializeField] public int MaxHealth = 100;

    public enum EState { Moving, Attacking, Harvesting };
    private EState State = EState.Moving;

    private Character AttackTarget = null;
    private IDestroyable HarvestTarget = null;

    public int Health { get; private set; }
    private float DamageTimeRemaining;

    private Coroutine GoToCoRoutine;

    void Start()
    {
        Health = MaxHealth;
        StartCoroutine(RandomPlaySound());
    }

    private IEnumerator DoGoTo(Character player)
    {
        if (player != null)
        {
            while (player != null && Vector3.Distance(transform.position, player.transform.position) > DistanceThreshold)
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
        
        if(GoToCoRoutine != null)
            StopCoroutine(GoToCoRoutine);

        GoToCoRoutine = StartCoroutine(DoGoTo(player));
    }

    private IEnumerator EffectDamage()
    {
        var renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        float red = 0.0f;

        while (red < 1.0f)
        {
            foreach (var renderer in renderers)
            {
                renderer.material.color = new Color(red, 0.0f, 0.0f, 1.0f);
            }

            red += Time.deltaTime * 2.0f;

            yield return null;
        }

        foreach (var renderer in renderers)
        {
            renderer.material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
    }

    private IEnumerator RandomPlaySound()
    {
        while (true)
        {
            float timer = Random.Range(SoundTime.start, SoundTime.end);

            while (timer > 0.0f)
            {
                timer -= Time.deltaTime;
                yield return null;
            }

            int num = Random.Range(1, 3);
            GameObject.Find("Game").GetComponent<Game>().AudioManager.PlayLayered("ZombieSound" + num);
        }
    }

    void Update()
    {
        GetComponentInChildren<Animator>().SetFloat("Speed", State == EState.Moving ? 1.0f : 0.0f);

        if (State == EState.Attacking)
        {
            if(AttackTarget != null)
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
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var destroyable = collision.transform.GetComponentInParent<IDestroyable>();

        if (destroyable != null)
        {
            if (GoToCoRoutine != null)
                StopCoroutine(GoToCoRoutine);

            HarvestTarget = destroyable;
            State = EState.Harvesting;

            GetComponentInChildren<Animator>().SetBool("IsHarvesting", true);
            StartCoroutine(DamageObstacle(HarvestTime));
        }
    }

    private IEnumerator DamageObstacle(float harvestTime)
    {
        bool destroyed = false;

        while (!destroyed)
        {
            float t = harvestTime;

            while (t > 0.0f)
            {
                t -= Time.deltaTime;
                yield return null;
            }

            if (HarvestTarget != null)
                destroyed = HarvestTarget.DamageObject();
        }

        HarvestTarget = null;
        GoTo(AttackTarget);
        GetComponentInChildren<Animator>().SetBool("IsHarvesting", false);
    }

    public void Damage(int amount)
    {
        Health -= amount;

        StopCoroutine(EffectDamage());
        StartCoroutine(EffectDamage());

        var game = GameObject.Find("Game").GetComponent<Game>();
        game.AudioManager.PlayLayered("ZombieHurt");

        if (Health <= 0)
        {
            Destroy(gameObject);
            game.ZombieKilled.Invoke();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        var damagable = other.GetComponent<Damagable>();

        if (damagable != null)
        {
            damagable.DamageTimer -= Time.deltaTime;

            if (damagable.DamageTimer < 0.0f)
            {
                damagable.DamageTimer = damagable.DamageTime;
                Damage(damagable.DamageAmount);
            }
        }
    }
}
