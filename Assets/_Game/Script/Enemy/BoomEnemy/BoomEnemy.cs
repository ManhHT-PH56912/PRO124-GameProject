using System.Collections;
using UnityEngine;

public enum State
{
    Idle,
    Move,
    Attack,
    Die
}

public class EnemyBOOM : EnemyBase, IObserver
{
    [Header("BoomEnemy Settings")]
    [SerializeField] private float aggroRange = 5f;
    [SerializeField] private float triggerRange = 1f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private int explosionDamage = 40;
    [SerializeField] private float chaseTimeout = 4f;

    private State currentState = State.Idle;
    private State lastState = State.Idle;

    private Vector3 spawnPos;
    private bool hasAggroed = false;
    private float chaseTimer = 0f;

    private void Awake()
    {
        Init();
        spawnPos = transform.position;
    }

    private void Start()
    {
        Speed = 2f;
        Health = 50;
        Damage = explosionDamage;

        ObServerManager.Instance.addObsever(this);
    }

    private void Update()
    {
        if (currentState == State.Die || player == null) return;

        EvaluateState();

        // Reset timer nếu trạng thái vừa chuyển sang Move
        if (currentState != lastState)
        {
            if (currentState == State.Move)
            {
                chaseTimer = 0f;
            }

            lastState = currentState;
        }

        switch (currentState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Move:
                Move(Speed);
                break;
            case State.Attack:
                Attack();
                break;
        }
    }

    private void EvaluateState()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= aggroRange)
        {
            hasAggroed = true;
        }

        if (!hasAggroed)
        {
            currentState = State.Idle;
            return;
        }

        if (dist <= triggerRange)
        {
            currentState = State.Attack;
        }
        else
        {
            currentState = State.Move;
        }
    }

    public override void Idle()
    {
        // Không làm gì khi idle
    }

    public override void Move(float speed)
    {
        chaseTimer += Time.deltaTime;

        Vector3 dir = (player.position - transform.position).normalized;
        transform.Translate(dir * speed * Time.deltaTime);

        if (chaseTimer >= chaseTimeout)
        {
            Attack();
            chaseTimer = 0f;
        }
    }

    public override void Attack()
    {
        Debug.Log("BoomEnemy Attack");

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // hit.GetComponent<PlayerHealth>()?.TakeDamage(explosionDamage);
                Debug.Log("BoomEnemy gây sát thương cho Player");
            }
        }

        currentState = State.Die;
        StartCoroutine(DelayedDie(1.5f));
    }

    public override void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0 && currentState != State.Die)
        {
            currentState = State.Die;
            Die();
        }
    }

    public override void Die()
    {

        Debug.Log("BoomEnemy died");
        ReturnPool();
    }

    public void ReturnPool()
    {
        ObServerManager.Instance.removeObsever(this);
        currentState = State.Idle;
        lastState = State.Idle;
        hasAggroed = false;
        chaseTimer = 0f;
        transform.position = spawnPos;
        gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
    private IEnumerator DelayedDie(float delay)
    {
        Debug.Log("Anim Die");
        yield return new WaitForSeconds(delay);
        Die();
    }

#endif
}
