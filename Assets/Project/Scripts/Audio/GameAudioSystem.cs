using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAudioSystem : MonoBehaviour
{
    public static GameAudioSystem Instance { get; private set; }

    [Header("Audio Source Pool")]
    [SerializeField] private int audioSourcePoolSize = 8;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.35f;

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusicClip;
    [SerializeField] private bool playMusicOnStart = true;
    [SerializeField] private bool loopBackgroundMusic = true;
    [SerializeField] private float musicFadeInDuration = 1.2f;
    [SerializeField] private float musicFadeOutDuration = 0.6f;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip placeBlockClip;
    [SerializeField] private AudioClip mixClip;
    [SerializeField] private AudioClip ashClip;
    [SerializeField] private AudioClip clearLineClip;
    [SerializeField] private AudioClip bombClip;
    [SerializeField] private AudioClip invalidDropClip;
    [SerializeField] private AudioClip buttonClickClip;

    [Header("Pitch Random")]
    [SerializeField] private bool randomizePitch = true;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    [Header("Editor Test")]
    [SerializeField] private bool enableEditorTestKeys = true;
    [SerializeField] private bool showDebugButtons = true;
    [SerializeField] private bool logAudioDebug = true;

    private readonly List<AudioSource> audioSources = new List<AudioSource>();
    private AudioSource musicSource;

    private int currentSourceIndex;
    private Coroutine musicFadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CreateMusicSource();
        CreateAudioSourcePool();
    }

    private void Start()
    {
        DebugCheckAudioSetup();

        if (playMusicOnStart)
        {
            PlayBackgroundMusic();
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!enableEditorTestKeys)
            return;

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Audio Test Key: P = Place Block");
            PlayPlaceBlock();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("Audio Test Key: M = Mix");
            PlayMix();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("Audio Test Key: A = Ash");
            PlayAsh();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Audio Test Key: C = Clear Line");
            PlayClearLine();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("Audio Test Key: B = Bomb");
            PlayBomb();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("Audio Test Key: I = Invalid Drop");
            PlayInvalidDrop();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("Audio Test Key: U = Button Click");
            PlayButtonClick();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("Audio Test Key: N = Play Background Music");
            PlayBackgroundMusic();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("Audio Test Key: V = Stop Background Music");
            StopBackgroundMusic();
        }
    }

    private void OnGUI()
    {
        if (!showDebugButtons)
            return;

        GUI.Box(new Rect(10, 10, 230, 325), "Audio Test");

        if (GUI.Button(new Rect(20, 40, 210, 25), "P - Place Block"))
        {
            PlayPlaceBlock();
        }

        if (GUI.Button(new Rect(20, 70, 210, 25), "M - Mix"))
        {
            PlayMix();
        }

        if (GUI.Button(new Rect(20, 100, 210, 25), "A - Ash"))
        {
            PlayAsh();
        }

        if (GUI.Button(new Rect(20, 130, 210, 25), "C - Clear Line"))
        {
            PlayClearLine();
        }

        if (GUI.Button(new Rect(20, 160, 210, 25), "B - Bomb"))
        {
            PlayBomb();
        }

        if (GUI.Button(new Rect(20, 190, 210, 25), "I - Invalid Drop"))
        {
            PlayInvalidDrop();
        }

        if (GUI.Button(new Rect(20, 220, 210, 25), "U - Button Click"))
        {
            PlayButtonClick();
        }

        if (GUI.Button(new Rect(20, 260, 210, 25), "N - Play Music"))
        {
            PlayBackgroundMusic();
        }

        if (GUI.Button(new Rect(20, 290, 210, 25), "V - Stop Music"))
        {
            StopBackgroundMusic();
        }
    }
