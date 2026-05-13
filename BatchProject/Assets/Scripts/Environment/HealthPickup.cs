using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healAmount = 15;
    public float lifetime = 30f;
    private float bobOffset;

    void Start()
    {
        bobOffset = Random.value * Mathf.PI * 2;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Bobbing effect
        float y = Mathf.Sin(Time.time * 3f + bobOffset) * 0.1f;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + y * Time.deltaTime, 0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}
