using UnityEngine;

public class WeaponChest : MonoBehaviour
{
    public WeaponData.WeaponType weaponType;
    public bool isOpened = false;
    public float interactRange = 1.8f;

    private SpriteRenderer sr;
    private Color closedColor = new Color(0.85f, 0.65f, 0.15f);
    private Color openedColor = new Color(0.4f, 0.35f, 0.25f, 0.5f);

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = closedColor;
    }

    void Update()
    {
        if (isOpened) return;

        PlayerController player = PlayerController.Instance;
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.transform.position);
        if (dist < interactRange)
        {
            WeaponData.WeaponDef weapon = WeaponData.Get(weaponType);
            if (UIManager.Instance != null)
                UIManager.Instance.ShowInteractPrompt("Press E to open  [" + weapon.Name + "]");

            if (Input.GetKeyDown(KeyCode.E))
            {
                Open();
            }
        }
    }

    void Open()
    {
        if (isOpened) return;
        isOpened = true;

        if (sr != null) sr.color = openedColor;

        // Drop a weapon pickup just in front of the chest so the player can choose
        // whether to swap with their current weapon (using G).
        Vector3 dropPos = transform.position + new Vector3(0f, -0.6f, 0f);
        WeaponPickup.Spawn(dropPos, weaponType);

        if (AudioManager.Instance != null) AudioManager.Instance.PlayChestOpen();
    }
}
