using UnityEngine;

/// <summary>
/// Smoothly pulses a SimpleLight's intensity.
/// Used on pickups, portals, and weapon drops to draw the player's eye.
/// </summary>
[RequireComponent(typeof(SimpleLight))]
public class PulsingLight : MonoBehaviour
{
    public float minIntensity = 0.4f;
    public float maxIntensity = 1.0f;
    public float speed = 2.5f;

    SimpleLight light2D;

    void Start()
    {
        light2D = GetComponent<SimpleLight>();
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
        light2D.SetIntensity(Mathf.Lerp(minIntensity, maxIntensity, t));
    }
}