#endif

    private void CreateMusicSource()
    {
        GameObject musicObject = new GameObject("BGM_Source");
        musicObject.transform.SetParent(transform);

        musicSource = musicObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = loopBackgroundMusic;
        musicSource.spatialBlend = 0f;
        musicSource.volume = masterVolume * musicVolume;
    }

    private void CreateAudioSourcePool()
    {
        audioSources.Clear();

        int safePoolSize = Mathf.Max(1, audioSourcePoolSize);

        for (int i = 0; i < safePoolSize; i++)
        {
            GameObject sourceObject = new GameObject($"SFX_Source_{i}");
            sourceObject.transform.SetParent(transform);

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.volume = masterVolume * sfxVolume;

            audioSources.Add(source);
        }
    }

    private void DebugCheckAudioSetup()
    {
        if (!logAudioDebug)
            return;

        AudioListener listener = FindAnyObjectByType<AudioListener>();

        if (listener == null)
        {
            Debug.LogWarning("GameAudioSystem: Không tìm thấy AudioListener. Hãy kiểm tra Main Camera có AudioListener chưa.");
        }

        if (backgroundMusicClip == null)
            Debug.LogWarning("GameAudioSystem: Background Music Clip chưa gán.");

        if (placeBlockClip == null)
            Debug.LogWarning("GameAudioSystem: Place Block Clip chưa gán.");

        if (mixClip == null)
            Debug.LogWarning("GameAudioSystem: Mix Clip chưa gán.");

        if (ashClip == null)
            Debug.LogWarning("GameAudioSystem: Ash Clip chưa gán.");

        if (clearLineClip == null)
            Debug.LogWarning("GameAudioSystem: Clear Line Clip chưa gán.");

        if (bombClip == null)
            Debug.LogWarning("GameAudioSystem: Bomb Clip chưa gán.");

        if (invalidDropClip == null)
            Debug.LogWarning("GameAudioSystem: Invalid Drop Clip chưa gán.");

        if (buttonClickClip == null)
            Debug.LogWarning("GameAudioSystem: Button Click Clip chưa gán.");

        Debug.Log($"GameAudioSystem: Ready. SFX sources = {audioSources.Count}, Master = {masterVolume}, SFX = {sfxVolume}, Music = {musicVolume}");
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusicClip == null)
        {
            if (logAudioDebug)
            {
                Debug.LogWarning("GameAudioSystem: Không phát được nhạc nền vì Background Music Clip chưa được gán.");
            }

            return;
        }

        if (musicSource == null)
        {
            CreateMusicSource();
        }

        musicSource.clip = backgroundMusicClip;
        musicSource.loop = loopBackgroundMusic;
        musicSource.pitch = 1f;

        if (musicFadeRoutine != null)
        {
            StopCoroutine(musicFadeRoutine);
        }

        musicSource.volume = 0f;
        musicSource.Play();

        musicFadeRoutine = StartCoroutine(FadeMusicVolume(0f, masterVolume * musicVolume, musicFadeInDuration));

        if (logAudioDebug)
        {
            Debug.Log($"GameAudioSystem: Playing background music: {backgroundMusicClip.name}");
        }
    }

    public void StopBackgroundMusic()
    {
        if (musicSource == null)
            return;

        if (!musicSource.isPlaying)
            return;

        if (musicFadeRoutine != null)
        {
            StopCoroutine(musicFadeRoutine);
        }

        musicFadeRoutine = StartCoroutine(StopMusicWithFade());
    }

    public void PauseBackgroundMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Pause();
    }

    public void ResumeBackgroundMusic()
    {
        if (musicSource == null)
            return;

        if (musicSource.clip == null)
        {
            PlayBackgroundMusic();
            return;
        }

        musicSource.UnPause();
    }

    private IEnumerator StopMusicWithFade()
    {
        float startVolume = musicSource.volume;
        yield return FadeMusicVolume(startVolume, 0f, musicFadeOutDuration);

        musicSource.Stop();
        musicFadeRoutine = null;
    }

    private IEnumerator FadeMusicVolume(float fromVolume, float toVolume, float duration)
    {
        if (musicSource == null)
            yield break;

        if (duration <= 0f)
        {
            musicSource.volume = toVolume;
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            musicSource.volume = Mathf.Lerp(fromVolume, toVolume, t);

            yield return null;
        }

        musicSource.volume = toVolume;
        musicFadeRoutine = null;
    }

    public void PlayPlaceBlock()
    {
        PlaySfx(placeBlockClip, "Place Block");
    }

    public void PlayMix()
    {
        PlaySfx(mixClip, "Mix");
    }

    public void PlayAsh()
    {
        PlaySfx(ashClip, "Ash");
    }

    public void PlayClearLine()
    {
        PlaySfx(clearLineClip, "Clear Line");
    }

    public void PlayBomb()
    {
        PlaySfx(bombClip, "Bomb");
    }

    public void PlayInvalidDrop()
    {
        PlaySfx(invalidDropClip, "Invalid Drop");
    }

    public void PlayButtonClick()
    {
        PlaySfx(buttonClickClip, "Button Click");
    }

    public void PlaySfx(AudioClip clip)
    {
        PlaySfx(clip, "Unnamed SFX");
    }

    private void PlaySfx(AudioClip clip, string clipName)
    {
        if (clip == null)
        {
            if (logAudioDebug)
            {
                Debug.LogWarning($"GameAudioSystem: Không phát được {clipName} vì AudioClip chưa được gán.");
            }

            return;
        }

        if (audioSources.Count == 0)
        {
            CreateAudioSourcePool();
        }

        AudioSource source = GetNextAudioSource();

        if (source == null)
        {
            Debug.LogWarning("GameAudioSystem: Không có AudioSource để phát âm.");
            return;
        }

        source.pitch = randomizePitch
            ? Random.Range(minPitch, maxPitch)
            : 1f;

        source.volume = masterVolume * sfxVolume;

        source.PlayOneShot(clip);

        if (logAudioDebug)
        {
            Debug.Log($"GameAudioSystem: Playing {clipName} | Clip = {clip.name} | Volume = {source.volume} | Pitch = {source.pitch}");
        }
    }

    private AudioSource GetNextAudioSource()
    {
        if (audioSources.Count == 0)
            return null;

        AudioSource source = audioSources[currentSourceIndex];

        currentSourceIndex++;

        if (currentSourceIndex >= audioSources.Count)
        {
            currentSourceIndex = 0;
        }

        return source;
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);

        RefreshMusicVolume();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);

        RefreshMusicVolume();
    }

    private void RefreshMusicVolume()
    {
        if (musicSource != null)
        {
            musicSource.volume = masterVolume * musicVolume;
        }
    }
}