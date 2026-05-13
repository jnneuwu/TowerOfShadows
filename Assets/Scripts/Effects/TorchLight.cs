using UnityEngine;

/// <summary>
/// Animated torch: makes the attached SimpleLight flicker over time.
/// Provides the "dynamic lighting" required by Lab 6.
/// </summary>
[RequireComponent(typeof(SimpleLight))]
public class TorchLight : MonoBehaviour
{
    public float baseIntensity = 0.85f;
    public float flickerAmount = 0.35f;
    public float flickerSpeed = 7f;

    SimpleLight light2D;
    float seed;

    void Start()
    {
        light2D = GetComponent<SimpleLight>();
        seed = Random.Range(0f, 100f);
    }

    void Update()
    {
        float t = Time.time * flickerSpeed + seed;
        float n = Mathf.Sin(t) * 0.5f + Mathf.Sin(t * 1.7f) * 0.3f + Mathf.Sin(t * 3.1f) * 0.2f;
        light2D.SetIntensity(baseIntensity + n * flickerAmount);
    }
}
