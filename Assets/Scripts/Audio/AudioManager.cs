using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Allows for static calls for all sound play calls
/// </summary>
public class AudioManager : MonoBehaviour
{
    // private singleton instance
    private static AudioManager _instance;

    // public accessor of instance
    public static AudioManager Instance
    {
        get
        {
            // setup GameManager as a singleton class
            if (_instance == null)
            {
                // create new game manager object
                GameObject newManager = new();
                newManager.name = "[Audio Manager]";
                DontDestroyOnLoad(newManager);
                newManager.AddComponent<AudioManager>();
                _instance = newManager.GetComponent<AudioManager>();

                // add audio source - Music
                _instance._musicSource = newManager.AddComponent<AudioSource>();
                _instance._musicSource.volume = GameManager.Instance.GetMusicVolume();
                _instance._musicSource.loop = true;
                // add audio source - SFX
                _instance._sfxSource = newManager.AddComponent<AudioSource>();
                // add audio source - ambient level fire
                _instance._levelFireSource = newManager.AddComponent<AudioSource>();
                _instance._levelFireSource.loop = true;

                // load music files
                _instance.LoadMusic();
                // ensure all audio files are loaded from resources
                _instance.LoadAudioLevelSelect();
                _instance.LoadAudioLevels();
            }
            // return new/existing instance
            return _instance;
        }
    }

    private AudioSource _musicSource;
    private AudioSource _sfxSource;
    private AudioSource _levelFireSource;

    #region Music
    private AudioClip _startMusic;

    private AudioClip _levelMusic1;
    private AudioClip _levelMusic2;

    private AudioClip _plainsAmbient;
    private AudioClip _mountainsAmbient;
    private AudioClip _forestAmbient;
    private AudioClip _swampAmbient;
    private AudioClip _lakeAmbient;
    private AudioClip _valleyAmbient;
    private AudioClip _beachAmbient;
    private AudioClip _fireAmbient;

    private void LoadMusic()
    {
        _startMusic = Resources.Load<AudioClip>("Music/StartTrack");

        _levelMusic1 = Resources.Load<AudioClip>("Music/LevelTrack1");
        _levelMusic2 = Resources.Load<AudioClip>("Music/LevelTrack2");

        _plainsAmbient = Resources.Load<AudioClip>("Music/PlainsAmbient");
        _mountainsAmbient = Resources.Load<AudioClip>("Music/MountainAmbient");
        _forestAmbient = Resources.Load<AudioClip>("Music/ForestAmbient");
        _swampAmbient = Resources.Load<AudioClip>("Music/SwampAmbient");
        _lakeAmbient = Resources.Load<AudioClip>("Music/LakeAmbient");
        _valleyAmbient = Resources.Load<AudioClip>("Music/ValleyAmbient");
        _beachAmbient = Resources.Load<AudioClip>("Music/BeachAmbient");
        _fireAmbient = Resources.Load<AudioClip>("Music/FireAmbient");

        // the only clip this audio source plays
        _levelFireSource.clip = _fireAmbient;
    }

    /// <summary>
    /// Queues new track, but does NOTHING if that track is already playing
    /// </summary>
    private void QueueTrack(AudioClip track)
    {
        if (_currTrack != track)
            _queueTrack = track;
    }

    public void UnqueueMusic()
    {
        // stop music and clear queue
        _currTrack = null;
        _queueTrack = null;
    }

    public void QueueStartMusic()
    {
        QueueTrack(_startMusic);
    }

    public void QueueLevelMusic1()
    {
        QueueTrack(_levelMusic1);
    }

    public void QueueLevelMusic2()
    {
        QueueTrack(_levelMusic2);
    }

    public void QueuePlainsAmbient()
    {
        QueueTrack(_plainsAmbient);
    }

    public void QueueMountainsAmbient()
    {
        QueueTrack(_mountainsAmbient);
    }

    public void QueueForestAmbient()
    {
        QueueTrack(_forestAmbient);
    }

    public void QueueSwampAmbient()
    {
        QueueTrack(_swampAmbient);
    }

