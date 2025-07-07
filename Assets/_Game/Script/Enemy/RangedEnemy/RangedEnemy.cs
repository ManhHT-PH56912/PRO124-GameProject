using UnityEngine;
using System.Collections;

public enum RangedState
{
    Idle,
    Attack,
    Flee,
    Die
}

public class RangedEnemy : EnemyBase, IObserver
{
    [Header("Ranged Settings")]
    public float aggroRange = 5f;
    public float fleeRange = 2f;
    public float patrolRadius = 3f;
    public float fleeDuration = 2f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [SerializeField] private float shootCooldown = 1f;
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float edgeEscapeDistance = 4f; // khoảng cách né vực

    private float lastShootTime = -Mathf.Infinity;
    private bool hasFleeRedirected = false;
    private Vector3 redirectedTarget;

    private RangedState currentState = RangedState.Idle;
    private Vector3 patrolCenter;
    private Vector3 patrolTarget;
    private float fleeTimer;
    private bool isFleeing = false;

    private void Awake()
    {
        Init();
        patrolCenter = transform.position;
        SetNewPatrolTarget();
    }

    private void Start()
    {
        Speed = 2f;
        Health = 40;
        ObServerManager.Instance.addObsever(this);
    }

    private void Update()
    {
        if (currentState == RangedState.Die || player == null) return;

        // ✅ KHÔNG đánh giá trạng thái khi đang né vực
        if (!(currentState == RangedState.Flee && hasFleeRedirected))
        {
            EvaluateState();
        }

        switch (currentState)
        {
            case RangedState.Idle: Idle(); break;
            case RangedState.Attack: Attack(); break;
            case RangedState.Flee: Flee(); break;
        }
    }

    private void EvaluateState()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= fleeRange)
        {
            if (!isFleeing)
            {
                currentState = RangedState.Flee;
                isFleeing = true;
                fleeTimer = fleeDuration;
                hasFleeRedirected = false;
            }
        }
        else if (dist <= aggroRange)
        {
            if (!isFleeing)
                currentState = RangedState.Attack;
        }
        else
        {
            if (!isFleeing)
                currentState = RangedState.Idle;
        }
    }

    public override void Idle()
    {
        Patrol();
    }

    public override void Move(float speed) { }

    private void Patrol()
    {
        Vector2 dir = new Vector2(patrolTarget.x - transform.position.x, 0).normalized;
        if (dir.x != 0)
            transform.localScale = new Vector3(Mathf.Sign(dir.x), 1, 1);

        transform.Translate(dir * Speed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x - patrolTarget.x) < 0.2f)
        {
            SetNewPatrolTarget();
        }
    }

    private void SetNewPatrolTarget()
    {
        float offsetX = Random.Range(-patrolRadius, patrolRadius);
        patrolTarget = new Vector3(patrolCenter.x + offsetX, transform.position.y, transform.position.z);
    }

    public override void Attack()
    {
        Vector2 dir = new Vector2(player.position.x - transform.position.x, 0).normalized;
        if (dir.x != 0)
            transform.localScale = new Vector3(Mathf.Sign(dir.x), 1, 1);

        if (Time.time >= lastShootTime + shootCooldown)
        {
            Shoot();
            lastShootTime = Time.time;
        }
    }

    private void Shoot()
    {
        if (bulletPrefab && firePoint)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            bullet.GetComponent<Bullet>()?.Init(direction);
        }
    }

    private void Flee()
    {
        fleeTimer -= Time.deltaTime;
        Vector2 dir;

        // Redirect nếu phía trước không có ground
        if (!hasFleeRedirected && !IsGroundAhead())
        {
            hasFleeRedirected = true;
            float escapeDir = -Mathf.Sign(transform.localScale.x);
            redirectedTarget = transform.position + new Vector3(escapeDir * edgeEscapeDistance, 0, 0);
        }

        if (hasFleeRedirected)
        {
            dir = (redirectedTarget - transform.position).normalized;

            if (Vector2.Distance(transform.position, redirectedTarget) < 0.2f)
            {
                isFleeing = false;
                hasFleeRedirected = false;
                currentState = RangedState.Attack;
                return;
            }
        }
        else
        {
            dir = new Vector2(transform.position.x - player.position.x, 0).normalized;

            if (fleeTimer <= 0f)
            {
                isFleeing = false;
                currentState = RangedState.Attack;
            }
        }

        if (dir.x != 0)
            transform.localScale = new Vector3(Mathf.Sign(dir.x), 1, 1);

        transform.Translate(dir * Speed * Time.deltaTime);
    }

    private bool IsGroundAhead()
    {
        Vector2 offset = new Vector2(Mathf.Sign(transform.localScale.x) * 1f, 0); // phía trước
        Vector2 origin = (Vector2)transform.position + offset;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);

#if UNITY_EDITOR
        Debug.DrawRay(origin, Vector2.down * groundCheckDistance, Color.red);
#endif

        return hit.collider != null;
    }

    public override void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0 && currentState != RangedState.Die)
        {
            currentState = RangedState.Die;
            Die();
        }
    }

    public override void Die()
    {
        Debug.Log("RangedEnemy died");
        ObServerManager.Instance.removeObsever(this);
        gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }
#endif

    public void ReturnPool()
    {
        gameObject.SetActive(false);
    }
}
