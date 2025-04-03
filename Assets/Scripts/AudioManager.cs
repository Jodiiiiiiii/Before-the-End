using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

                // add audio source
                newManager.AddComponent<AudioSource>();
                _instance._source = newManager.GetComponent<AudioSource>();

                // ensure all audio files are loaded from resources
                _instance.LoadAudioUI();
                _instance.LoadAudioLevelSelect();
                _instance.LoadAudioLevels();
            }
            // return new/existing instance
            return _instance;
        }
    }

    private AudioSource _source;

    #region User Interface
    /// <summary>
    /// Loads all UI audio files directly from resources.
    /// </summary>
    private void LoadAudioUI()
    {

    }
    #endregion

    #region Level Select
    private AudioClip[] _levelSelectSteps;

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
    }

    public void PlayMove()
    {
        _source.PlayOneShot(_levelSelectSteps[Random.Range(0, 4)], GameManager.Instance.GetSfxVolume());
    }
    #endregion

    #region Levels
    private AudioClip[] _panelPush;
    private AudioClip[] _logPush;
    private AudioClip[] _swapDino;
    private AudioClip[] _swim;
    private AudioClip _objSplash;

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

        _objSplash = Resources.Load<AudioClip>("SFX/ObjectSplash");

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
        _source.PlayOneShot(_panelPush[Random.Range(0, 3)], GameManager.Instance.GetSfxVolume());
    }

    public void PlayPushLog()
    {
        _source.PlayOneShot(_logPush[Random.Range(0, 3)], GameManager.Instance.GetSfxVolume());
    }

    public void PlaySwap()
    {
        _source.PlayOneShot(_swapDino[Random.Range(0, 3)], GameManager.Instance.GetSfxVolume());
    }

    public void PlaySwim()
    {
        _source.PlayOneShot(_swim[Random.Range(0, 4)], GameManager.Instance.GetSfxVolume());
    }

    public void PlayObjectSplash()
    {
        _source.PlayOneShot(_objSplash, GameManager.Instance.GetSfxVolume());
    }

    public void PlayStegoAbility()
    {
        _source.PlayOneShot(_stegoAbility, GameManager.Instance.GetSfxVolume());
    }

    public void PlayAnkyAbility()
    {
        _source.PlayOneShot(_ankyAbility, GameManager.Instance.GetSfxVolume());
    }

    public void PlayTrikeCrush()
    {
        _source.PlayOneShot(_trikeCrush, GameManager.Instance.GetSfxVolume());
    }

    public void PlayTrikePush()
    {
        _source.PlayOneShot(_trikePush, GameManager.Instance.GetSfxVolume());
    }

    public void PlaySpinoEnter()
    {
        _source.PlayOneShot(_spinoEnter, GameManager.Instance.GetSfxVolume());
    }

    public void PlaySpinoExit()
    {
        _source.PlayOneShot(_spinoExit, GameManager.Instance.GetSfxVolume());
    }

    public void PlayPteraAbility()
    {
        _source.PlayOneShot(_pteraAbility, GameManager.Instance.GetSfxVolume());
    }

    public void PlayPyroAbility()
    {
        _source.PlayOneShot(_pyroAbility, GameManager.Instance.GetSfxVolume());
    }

    public void PlayCompyAbility()
    {
        _source.PlayOneShot(_compyAbility, GameManager.Instance.GetSfxVolume());
    }

    public void PlayCompySwap()
    {
        _source.PlayOneShot(_compySwap, GameManager.Instance.GetSfxVolume());
    }
    #endregion


}