    public void QueueLakeAmbient()
    {
        QueueTrack(_lakeAmbient);
    }

    public void QueueValleyAmbient()
    {
        QueueTrack(_valleyAmbient);
    }

    public void QueueBeachAmbient()
    {
        QueueTrack(_beachAmbient);
    }

    public void QueueFireAmbient()
    {
        QueueTrack(_fireAmbient);
    }

    private const float VOLUME_CHANGE_RATE = 1.25f;  // rate at which volume fades & increases back when switching track

    // for queuing track change and managing current music / ambient track
    private AudioClip _queueTrack = null;
    private AudioClip _currTrack = null;

    private bool _isAmbientFireLevel = false;

    private void Update()
    {
        // transition out to new queued track
        if (_queueTrack is not null)
        {
            // slowly decrement volume down
            _musicSource.volume -= VOLUME_CHANGE_RATE * Time.deltaTime;

            if (_musicSource.volume <= 0)
            {
                // switch track
                _musicSource.volume = 0;
                _musicSource.clip = _queueTrack;
                _musicSource.Play(); // start playing new looping track

                // clear queue
                _currTrack = _queueTrack;
                _queueTrack = null;
            }
        }
        // standard track looping behavior
        else if (_currTrack is not null)
        {
            // move towards 25% configured volume during pause
            if (GameManager.Instance.IsPaused && SceneManager.GetActiveScene().name != "MainMenu")
            {
                if (_musicSource.volume > GameManager.Instance.GetMusicVolume() / 4f)
                {
                    _musicSource.volume -= VOLUME_CHANGE_RATE * Time.deltaTime;
                    if (_musicSource.volume < GameManager.Instance.GetMusicVolume() / 4f)
                        _musicSource.volume = GameManager.Instance.GetMusicVolume() / 4f;
                }
                // increasing to 25% should only occur on instant pause upon loading new track
                else
                {
                    _musicSource.volume += VOLUME_CHANGE_RATE * Time.deltaTime;
                    if (_musicSource.volume > GameManager.Instance.GetMusicVolume() / 4f)
                        _musicSource.volume = GameManager.Instance.GetMusicVolume() / 4f;
                }
            }
            // ensure at full configuredvolume when NOT paused
            else
            {
                // ensure at full volume
                _musicSource.volume += VOLUME_CHANGE_RATE * Time.deltaTime;
                if (_musicSource.volume > GameManager.Instance.GetMusicVolume())
                    _musicSource.volume = GameManager.Instance.GetMusicVolume();
            }
        }
        // no track to play - or music was stopped
        else
        {
            if (_musicSource.volume > 0)
            {
                // decrease volume to zero
                _musicSource.volume -= VOLUME_CHANGE_RATE * Time.deltaTime;
                if (_musicSource.volume < 0f)
                {
                    _musicSource.volume = 0f;
                    // also stop track since it has finished fading out
                    _musicSource.Stop();
                }
            }
        }

        // AMBIENT LEVEL FIRE ------------------------------------
        // fire level
        if (_isAmbientFireLevel)
        {
            // fade out ambient level fire when paused
            if (GameManager.Instance.IsPaused && SceneManager.GetActiveScene().name != "MainMenu")
            {
                // ensure at full volume (full volume for ambient level fire is HALF the music volume)
                _levelFireSource.volume -= VOLUME_CHANGE_RATE * Time.deltaTime;
                if (_levelFireSource.volume <= 0f)
                    _levelFireSource.volume = 0f;
            }
            // standard behavior
            else
            {
                // ensure at full volume (full volume for ambient level fire is HALF the music volume)
                _levelFireSource.volume += VOLUME_CHANGE_RATE * Time.deltaTime;
                if (_levelFireSource.volume > GameManager.Instance.GetMusicVolume() / 2f)
                    _levelFireSource.volume = GameManager.Instance.GetMusicVolume() / 2f;
            }
        }
        // not fire level
        else
        {
            // decrease to no volume, then stop
            if (_levelFireSource.volume > 0)    // only handles decrease logic if it has not yet been set to 0
            {
                _levelFireSource.volume -= VOLUME_CHANGE_RATE * Time.deltaTime;
                if (_levelFireSource.volume <= 0)
                {
                    _levelFireSource.volume = 0f;
                    _levelFireSource.Stop();
                }
            }
        }
    }

