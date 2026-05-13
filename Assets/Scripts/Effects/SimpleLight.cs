using UnityEngine;

/// <summary>
/// Sprite-based 2D light. Renders a radial gradient sprite tinted by colour and intensity.
/// Works in any render pipeline (no URP required).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SimpleLight : MonoBehaviour
{
    public Color lightColor = new Color(1f, 0.85f, 0.4f);
    [Range(0f, 2f)] public float intensity = 1f;
    public float radius = 3f;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sortingOrder = 50;
    }

    void LateUpdate()
    {
        sr.color = new Color(lightColor.r, lightColor.g, lightColor.b, Mathf.Clamp01(intensity * 0.6f));
        transform.localScale = Vector3.one * radius;
    }

    public void SetIntensity(float i) => intensity = i;
    public void SetColor(Color c) => lightColor = c;
}
