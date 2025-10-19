using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public PlayerStats player;

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
    }
}