    /// <summary>
    /// activates audio source for level ambient fire
    /// </summary>
    public void EnableAmbientLevelFire()
    {
        _isAmbientFireLevel = true;

        // starts playing ambient - most likely at 0 volume
        if (!_levelFireSource.isPlaying)            // ensure clip is not restarted if already playing
            _levelFireSource.Play();
    }

    /// <summary>
    /// de-activates audio source for level ambient fire
    /// </summary>
    public void DisableAmbientLevelFire()
    {
        _isAmbientFireLevel = false;
    }
    #endregion

    #region Misc. (Level Select, UI, Cutscenes)
    private AudioClip[] _levelSelectSteps;

    private AudioClip _levelEnter;
    private AudioClip _levelExit;

    private AudioClip _clickUI;
    private AudioClip _changeSliderUI;

    private AudioClip _visionStart;
    private AudioClip _rumble;
    private AudioClip _asteroid;
    private AudioClip _breeze;
    private AudioClip _cutscenePause;

    /// <summary>
    /// Loads all level select audio files directly from resources.
    /// </summary>
    private void LoadAudioLevelSelect()
    {
        _levelSelectSteps = new AudioClip[4];
        _levelSelectSteps[0] = Resources.Load<AudioClip>("SFX/Step1");
        _levelSelectSteps[1] = Resources.Load<AudioClip>("SFX/Step2");
        _levelSelectSteps[2] = Resources.Load<AudioClip>("SFX/Step3");
        _levelSelectSteps[3] = Resources.Load<AudioClip>("SFX/Step4");

        _levelEnter = Resources.Load<AudioClip>("SFX/LevelEnter");
        _levelExit = Resources.Load<AudioClip>("SFX/LevelExit");

        _clickUI = Resources.Load<AudioClip>("SFX/ClickUI");
        _changeSliderUI = Resources.Load<AudioClip>("SFX/ChangeSliderUI");

        _visionStart = Resources.Load<AudioClip>("Cutscene/Vision");
        _rumble = Resources.Load<AudioClip>("Cutscene/Rumble");
        _asteroid = Resources.Load<AudioClip>("Cutscene/Asteroid");
        _breeze = Resources.Load<AudioClip>("Cutscene/Breeze");
        _cutscenePause = Resources.Load<AudioClip>("Cutscene/RewindPause");
    }

    public void PlayMove()
    {
        _sfxSource.PlayOneShot(_levelSelectSteps[Random.Range(0, 4)], GameManager.Instance.GetSfxVolume());
    }

    public void PlayLevelEnter()
    {
        _sfxSource.PlayOneShot(_levelEnter, GameManager.Instance.GetSfxVolume());
    }

    public void PlayLevelExit()
    {
        _sfxSource.PlayOneShot(_levelExit, GameManager.Instance.GetSfxVolume());
    }

    public void PlayClickUI()
    {
        _sfxSource.PlayOneShot(_clickUI, GameManager.Instance.GetSfxVolume());
    }

    public void PlayChangeSliderUI()
    {
        // avoid rapid clicking of slider UI
        if (!_sfxSource.isPlaying)
            _sfxSource.PlayOneShot(_changeSliderUI, GameManager.Instance.GetSfxVolume());
    }

    public void PlayVisionStart()
    {
        _sfxSource.PlayOneShot(_visionStart, GameManager.Instance.GetSfxVolume());
    }

    public void PlayRumble()
    {
        _sfxSource.PlayOneShot(_rumble, GameManager.Instance.GetSfxVolume());
    }

    public void PlayAsteroidImpact()
    {
        _sfxSource.PlayOneShot(_asteroid, GameManager.Instance.GetSfxVolume());
    }

