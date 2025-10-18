using UnityEngine;

public class Enemy : MonoBehaviour
{
    EnemyData data;
    public float speed = 4f;
    Rigidbody2D rb;

    EnemyPool ownerPool;

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
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(1);
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
