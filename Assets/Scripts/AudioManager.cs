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

    /// <summary>
    /// Loads all level audio files directly from resources.
    /// </summary>
    private void LoadAudioLevels()
    {
        _panelPush = new AudioClip[3];
        _panelPush[0] = Resources.Load<AudioClip>("SFX/Push1");
        _panelPush[1] = Resources.Load<AudioClip>("SFX/Push2");
        _panelPush[2] = Resources.Load<AudioClip>("SFX/Push3");
    }

    public void PlayPush()
    {
        _source.PlayOneShot(_panelPush[Random.Range(0, 3)], GameManager.Instance.GetSfxVolume());
    }
    #endregion


}
