using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 10;
    public bool isPlayerBullet = true;
    public BulletEffect effect = BulletEffect.None;
    public float effectDuration;
    public float effectStrength;
    private int pierceCount = 0;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                if (effect == BulletEffect.Burn)
                    enemy.ApplyBurn(effectDuration, effectStrength);
                else if (effect == BulletEffect.Slow)
                    enemy.ApplySlow(effectDuration, effectStrength);

                if (AudioManager.Instance != null) AudioManager.Instance.PlayHitEnemy();

                if (effect == BulletEffect.Pierce)
                {
                    pierceCount++;
                    if (pierceCount >= 3) Destroy(gameObject);
                    return;
                }
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
