using System.Collections;
using UnityEngine;

public class MeeleEnemy : EnemyBase, IObserver
{
    // ====== AI settings ======
    [Header("AI Settings")]
    [SerializeField] private float aggroRange = 5f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private float WakeSpeed = 0.3f;
    [SerializeField] private float returnThreshold = 0.4f;

    [Header("Attack Settings")]
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private float attackCooldown = 1.2f;

    [SerializeField] private Transform visual;

    private enum State { Idle, Move, Attack, Die, Return }
    private State currentState = State.Idle;

    private Vector3 startPos;
    private int patrolDir = 1;
    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    /*──────────────────────────────────────*/

    private void Awake()
    {

    }

    private void Start()
    {
        Health = 100;
        Speed = 2f;
        Damage = 10;
        base.Init();
        startPos = transform.position;
        ObServerManager.Instance.addObsever(this);
    }

    private void Update()
    {
        if (currentState != State.Die)
        {
            EvaluateState();
        }

        switch (currentState)
        {
            case State.Idle: Idle(); break;
            case State.Move: Move(Speed); break;
            case State.Attack: Attack(); break;
            case State.Return: ReturnToStart(); break;
            case State.Die: Die(); break;
        }
    }

    /*──────────────────────────────────────*/
    public void EvaluateState()
    {
        if (player == null) return;
        if (currentState == State.Die) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer <= aggroRange)
        {
            currentState = distToPlayer > attackRange ? State.Move : State.Attack;
        }
        else
        {
            float distToStart = Vector3.Distance(transform.position, startPos);

            // Chỉ cho phép Return nếu không phải đang Idle (đang tuần tra)
            if (distToStart > returnThreshold && currentState != State.Idle)
            {
                currentState = State.Return;
            }
            else
            {
                currentState = State.Idle;
            }
        }
    }



    public override void Idle()
    {
        // Tuần tra xung quanh startPos
        transform.Translate(Vector3.right * patrolDir * Speed * WakeSpeed * Time.deltaTime);

        float offset = transform.position.x - startPos.x;
        if (Mathf.Abs(offset) >= patrolDistance)
        {
            patrolDir *= -1;
            float clampX = startPos.x + Mathf.Clamp(offset, -patrolDistance, patrolDistance);
            transform.position = new Vector3(clampX, transform.position.y, transform.position.z);
        }

        Flip(patrolDir);
    }

    public override void Move(float speed)
    {
        if (isAttacking) return;
        Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        Vector3 dir = (targetPos - transform.position).normalized;

        transform.Translate(dir * speed * Time.deltaTime);

        if (dir.x != 0)
            Flip(Mathf.Sign(dir.x));
    }

    private void ReturnToStart()
    {
        Vector3 dir = (startPos - transform.position).normalized;
        transform.Translate(dir * Speed * Time.deltaTime);

        if (dir.x != 0)
            Flip(Mathf.Sign(dir.x));

        // Khi về gần đến startPos thì chuyển sang Idle
        if (Vector3.Distance(transform.position, startPos) < returnThreshold)
        {
            currentState = State.Idle;
        }
    }

    public override void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        StartCoroutine(DoMeleeAttack());
        lastAttackTime = Time.time;
    }

    private IEnumerator DoMeleeAttack()
    {
        isAttacking = true;
        attackHitbox.SetActive(true);           // Bật collider tấn công
        yield return new WaitForSeconds(0.3f);  // Thời gian trúng đòn
        attackHitbox.SetActive(false);
        isAttacking = false;
    }

    public override void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0 && currentState != State.Die)
        {
            currentState = State.Die;
        }
    }

    public override void Die()
    {
        Debug.Log("MeeleEnemy died");
        ReturnPool();
    }

    public void Flip(float dirX)
    {
        if (visual != null)
        {
            Vector3 vScale = visual.localScale;
            vScale.x = dirX * Mathf.Abs(vScale.x);
            visual.localScale = vScale;
        }
    }

    public void ReturnPool()
    {
        // Nếu dùng pooling thì gọi ở đây
        ObServerManager.Instance.removeObsever(this);
        Destroy(gameObject); // hoặc setActive(false) nếu pooling
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}