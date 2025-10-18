using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public PlayerHealth player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Enemy hit!");

            player.xp += 100;

            if (player.xp >= player.xpThreshold) 
            {
                player.LevelUp();
            }

            collision.GetComponent<Enemy>().Death();
        }
    }
}
