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
                newManager.AddComponent<AudioManager>();
                DontDestroyOnLoad(newManager);
                _instance = newManager.GetComponent<AudioManager>();

                // ensure all audio files are loaded from resources
                _instance.LoadAudioUI();
                _instance.LoadAudioLevelSelect();
                _instance.LoadAudioLevels();
            }
            // return new/existing instance
            return _instance;
        }
    }

    #region User Interface
    /// <summary>
    /// Loads all UI audio files directly from resources.
    /// </summary>
    private void LoadAudioUI()
    {

    }
    #endregion

    #region Level Select
    /// <summary>
    /// Loads all level select audio files directly from resources.
    /// </summary>
    private void LoadAudioLevelSelect()
    {

    }
    #endregion

    #region Levels
    /// <summary>
    /// Loads all level audio files directly from resources.
    /// </summary>
    private void LoadAudioLevels()
    {

    }
    #endregion


}
