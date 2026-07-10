using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource eventSource; 

    [Header("Audio Clips")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip placeMarkSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;
    [SerializeField] private AudioClip drawSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main Menu Scene")
        {
            PlayMusic(mainMenuMusic);
        }
        else if (scene.name == "GameScene" || scene.name == "Game Scene")
        {
            PlayMusic(gameMusic);
        }
        
        HookAllButtons();
    }

    private void HookAllButtons()
    {
        UnityEngine.UI.Button[] buttons = Resources.FindObjectsOfTypeAll<UnityEngine.UI.Button>();
        foreach (var btn in buttons)
        {
            if (btn.gameObject.scene.isLoaded)
            {
                btn.onClick.AddListener(PlayButtonClick);
            }
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }

    public void PlayPlaceMark()
    {
        PlaySFX(placeMarkSound);
    }

    public void PlayWinSound()
    {
        if (winSound != null && eventSource != null) eventSource.PlayOneShot(winSound);
    }

    public void PlayLoseSound()
    {
        if (loseSound != null && eventSource != null) eventSource.PlayOneShot(loseSound);
    }

    public void PlayDrawSound()
    {
        if (drawSound != null && eventSource != null) eventSource.PlayOneShot(drawSound);
    }
}
