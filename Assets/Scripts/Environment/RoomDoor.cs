using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    public bool isOpen = false;
    private Collider2D col;
    private SpriteRenderer sr;

    void Start()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        if (isOpen) Open();
        else Close();
    }

    public void Open()
    {
        isOpen = true;
        if (col != null) col.enabled = false;
        if (sr != null) sr.color = new Color(0.2f, 0.8f, 0.3f, 0.3f);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayDoorOpen();
    }

    public void Close()
    {
        isOpen = false;
        if (col != null) col.enabled = true;
        if (sr != null) sr.color = new Color(0.2f, 0.8f, 0.3f, 1f);
    }
}
