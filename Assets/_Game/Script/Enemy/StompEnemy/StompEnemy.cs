using UnityEngine;

public class StompEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 3f;

    [Header("Detection")]
    public LayerMask playerLayer;
    public Transform visual;
    public float stompOffsetY = 0.5f; // Độ cao để tính "đạp đầu"

    private Rigidbody2D rb;
    private Vector2 startPos;
    private int dir = 1;
    private bool reachedEdge = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
    }

    [System.Obsolete]
    private void Update()
    {
        Patrol();
    }

    [System.Obsolete]
    private void Patrol()
{
    rb.velocity = new Vector2(patrolSpeed * dir, rb.velocity.y);

    float dist = transform.position.x - startPos.x;

    // Khi đến biên, chỉ lật hướng 1 lần
    if (!reachedEdge && Mathf.Abs(dist) >= patrolDistance)
    {
        dir *= -1;
        reachedEdge = true;

        // Clamp lại vị trí để không vượt quá giới hạn
        float clampedX = startPos.x + Mathf.Clamp(dist, -patrolDistance, patrolDistance);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }

    // Khi đã quay đầu và rời khỏi mép, cho phép quay đầu lần sau
    if (reachedEdge && Mathf.Abs(dist) < patrolDistance - 0.1f)
    {
        reachedEdge = false;
    }

    // Flip hình ảnh nếu có
    if (visual != null)
    {
        Vector3 scale = visual.localScale;
        scale.x = dir * Mathf.Abs(scale.x);
        visual.localScale = scale;
    }
}


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            float playerY = collision.transform.position.y;
            float enemyY = transform.position.y;

            // Nếu player cao hơn đầu quái + offset => được coi là đạp đầu
            if (playerY > enemyY + stompOffsetY)
            {
                Debug.Log("Player đạp chết StompEnemy!");
                Die();

                // Player nảy lên
                PlayerJumpBounce bounce = collision.collider.GetComponent<PlayerJumpBounce>();
                if (bounce != null) bounce.Bounce();
            }
            else
            {
                // Nếu đụng ngang thì gây sát thương cho Player
                // collision.collider.GetComponent<PlayerHealth>()?.TakeDamage(10);
                Debug.Log("Player bị thương vì đụng ngang StompEnemy");
            }
        }
    }

    private void Die()
    {
        Debug.Log("Enemy bị đạp chết!");
        Destroy(gameObject); // Hoặc setActive(false) nếu dùng pooling
    }
}
