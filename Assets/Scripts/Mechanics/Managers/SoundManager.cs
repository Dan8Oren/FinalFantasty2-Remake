using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    private const int DEFAULT_PRIORITY = 128;


    private AudioSource _audioSource;

    public static SoundManager Instance { get; private set; }
    public bool IsPlaying { get; private set; }


    private void Start()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);

        Instance = this;
        DontDestroyOnLoad(gameObject);
        _audioSource = GetComponent<AudioSource>();
    }

    public void StopPlaying()
    {
        _audioSource.Stop();
        IsPlaying = false;
    }

    /**
     * Plays the background music by the scene's name.
     */
    public void PlayThemeByScene()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        switch (sceneName)
        {
            case "MainMenu":
                _audioSource.clip = mainMenuMusic;
                _audioSource.volume = mainMenuVolume;
                break;
            case MySceneManager.k_FIGHT:
                var rand = Random.Range(0, battleMusic.Length);
                _audioSource.clip = battleMusic[rand];
                _audioSource.volume = battleVolume;
                break;
            default: //world scene
                if (_audioSource != null && _audioSource.clip == worldSound) //keep playing on world map
                    return;
                _audioSource.clip = worldSound;
                _audioSource.volume = worldVolume;
                break;
        }

        _audioSource.Play();
        IsPlaying = true;
    }

    public void PlayBattleResult(bool isBattleWon)
    {
        IsPlaying = true;
        if (isBattleWon)
        {
            _audioSource.clip = winBattleSound;
            _audioSource.volume = winBattleVolume;
            _audioSource.Play();
            return;
        }

        _audioSource.clip = gameOverSound;
        _audioSource.volume = gameOverVolume;
        _audioSource.Play();
    }

    public void PlayAddHealth(AudioSource audioSource)
    {
        audioSource.clip = addHealthSound;
        audioSource.volume = addHealthVolume;
        audioSource.Play();
    }

    public void PlayDeath(AudioSource audioSource)
    {
        audioSource.clip = deathSound;
        audioSource.volume = deathVolume;
        audioSource.Play();
    }

    public void PlayEndGame()
    {
        IsPlaying = true;
        _audioSource.clip = winGameSound;
        _audioSource.volume = winGameVolume;
        _audioSource.Play();
    }

    public void PlayHit(AudioSource audioSource)
    {
        audioSource.clip = damageSound;
        audioSource.volume = damageVolume;
        audioSource.Play();
    }

    public void PlayGoodSequenceClick(AudioSource audioSource)
    {
        audioSource.clip = goodSequenceClickSound;
        audioSource.volume = goodSequenceClickVolume;
        audioSource.Play();
    }

    public void PlayJoinedTheTeam()
    {
        _audioSource.clip = joinedTheTeamSound;
        _audioSource.volume = joinedTheTeamVolume;
        _audioSource.Play();
    }

    public void PlayBeforeFight()
    {
        IsPlaying = true;
        _audioSource.clip = beforeFightSound;
        _audioSource.volume = beforeFightVolume;
        _audioSource.Play();
    }

    #region AudioClip

    [SerializeField] private AudioClip mainMenuMusic;

    [Range(0, 1)] [SerializeField] private float mainMenuVolume;

    [SerializeField] private AudioClip worldSound;

    [Range(0, 1)] [SerializeField] private float worldVolume;

    [SerializeField] private AudioClip[] battleMusic;

    [Range(0, 1)] [SerializeField] private float battleVolume;

    [SerializeField] private AudioClip winGameSound;

    [Range(0, 1)] [SerializeField] private float winGameVolume;

    [SerializeField] private AudioClip gameOverSound;

    [Range(0, 1)] [SerializeField] private float gameOverVolume;

    [SerializeField] private AudioClip beforeFightSound;

    [Range(0, 1)] [SerializeField] private float beforeFightVolume;

    [SerializeField] private AudioClip winBattleSound;

    [Range(0, 1)] [SerializeField] private float winBattleVolume;

    [SerializeField] private AudioClip damageSound;

    [Range(0, 1)] [SerializeField] private float damageVolume;

    [SerializeField] private AudioClip addHealthSound;

    [Range(0, 1)] [SerializeField] private float addHealthVolume;

    [SerializeField] private AudioClip deathSound;

    [Range(0, 1)] [SerializeField] private float deathVolume;

    [SerializeField] private AudioClip joinedTheTeamSound;

    [Range(0, 1)] [SerializeField] private float joinedTheTeamVolume;

    [SerializeField] private AudioClip goodSequenceClickSound;

    [Range(0, 1)] [SerializeField] private float goodSequenceClickVolume;

    #endregion
}