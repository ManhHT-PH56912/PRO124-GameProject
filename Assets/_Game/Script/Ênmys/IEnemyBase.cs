public interface IEnemyBase
{
    // Properties
    int Health { get; set; }
    float Speed { get; set; }
    int Damage { get; set; }

    void TakeDamage(int damage);
    int GetHealth();

    // State EmemuBase
    void Idle();
    void Move(float speed);
    void Attack();
    void Die();
}