    public void PlayBreeze()
    {
        _sfxSource.PlayOneShot(_breeze, GameManager.Instance.GetSfxVolume());
    }

    public void PlayCutscenePause()
    {
        _sfxSource.PlayOneShot(_cutscenePause, GameManager.Instance.GetSfxVolume());
    }
    #endregion

    #region Levels
    private AudioClip[] _panelPush;
    private AudioClip[] _logPush;
    private AudioClip[] _swapDino;
    private AudioClip[] _swim;

    private AudioClip _moveFail;
    private AudioClip _rewind;
    private AudioClip _objSplash;
    private AudioClip _objSink;
    private AudioClip _fireIgnite;
    private AudioClip _fireSpread;
    private AudioClip _fireExtinguish;
    private AudioClip _levelComplete;

    private AudioClip _abilityFail;
    private AudioClip _stegoAbility;
    private AudioClip _ankyAbility;
    private AudioClip _trikeCrush;
    private AudioClip _trikePush;
    private AudioClip _spinoEnter;
    private AudioClip _spinoExit;
    private AudioClip _pteraAbility;
    private AudioClip _pyroAbility;
    private AudioClip _compyAbility;
    private AudioClip _compySwap;

    /// <summary>
    /// Loads all level audio files directly from resources.
    /// </summary>
    private void LoadAudioLevels()
    {
        _panelPush = new AudioClip[3];
        _panelPush[0] = Resources.Load<AudioClip>("SFX/Push1");
        _panelPush[1] = Resources.Load<AudioClip>("SFX/Push2");
        _panelPush[2] = Resources.Load<AudioClip>("SFX/Push3");

        _logPush = new AudioClip[3];
        _logPush[0] = Resources.Load<AudioClip>("SFX/LogPush1");
        _logPush[1] = Resources.Load<AudioClip>("SFX/LogPush2");
        _logPush[2] = Resources.Load<AudioClip>("SFX/LogPush3");

        _swapDino = new AudioClip[3];
        _swapDino[0] = Resources.Load<AudioClip>("SFX/Swap1");
        _swapDino[1] = Resources.Load<AudioClip>("SFX/Swap2");
        _swapDino[2] = Resources.Load<AudioClip>("SFX/Swap3");

        _swim = new AudioClip[4];
        _swim[0] = Resources.Load<AudioClip>("SFX/Swim1");
        _swim[1] = Resources.Load<AudioClip>("SFX/Swim2");
        _swim[2] = Resources.Load<AudioClip>("SFX/Swim3");
        _swim[3] = Resources.Load<AudioClip>("SFX/Swim4");

        _moveFail = Resources.Load<AudioClip>("SFX/MoveFail");
        _rewind = Resources.Load<AudioClip>("SFX/Rewind");
        _objSplash = Resources.Load<AudioClip>("SFX/ObjectSplash");
        _objSink = Resources.Load<AudioClip>("SFX/ObjectSink");
        _fireIgnite = Resources.Load<AudioClip>("SFX/FireIgnite");
        _fireSpread = Resources.Load<AudioClip>("SFX/FireSpread");
        _fireExtinguish = Resources.Load<AudioClip>("SFX/FireExtinguish");
        _levelComplete = Resources.Load<AudioClip>("SFX/LevelComplete");

        _abilityFail = Resources.Load<AudioClip>("SFX/AbilityFail");
        _stegoAbility = Resources.Load<AudioClip>("SFX/Stego");
        _ankyAbility = Resources.Load<AudioClip>("SFX/Anky");
        _trikeCrush = Resources.Load<AudioClip>("SFX/LogCrush");
        _trikePush = Resources.Load<AudioClip>("SFX/RockPush");
        _spinoEnter = Resources.Load<AudioClip>("SFX/SpinoEnter");
        _spinoExit = Resources.Load<AudioClip>("SFX/SpinoExit");
        _pteraAbility = Resources.Load<AudioClip>("SFX/Ptera");
        _pyroAbility = Resources.Load<AudioClip>("SFX/Pyro");
        _compyAbility = Resources.Load<AudioClip>("SFX/Compy");
        _compySwap = Resources.Load<AudioClip>("SFX/CompyShort");
    }

