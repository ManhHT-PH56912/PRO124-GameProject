// EnemyBOOM.cs
using UnityEngine;

public class EnemyBOOM : MonoBehaviour, IEnemyBase
{
    // ====== IEnemyBase properties ======
    public int Health { get; set; } = 100;
    public float Speed { get; set; } = 3.5f;
    public int Damage { get; set; } = 10;

    // ====== AI ranges & patrol ======
    [Header("AI Settings")]
    [SerializeField] private float aggroRange = 5f;  // Idle → Move
    [SerializeField] private float attackRange = 2f;  // Move → Attack
    [SerializeField] private float patrolDistance = 3f;

    private Vector3 startPos;
    private int patrolDir = 1;        // +1 sang phải, ‑1 sang trái
    private Transform player;             // cache player transform

    // ====== FSM ======
    private enum State { Idle, Move, Attack, Die }
    private State currentState = State.Idle;

    /*──────────────────────────────────────*/
    private void Awake()
    {
        startPos = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)            // Không tìm thấy Player
        {
            Debug.LogWarning("EnemyBOOM: Player tag not found → script disabled.");
            enabled = false;
        }
    }

    private void Update()
    {
        if (currentState != State.Die) EvaluateState();

        switch (currentState)
        {
            case State.Idle: Idle(); break;
            case State.Move: Move(Speed); break;
            case State.Attack: Attack(); break;
            case State.Die: Die(); break;
        }
    }

    /*──────────────────────────────────────*/
    /// <summary> Xác định trạng thái mới dựa trên khoảng cách Player </summary>
    private void EvaluateState()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > aggroRange) currentState = State.Idle;
        else if (dist > attackRange) currentState = State.Move;
        else currentState = State.Attack;
    }

    /*──────────────── IEnemyBase methods ────────────────*/
    public void Idle()
    {
        // Tuần tra qua lại trục X
        transform.Translate(Vector3.right * patrolDir * Speed * Time.deltaTime);

        float offset = transform.position.x - startPos.x;
        if (Mathf.Abs(offset) >= patrolDistance)
        {
            patrolDir *= -1; // đổi hướng
            float clampX = startPos.x + Mathf.Clamp(offset, -patrolDistance, patrolDistance);
            transform.position = new Vector3(clampX, transform.position.y, transform.position.z);
        }
    }

    public void Move(float speed)
    {
        // Đuổi theo người chơi (trên mặt phẳng X‑Z)
        Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        Vector3 dir = (targetPos - transform.position).normalized;

        transform.Translate(dir * speed * Time.deltaTime);

        // Xoay mặt về hướng di chuyển
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    public void Attack()
    {
        Debug.Log($"Enemy attacks player for {Damage} dmg");
        // Thực thi sát thương: animation, collision, v.v.
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0 && currentState != State.Die)
            currentState = State.Die;
    }

    public int GetHealth() => Health;

    public void Die()
    {
        Debug.Log("Enemy died");
        Destroy(gameObject);
    }

#if UNITY_EDITOR   // Vẽ gizmo phạm vi cho dễ chỉnh
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}
