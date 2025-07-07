using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Hit player!");

            // Gọi hàm mất máu từ Player
            // PlayerHealth player = other.GetComponent<PlayerHealth>();
            // if (player != null)
            // {
            //     player.TakeDamage(damage);
            // }                                            
        }
    }

    // Cho phép Enemy gán sát thương từ ngoài vào nếu cần
    public void SetDamage(int dmg)
    {
        damage = dmg;
    }
}
