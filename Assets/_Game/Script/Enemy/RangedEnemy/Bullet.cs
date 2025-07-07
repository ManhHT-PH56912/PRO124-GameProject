using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;
    private Vector2 direction;

    public void Init(Vector2 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, 3f); 
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
        if (Mathf.Abs(transform.position.x) > 10f || Mathf.Abs(transform.position.y) > 20f)
        {
            Destroy(gameObject); // Destroy bullet if it goes too far
        }
    }

     private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // collision.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
