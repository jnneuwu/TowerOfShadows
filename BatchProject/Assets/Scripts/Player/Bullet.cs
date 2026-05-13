using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 10;
    public bool isPlayerBullet = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayHitEnemy();
                Destroy(gameObject);
                return;
            }
            ExplosiveCrate crate = other.GetComponent<ExplosiveCrate>();
            if (crate != null)
            {
                crate.Explode();
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }

        if (other.CompareTag("Wall") || other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Wall") || col.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
