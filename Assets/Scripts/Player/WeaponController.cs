using System.Collections;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public PlayerStats player;
    public PlayerMovement movement;

    public float timePerKill = 1f;

    public AudioSource source;
    public AudioClip snip;

    bool once;

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

            player.GainXP(100);

            if(!player.insanityActive)
                player.DelayInsanity(timePerKill);

            
            if(!once)
                StartCoroutine(PlayAudio());

            collision.GetComponent<Enemy>().Death();
        }
        else if (collision.gameObject.CompareTag("Boss"))
        {
            if (collision.gameObject.GetComponent<Boss>().IsInvulnerable()) return;

            collision.gameObject.GetComponent<Boss>().TakeDamage(1);

            if(!once)
                StartCoroutine(PlayAudio());

            StartCoroutine(PassThrough());
        }
    }

    IEnumerator PlayAudio()
    {
        source.PlayOneShot(snip);

        once = true;

        yield return new WaitForSeconds(0.2f);

        once = false;
    }
}
