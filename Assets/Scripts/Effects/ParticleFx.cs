using UnityEngine;

/// <summary>
/// Static helper for spawning short-lived particle bursts and a brief muzzle flash light.
/// Used by shooting, enemy death, and explosions.
/// </summary>
public static class ParticleFx
{
    static Material spriteMat;
    static Sprite lightSprite;

    static Material SpriteMat
    {
        get
        {
            if (spriteMat == null) spriteMat = new Material(Shader.Find("Sprites/Default"));
            return spriteMat;
        }
    }

    public static void SetLightSprite(Sprite s) { lightSprite = s; }

    /// <summary>Spawn a one-shot particle burst at world position.</summary>
    public static void Burst(Vector3 pos, Color color, int count = 14, float speed = 5f, float life = 0.45f, float size = 0.18f)
    {
        GameObject obj = new GameObject("FxBurst");
        obj.transform.position = pos;

        var ps = obj.AddComponent<ParticleSystem>();
        ps.Stop();

        var main = ps.main;
        main.duration = 0.2f;
        main.loop = false;
        main.startLifetime = life;
        main.startSpeed = speed;
        main.startSize = size;
        main.startColor = color;
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = count + 2;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(BuildFadeOutGradient(color));

        var psr = ps.GetComponent<ParticleSystemRenderer>();
        psr.material = SpriteMat;
        psr.sortingOrder = 30;

        ps.Play();
        Object.Destroy(obj, life + 0.5f);
    }

    /// <summary>Spawn a brief flash of light (muzzle flash, hit spark).</summary>
    public static void Flash(Vector3 pos, Color color, float radius = 1.4f, float duration = 0.08f)
    {
        if (lightSprite == null) return;
        GameObject obj = new GameObject("FxFlash");
        obj.transform.position = pos;
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = lightSprite;
        sr.color = new Color(color.r, color.g, color.b, 0.85f);
        sr.sortingOrder = 51;
        obj.transform.localScale = Vector3.one * radius;
        obj.AddComponent<FadeAndDestroy>().lifetime = duration;
    }

    static Gradient BuildFadeOutGradient(Color c)
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        return g;
    }
}

/// <summary>Helper that fades a SpriteRenderer's alpha to 0 then destroys the object.</summary>
public class FadeAndDestroy : MonoBehaviour
{
    public float lifetime = 0.1f;
    SpriteRenderer sr;
    float t;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        t = lifetime;
    }

    void Update()
    {
        t -= Time.deltaTime;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Clamp01(t / lifetime) * 0.85f;
            sr.color = c;
        }
        if (t <= 0) Destroy(gameObject);
    }
}
