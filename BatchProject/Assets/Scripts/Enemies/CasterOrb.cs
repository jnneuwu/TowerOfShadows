using UnityEngine;

public class CasterOrb : MonoBehaviour
{
    public float speed = 4f;
    public float turnRate = 50f;
    public int damage = 20;
    public Transform target;
    public float lifetime = 6f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (target == null)
        {
            if (PlayerController.Instance != null) target = PlayerController.Instance.transform;
        }

        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (target == null) return;
        Vector2 dir = ((Vector2)target.position - rb.position).normalized;
        Vector2 current = rb.linearVelocity.normalized;
        if (current == Vector2.zero) current = dir;
        Vector2 newDir = Vector3.RotateTowards(current, dir, turnRate * Mathf.Deg2Rad * Time.fixedDeltaTime, 0f);
        rb.linearVelocity = newDir * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController p = other.GetComponent<PlayerController>();
        if (p != null)
        {
            p.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
        if (other.CompareTag("Wall") || other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
