using UnityEngine;

public class Portal : MonoBehaviour
{
    public bool isActive = false;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        isActive = true;
        gameObject.SetActive(true);
        if (sr != null) sr.color = new Color(0.92f, 0.76f, 1f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        if (other.GetComponent<PlayerController>() != null)
        {
            if (GameManager.Instance != null) GameManager.Instance.NextFloor();
        }
    }
}