    public void PlayPushPanel()
    {
        _sfxSource.PlayOneShot(_panelPush[Random.Range(0, 3)], GameManager.Instance.GetSfxVolume());
    }

    public void PlayPushLog()
    {
        _sfxSource.PlayOneShot(_logPush[Random.Range(0, 3)], GameManager.Instance.GetSfxVolume());
    }

    public void PlaySwap()
    {
        _sfxSource.PlayOneShot(_swapDino[Random.Range(0, 3)], GameManager.Instance.GetSfxVolume());
    }

    public void PlaySwim()
    {
        _sfxSource.PlayOneShot(_swim[Random.Range(0, 4)], GameManager.Instance.GetSfxVolume());
    }

    public void PlayMoveFail()
    {
        _sfxSource.PlayOneShot(_moveFail, GameManager.Instance.GetSfxVolume());
    }

    public void PlayRewind()
    {
        _sfxSource.PlayOneShot(_rewind, GameManager.Instance.GetSfxVolume());
    }

    public void PlayObjectSplash()
    {
        _sfxSource.PlayOneShot(_objSplash, GameManager.Instance.GetSfxVolume());
    }

    public void PlayObjectSink()
    {
        _sfxSource.PlayOneShot(_objSink, GameManager.Instance.GetSfxVolume());
    }

    public void PlayFireIgnite()
    {
        _sfxSource.PlayOneShot(_fireIgnite, GameManager.Instance.GetSfxVolume());
    }

    public void PlayFireSpread()
    {
        _sfxSource.PlayOneShot(_fireSpread, GameManager.Instance.GetSfxVolume());
    }

    public void PlayFireExtinguish()
    {
        _sfxSource.PlayOneShot(_fireExtinguish, GameManager.Instance.GetSfxVolume());
    }

    public void PlayBushBurn()
    {
        // bush burn should be quieter than extinguish used with rock push
        _sfxSource.PlayOneShot(_fireExtinguish, GameManager.Instance.GetSfxVolume() / 2f);
    }

    public void PlayLevelComplete()
    {
        _sfxSource.PlayOneShot(_levelComplete, GameManager.Instance.GetSfxVolume());
    }

    public void PlayAbilityFail()
    {
        _sfxSource.PlayOneShot(_abilityFail, GameManager.Instance.GetSfxVolume());
    }

    public void PlayStegoAbility()
    {
        _sfxSource.PlayOneShot(_stegoAbility, GameManager.Instance.GetSfxVolume());
    }

    public void PlayAnkyAbility()
    {
        _sfxSource.PlayOneShot(_ankyAbility, GameManager.Instance.GetSfxVolume());
    }

    public void PlayTrikeCrush()
    {
        _sfxSource.PlayOneShot(_trikeCrush, GameManager.Instance.GetSfxVolume());
    }

    public void PlayTrikePush()
    {
        _sfxSource.PlayOneShot(_trikePush, GameManager.Instance.GetSfxVolume());
    }

    public void PlaySpinoEnter()
    {
        _sfxSource.PlayOneShot(_spinoEnter, GameManager.Instance.GetSfxVolume());
    }

    public void PlaySpinoExit()
    {
        _sfxSource.PlayOneShot(_spinoExit, GameManager.Instance.GetSfxVolume());
    }

    public void PlayPteraAbility()
    {
        _sfxSource.PlayOneShot(_pteraAbility, GameManager.Instance.GetSfxVolume());
    }

    public void PlayPyroAbility()
    {
        _sfxSource.PlayOneShot(_pyroAbility, GameManager.Instance.GetSfxVolume());
    }

    public void PlayCompyAbility()
    {
        _sfxSource.PlayOneShot(_compyAbility, GameManager.Instance.GetSfxVolume());
    }

    public void PlayCompySwap()
    {
        _sfxSource.PlayOneShot(_compySwap, GameManager.Instance.GetSfxVolume());
    }
    #endregion

}
