using UnityEngine;

public class Enemy : MonoBehaviour
{
    EnemyData data;
    public float speed = 4f;
    Rigidbody2D rb;

    EnemyPool ownerPool;

    public System.Action<Enemy> OnDeath;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(EnemyData enemyData, EnemyPool pool)
    {
        data = enemyData;
        ownerPool = pool;

        speed = data.moveSpeed;

        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    private void FixedUpdate()
    {
        rb.linearVelocityX = speed;
    }

    public void Death()
    {
        OnDeath?.Invoke(this);

        ReturnToPool();
    }

    public void ReturnToPool()
    {
        if (ownerPool != null)
        {
            ownerPool.ReturnEnemy(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Collider2D>(), GetComponent<Collider2D>(), true);
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            
            if (playerStats.isInvincible || playerStats.isDead)
            {
                Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Collider2D>(), GetComponent<Collider2D>(), true);
                return;
            }

            playerStats.TakeDamage(1);

            Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Collider2D>(), GetComponent<Collider2D>(), true);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x *= -1;
            transform.localScale = currentScale;

            speed *= -1;
        }
    }
}
