using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private AudioClip shootClip;
    private AudioClip hitEnemyClip;
    private AudioClip enemyDeathClip;
    private AudioClip explosionClip;
    private AudioClip playerHurtClip;
    private AudioClip healthPickupClip;
    private AudioClip doorOpenClip;
    private AudioClip chestOpenClip;
    private AudioClip bgmClip;

    private bool initialized = false;

    // Music / SFX toggle state, persisted in PlayerPrefs (Lab 5).
    public bool IsMusicOn { get; private set; } = true;
    public bool IsSFXOn   { get; private set; } = true;
    private const string PREF_MUSIC = "ts_music_on";
    private const string PREF_SFX   = "ts_sfx_on";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // Restore the user's last on/off choice (defaults to on).
            IsMusicOn = PlayerPrefs.GetInt(PREF_MUSIC, 1) == 1;
            IsSFXOn   = PlayerPrefs.GetInt(PREF_SFX,   1) == 1;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (!initialized && Instance == this)
        {
            initialized = true;
            SetupSources();
            LoadGeneratedClips();
            StartCoroutine(LoadAllCustomClips());
        }
    }

    void SetupSources()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0.65f;
        musicSource.spatialBlend = 0f;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = 1f;
        sfxSource.spatialBlend = 0f;
        sfxSource.playOnAwake = false;
    }

    // Generate fallback clips first; custom files (Resources/StreamingAssets) override later.
    void LoadGeneratedClips()
    {
        shootClip       = TryLoadResource("shoot")    ?? GenTone("shoot", 0.07f, 700f, 250f, 0.7f);
        hitEnemyClip    = TryLoadResource("hit")      ?? GenTone("hit", 0.05f, 350f, 150f, 0.6f);
        enemyDeathClip  = TryLoadResource("enemy_death") ?? GenTone("death", 0.3f, 450f, 80f, 0.6f);
        explosionClip   = TryLoadResource("explosion") ?? GenMemeBoom();
        playerHurtClip  = TryLoadResource("player_hurt") ?? GenHurt();
        healthPickupClip= TryLoadResource("health_pickup") ?? GenTone("pickup", 0.18f, 600f, 1200f, 0.5f);
        doorOpenClip    = TryLoadResource("door_open") ?? GenTone("door", 0.22f, 350f, 800f, 0.5f);
        chestOpenClip   = TryLoadResource("chest_open") ?? GenChest();
        bgmClip         = TryLoadResource("bgm")      ?? GenBGM();
    }

    static AudioClip TryLoadResource(string name)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/" + name);
        if (clip != null) Debug.Log("[Audio] Loaded from Resources: " + name);
        return clip;
    }

    // Async-load custom audio files from StreamingAssets/Audio/ (.wav, .mp3, .ogg).
    IEnumerator LoadAllCustomClips()
    {
        yield return TryLoadStreaming("shoot",        c => shootClip = c);
        yield return TryLoadStreaming("hit",          c => hitEnemyClip = c);
        yield return TryLoadStreaming("enemy_death",  c => enemyDeathClip = c);
        yield return TryLoadStreaming("explosion",    c => explosionClip = c);
        yield return TryLoadStreaming("player_hurt",  c => playerHurtClip = c);
        yield return TryLoadStreaming("health_pickup", c => healthPickupClip = c);
        yield return TryLoadStreaming("door_open",    c => doorOpenClip = c);
        yield return TryLoadStreaming("chest_open",   c => chestOpenClip = c);
        yield return TryLoadStreaming("bgm",          c => bgmClip = c);

        // Once everything is loaded, start the BGM (or just assign it if music is muted).
        if (bgmClip != null && musicSource != null)
        {
            musicSource.clip = bgmClip;
            if (IsMusicOn) musicSource.Play();
        }

        Debug.Log("[Audio] All clips loaded!");
    }

    // Public music / SFX toggle (Lab 5 requirement).
    public void ToggleMusic()
    {
        IsMusicOn = !IsMusicOn;
        if (musicSource != null)
        {
            if (IsMusicOn)
            {
                if (bgmClip != null && !musicSource.isPlaying) musicSource.Play();
                musicSource.mute = false;
            }
            else
            {
                musicSource.mute = true;
            }
        }
        PlayerPrefs.SetInt(PREF_MUSIC, IsMusicOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleSFX()
    {
        IsSFXOn = !IsSFXOn;
        PlayerPrefs.SetInt(PREF_SFX, IsSFXOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    IEnumerator TryLoadStreaming(string name, System.Action<AudioClip> setter)
    {
        // Unity runtime only supports these three formats via UnityWebRequest.
        string[] supportedExtensions = { ".wav", ".mp3", ".ogg" };
        string basePath = Path.Combine(Application.streamingAssetsPath, "Audio");

        // Warn if a known-unsupported file is present so the user knows to convert it.
        string[] unsupportedCheck = { ".m4a", ".mp4", ".aac", ".flac" };
        foreach (string ext in unsupportedCheck)
        {
            string badPath = Path.Combine(basePath, name + ext);
            if (File.Exists(badPath))
            {
                Debug.LogWarning("[Audio] Found " + name + ext + " but Unity does not support this format at runtime. Please convert it to .mp3, .wav, or .ogg.");
            }
        }

        foreach (string ext in supportedExtensions)
        {
            string filePath = Path.Combine(basePath, name + ext);
            if (!File.Exists(filePath)) continue;

            string url = "file:///" + filePath.Replace("\\", "/");
            AudioType audioType = ext switch
            {
                ".wav" => AudioType.WAV,
                ".mp3" => AudioType.MPEG,
                ".ogg" => AudioType.OGGVORBIS,
                _ => AudioType.UNKNOWN
            };

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    if (clip != null && clip.length > 0)
                    {
                        clip.name = name;
                        setter(clip);
                        Debug.Log("[Audio] Loaded from StreamingAssets: " + name + ext);
                        yield break;
                    }
                }
                else
                {
                    Debug.LogWarning("[Audio] Failed to load " + name + ext + ": " + www.error);
                }
            }
        }
    }

    // ----- Procedural fallback clips (used when no custom file is found) -----

    static AudioClip GenTone(string name, float dur, float f0, float f1, float vol)
    {
        int rate = 44100;
        int n = Mathf.CeilToInt(dur * rate);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / rate;
            float p = (float)i / n;
            float env = 1f - p;
            float freq = Mathf.Lerp(f0, f1, p);
            d[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * vol;
        }
        AudioClip c = AudioClip.Create(name, n, 1, rate, false);
        c.SetData(d, 0);
        return c;
    }

    static AudioClip GenMemeBoom()
    {
        int rate = 44100;
        float dur = 0.6f;
        int n = Mathf.CeilToInt(dur * rate);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / rate;
            float attack = 0f;
            if (t < 0.015f)
            {
                float a = 1f - t / 0.015f;
                attack = Mathf.Sin(2f * Mathf.PI * 120f * t) * a * a * 0.95f;
            }
            float bassEnv = t < 0.02f ? t / 0.02f : Mathf.Exp(-3.5f * (t - 0.02f));
            float bass = Mathf.Sin(2f * Mathf.PI * 42f * t) * bassEnv * 0.9f;
            float midEnv = t < 0.01f ? t / 0.01f : Mathf.Exp(-6f * t);
            float mid = Mathf.Sin(2f * Mathf.PI * 85f * t) * midEnv * 0.5f;
            mid += Mathf.Sin(2f * Mathf.PI * 170f * t) * midEnv * 0.2f;
            float noiseEnv = t < 0.03f ? (1f - t / 0.03f) : 0f;
            float noiseVal = Mathf.Sin(t * 12345.6f + Mathf.Sin(t * 7777f) * 5f);
            float noise = noiseVal * noiseEnv * 0.25f;
            float tailEnv = t > 0.1f ? Mathf.Exp(-4f * (t - 0.1f)) : 0f;
            float tail = Mathf.Sin(2f * Mathf.PI * 38f * t) * tailEnv * 0.3f;
            d[i] = Mathf.Clamp(attack + bass + mid + noise + tail, -1f, 1f);
        }
        AudioClip c = AudioClip.Create("meme_boom", n, 1, rate, false);
        c.SetData(d, 0);
        return c;
    }

    static AudioClip GenHurt()
    {
        int rate = 44100;
        int n = Mathf.CeilToInt(0.12f * rate);
        float[] d = new float[n];
        uint seed = 77777;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / rate;
            float env = 1f - (float)i / n;
            seed = seed * 1103515245 + 12345;
            float nv = ((seed >> 16) & 0x7FFF) / 16383.5f - 1f;
            d[i] = Mathf.Sin(2f * Mathf.PI * 150f * t) * env * 0.5f + nv * env * 0.2f;
        }
        AudioClip c = AudioClip.Create("hurt", n, 1, rate, false);
        c.SetData(d, 0);
        return c;
    }

    static AudioClip GenChest()
    {
        int rate = 44100;
        int n = Mathf.CeilToInt(0.3f * rate);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / rate;
            float p = (float)i / n;
            float env = p < 0.08f ? p / 0.08f : (1f - p);
            float freq = p < 0.35f ? Mathf.Lerp(500f, 900f, p / 0.35f) : Mathf.Lerp(900f, 1300f, (p - 0.35f) / 0.65f);
            d[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.5f;
        }
        AudioClip c = AudioClip.Create("chest", n, 1, rate, false);
        c.SetData(d, 0);
        return c;
    }

    /// <summary>
    /// Procedural dark dungeon BGM made of four low-frequency layers:
    /// 1) sub-bass drone, 2) slow minor chord with vibrato,
    /// 3) low-pass-style wind noise, 4) distant heartbeat pulse.
    /// No bright/treble sounds, so it stays oppressive and atmospheric.
    /// </summary>
    static AudioClip GenBGM()
    {
        int rate = 44100;
        float dur = 32f;                         // 32-second loop
        int n = Mathf.CeilToInt(dur * rate);
        float[] d = new float[n];

        // Roots of the A minor chord progression (A1, G1, Bb1, A1).
        float[] chordRoot = { 55f, 49f, 58.27f, 55f };
        float chordLen = dur / chordRoot.Length;
        float beatPeriod = 1.2f;                 // slow heartbeat
        uint seed = 31415;                       // wind noise seed

        for (int i = 0; i < n; i++)
        {
            float t = (float)i / rate;

            // 1) Sub-bass drone (~28 Hz / 40 Hz) for low-end pressure.
            float subBass = Mathf.Sin(2f * Mathf.PI * 28f * t) * 0.12f
                          + Mathf.Sin(2f * Mathf.PI * 40f * t) * 0.08f;

            // 2) Slow minor triad with a breathing envelope and tiny vibrato.
            int ci = Mathf.FloorToInt(t / chordLen) % chordRoot.Length;
            float chordEnv = Mathf.Max(0.2f,
                0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 0.0625f * t - Mathf.PI * 0.5f));
            float root = chordRoot[ci];
            float chord = Mathf.Sin(2f * Mathf.PI * root * t) * 0.10f
                        + Mathf.Sin(2f * Mathf.PI * root * 1.189f * t) * 0.06f   // minor third
                        + Mathf.Sin(2f * Mathf.PI * root * 1.498f * t) * 0.05f;  // ~fifth
            float vibrato = 1f + 0.012f * Mathf.Sin(2f * Mathf.PI * 4.5f * t);
            chord *= vibrato * chordEnv;

            // 3) Wind noise: cheap white-noise generator scaled by a slow LFO.
            seed = seed * 1664525u + 1013904223u;
            float rawNoise = ((seed >> 16) & 0x7FFF) / 16383.5f - 1f;
            float windLfo = Mathf.Max(0f,
                0.4f + 0.6f * Mathf.Sin(2f * Mathf.PI * 0.13f * t + Mathf.Sin(t * 0.3f)));
            float wind = rawNoise * 0.5f * windLfo * 0.05f;

            // 4) Heartbeat: two short low pulses ("thump-thump") every 1.2 seconds.
            float bt = (t % beatPeriod) / beatPeriod;
            float beat1Env = bt < 0.03f ? bt / 0.03f : Mathf.Exp(-12f * (bt - 0.03f));
            float beat2T = bt - 0.18f;
            float beat2Env = beat2T < 0f ? 0f
                : (beat2T < 0.03f ? beat2T / 0.03f : Mathf.Exp(-12f * (beat2T - 0.03f)));
            float heartbeat = Mathf.Sin(2f * Mathf.PI * 50f * t)
                              * (beat1Env + beat2Env * 0.7f) * 0.10f;

            // Very slow global breathing so the loop never feels static.
            float globalBreath = 0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * 0.033f * t);

            float sample = (subBass + chord + wind + heartbeat) * globalBreath;
            d[i] = Mathf.Clamp(sample, -1f, 1f);
        }

        AudioClip c = AudioClip.Create("bgm", n, 1, rate, false);
        c.SetData(d, 0);
        return c;
    }

    // ----- Public playback API -----

    public void PlaySFX(AudioClip clip)
    {
        if (!IsSFXOn) return; // SFX muted by the player
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayShoot() => PlaySFX(shootClip);
    public void PlayHitEnemy() => PlaySFX(hitEnemyClip);
    public void PlayExplosion()
    {
        if (!IsSFXOn) return;
        // Play the explosion clip at higher volume so the impact really lands.
        if (explosionClip != null && sfxSource != null)
            sfxSource.PlayOneShot(explosionClip, 1.4f);
    }
    public void PlayPlayerHurt() => PlaySFX(playerHurtClip);
    public void PlayHealthPickup() => PlaySFX(healthPickupClip);
    public void PlayDoorOpen() => PlaySFX(doorOpenClip);
    public void PlayEnemyDeath() => PlaySFX(enemyDeathClip);
    public void PlayChestOpen() => PlaySFX(chestOpenClip);
}
