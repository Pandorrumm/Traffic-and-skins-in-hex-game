using UnityEngine;
using Singleton;
using UnityEngine.Audio;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class SoundsAndMusicController : Singleton<SoundsAndMusicController>
{
    //public AudioClip music;
    //public AudioClip swapSound;
    //public AudioClip pickupSound;
    //public AudioClip placeSound;
    //public AudioClip throwSound;
    //public AudioClip buttonPressed;
    //public AudioClip impossibleAction;
    //[Space]
    //public AudioSource audioSource;

    [Header("KEY SOUNDS")]
    [SerializeField] private string mainMenuMusic = null;
    [SerializeField] private string globalMapMusic = null;
    [SerializeField] private string freeGameMusic = null;
    [SerializeField] private string swapSound = null;
    [SerializeField] private string pickupSound = null;
    [SerializeField] private string placeSound = null;
    [SerializeField] private string throwSound = null;
    [SerializeField] private string buttonPressedSound = null;
    [SerializeField] private string impossibleActionSound = null;
    [SerializeField] private string winSound = null;
    [SerializeField] private string loseSound = null;
    [SerializeField] private string usingBonusSound = null;
    [SerializeField] private string blockButtonSound = null;

    [Space]
    [Space]   
    [SerializeField] private AudioManager audioManagerSFX = null;
    [SerializeField] private AudioManager audioManagerBackground = null;
    [SerializeField] private AudioMixer audioMixer = null;
    [SerializeField] private string toggleMusicObjectName = null;
    [SerializeField] private string toggleSoundObjectName = null;

    private void OnEnable()
    {
        StartPlayBackgroundMusic.PlayBackgroundMusicEvent += BackgroundMusic;
    }

    private void OnDisable()
    {
        StartPlayBackgroundMusic.PlayBackgroundMusicEvent -= BackgroundMusic;
    }

    private void Awake()
    {
        if (HasInstance)
        {
            Destroy(gameObject);
        }
        else
        {
            InitInstance();
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        if (!PlayerPrefs.HasKey("Volume" + toggleMusicObjectName))
        {
            audioMixer.DOSetFloat("BackgroundVolume", 0, 0);
        }
        else if (PlayerPrefs.GetInt("Volume" + toggleMusicObjectName) == 0)
        {
            audioMixer.DOSetFloat("BackgroundVolume", -80, 0);
        }

        if (!PlayerPrefs.HasKey("Volume" + toggleSoundObjectName))
        {
            audioMixer.DOSetFloat("SFXVolume", 0, 0);
        }
        else if (PlayerPrefs.GetInt("Volume" + toggleSoundObjectName) == 0)
        {
            audioMixer.DOSetFloat("SFXVolume", -80, 0);
        }
    }

    private void BackgroundMusic()
    {
        if (GameSettings.Instance.currentGameMode == GameStyle.FreeMode && SceneManager.GetActiveScene().buildIndex == 2)
        {
            audioManagerBackground.PlayAudio(freeGameMusic);
        }
        else if (GameSettings.Instance.currentGameMode == GameStyle.GlobalMapMission && SceneManager.GetActiveScene().buildIndex == 2)
        {
            audioManagerBackground.PlayAudio(globalMapMusic);
        }
        else if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            audioManagerBackground.PlayAudio(mainMenuMusic);
        }
    }

    public void PlaySwap()
    {
        audioManagerSFX.PlayAudio(swapSound);
    }

    public void PlayPickup()
    {
        audioManagerSFX.PlayAudio(pickupSound);
    }

    public void PlayPlace()
    {
        audioManagerSFX.PlayAudio(placeSound);
    }

    public void PlayThrow()
    {
        audioManagerSFX.PlayAudio(throwSound);
    }

    public void PlayButtonPressed()
    {
        audioManagerSFX.PlayAudio(buttonPressedSound);
    }

    public void PlayImpossibleAction()
    {
        audioManagerSFX.PlayAudio(impossibleActionSound);
    }

    public void PlayWin()
    {
        audioManagerSFX.PlayAudio(winSound);
    }

    public void PlayLose()
    {
        audioManagerSFX.PlayAudio(loseSound);
    }

    public void PlayUsingBonus()
    {
        audioManagerSFX.PlayAudio(usingBonusSound);
    }

    public void PlayBlockButton()
    {
        audioManagerSFX.PlayAudio(blockButtonSound);
    }

    public void VolumeOff()
    {
        audioManagerBackground.VolumeOff(0);
    }
}