using System.Collections;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public PlayerStats player;
    public PlayerMovement movement;

    IEnumerator PassThrough()
    {
        Physics2D.IgnoreCollision(
            movement.lastPlatform.GetComponent<Collider2D>(),
            player.gameObject.GetComponent<Collider2D>(),
            true
        );

        yield return new WaitForSeconds(0.85f);

        Physics2D.IgnoreCollision(
            movement.lastPlatform.GetComponent<Collider2D>(),
            player.gameObject.GetComponent<Collider2D>(),
            false
        );
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !player.isInvincible && !player.isDead)
        {
            Debug.Log("Enemy hit!");

            player.xp += 100;

            if (player.xp >= player.xpThreshold) 
            {
                player.LevelUp();
            }

            if(!player.insanityActive)
                player.IncreaseInsanity();

            collision.GetComponent<Enemy>().Death();
        }
        else if (collision.gameObject.CompareTag("Boss"))
        {
            collision.gameObject.GetComponent<Boss>().TakeDamage(1);

            StartCoroutine(PassThrough());
        }
    }
}
