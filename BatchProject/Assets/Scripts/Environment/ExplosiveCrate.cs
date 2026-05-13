using UnityEngine;

public class ExplosiveCrate : MonoBehaviour
{
    public float blastRadius = 3f;
    public int blastDamage = 50;
    public GameObject explosionEffectPrefab;
    public bool showBlastRadius = true;
    public int rangeSegments = 48;

    private bool exploded = false;
    private LineRenderer blastRing;
    private static Material blastRingMaterial;

    void Start()
    {
        CreateBlastRing();
    }

    public void Explode()
    {
        if (exploded) return;
        exploded = true;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayExplosion();

        // AOE damage
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, blastRadius);
        foreach (var hit in hits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null) enemy.TakeDamage(blastDamage);

            PlayerController player = hit.GetComponent<PlayerController>();
            if (player != null) player.TakeDamage(blastDamage);
        }

        // Visual effect
        if (explosionEffectPrefab != null)
        {
            GameObject fx = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1f);
        }

        Destroy(gameObject);
    }

    void CreateBlastRing()
    {
        if (!showBlastRadius) return;

        GameObject ringObject = new GameObject("BlastRadius");
        ringObject.transform.SetParent(transform, false);
        ringObject.transform.localPosition = Vector3.zero;

        blastRing = ringObject.AddComponent<LineRenderer>();
        blastRing.useWorldSpace = false;
        blastRing.loop = true;
        blastRing.positionCount = Mathf.Max(16, rangeSegments);
        blastRing.widthMultiplier = 0.08f;
        blastRing.numCornerVertices = 4;
        blastRing.numCapVertices = 4;
        blastRing.material = GetBlastRingMaterial();
        blastRing.startColor = new Color(1f, 0.8f, 0.25f, 0.8f);
        blastRing.endColor = new Color(1f, 0.8f, 0.25f, 0.8f);
        blastRing.sortingOrder = 5;

        for (int i = 0; i < blastRing.positionCount; i++)
        {
            float angle = (Mathf.PI * 2f * i) / blastRing.positionCount;
            Vector3 point = new Vector3(Mathf.Cos(angle) * blastRadius, Mathf.Sin(angle) * blastRadius, 0f);
            blastRing.SetPosition(i, point);
        }
    }

    static Material GetBlastRingMaterial()
    {
        if (blastRingMaterial == null)
        {
            blastRingMaterial = new Material(Shader.Find("Sprites/Default"));
        }
        return blastRingMaterial;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, blastRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, blastRadius);
    }
}
