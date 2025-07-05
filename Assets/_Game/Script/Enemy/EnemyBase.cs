using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    protected Transform player;
    public virtual int Health { get; protected set; }
    public virtual float Speed { get; protected set; }
    public virtual int Damage { get; protected set; }

    public abstract void TakeDamage(int damage);
    public abstract void Idle();
    public abstract void Move(float speed);
    public abstract void Attack();
    public abstract void Die();
    public virtual void Init()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogWarning($"{name}: Player not found!");
            enabled = false;
        }
    }

    public virtual Transform GetPlayer()
    {
        return player;
    }
}
