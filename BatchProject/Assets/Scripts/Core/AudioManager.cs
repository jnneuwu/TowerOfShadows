using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    
    [Header("SFX Clips")]
    public AudioClip shootClip;
    public AudioClip hitEnemyClip;
    public AudioClip hitWallClip;
    public AudioClip enemyDeathClip;
    public AudioClip explosionClip;
    public AudioClip playerHurtClip;
    public AudioClip healthPickupClip;
    public AudioClip doorOpenClip;
    public AudioClip chargerWindupClip;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.volume = 0.3f;
            }
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.volume = 0.5f;
            }
        }
        else Destroy(gameObject);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayShoot() => PlaySFX(shootClip);
    public void PlayHitEnemy() => PlaySFX(hitEnemyClip);
    public void PlayExplosion() => PlaySFX(explosionClip);
    public void PlayPlayerHurt() => PlaySFX(playerHurtClip);
    public void PlayHealthPickup() => PlaySFX(healthPickupClip);
    public void PlayDoorOpen() => PlaySFX(doorOpenClip);
    public void PlayEnemyDeath() => PlaySFX(enemyDeathClip);
}
