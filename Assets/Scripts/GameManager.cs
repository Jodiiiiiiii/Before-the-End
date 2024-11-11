using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Unity.Mathematics;

/// <summary>
/// Stores and manages player data saved between scenes.
/// Stores and manages save data saved between sessions.
/// </summary>
public class GameManager : MonoBehaviour
{
    // private singleton instance
    private static GameManager _instance;

    // public accessor of instance
    public static GameManager Instance
    {
        get
        {
            // setup GameManager as a singleton class
            if (_instance == null)
            {
                // create new game manager object
                GameObject newManager = new();
                newManager.name = "Game Manager";
                newManager.AddComponent<GameManager>();
                DontDestroyOnLoad(newManager);
                _instance = newManager.GetComponent<GameManager>();

                // ensures controls are updated with player overrides
                // loaded here so it always happens at the start and not after rebindings are needed
                string rebindsJson = PlayerPrefs.GetString("rebinds");
                InputSystem.actions.LoadBindingOverridesFromJson(rebindsJson);
            }
            // return new/existing instance
            return _instance;
        }
    }

    #region SINGLE SCENE DATA
    public bool IsPaused = false;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Resets Single Scene Data, happens at the start of each new scene.
    /// For example, ensures that pause state is always false at the start of a new scene
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        IsPaused = false;
    }
    #endregion

    #region SAVE DATA
    // save data (saved between sessions)
    [System.Serializable]
    public class PersistentData
    {
        public int MasterVolume;    // 0 to 200
        public int MusicVolume;     // 0 to 200
        public int SfxVolume;       // 0 to 200

        // TODO: add level progression data here
    }

    // private stored save data
    private PersistentData _saveData;

    // public accessor for save data
    public PersistentData SaveData
    {
        get
        {
            // initialize if necessary and possible
            if (_saveData == null)
            {
                InitializeSaveData();
            }

            return _saveData;
        }
        private set
        {
            _saveData = value;
        }
    }

    /// <summary>
    /// initializes base stats of save data (used for first time playing).
    /// Used both for reading existing save data AND for creating new save data if none is found.
    /// </summary>
    private void InitializeSaveData()
    {
        // initialize and load save data
        PersistentData newSaveData = new PersistentData();
        
        // Read save data from PlayerPrefs (or assign default values)
        newSaveData.MasterVolume = PlayerPrefs.GetInt("masterVolume", 100);
        newSaveData.MusicVolume = PlayerPrefs.GetInt("musicVolume", 100);
        newSaveData.SfxVolume = PlayerPrefs.GetInt("sfxVolume", 100);

        // TODO: add level progression loading here

        /*****************************************************************
        // JSON functionality. To be replaced with PlayerPrefs

        string path = Application.persistentDataPath + "\\savedata.json";
        if (File.Exists(path))
        {
            // read json file into data object
            string json = File.ReadAllText(path);
            newSaveData = JsonUtility.FromJson<PersistentData>(json);
        }
        *****************************************************************/

        // Apply read/initialized data to instance
        Instance.SaveData = newSaveData;
    }

    private void OnApplicationQuit()
    {
        // save controls rebindings
        string rebindsJson = InputSystem.actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebindsJson);

        // Store save data in PlayerPrefs
        PlayerPrefs.SetInt("masterVolume", SaveData.MasterVolume);
        PlayerPrefs.SetInt("musicVolume", SaveData.MusicVolume);
        PlayerPrefs.SetInt("sfxVolume", SaveData.SfxVolume);

        // TODO: add level progression saving here

        PlayerPrefs.Save();

        /*****************************************************************
        // JSON functionality. To be replaced with PlayerPrefs

        string json = JsonUtility.ToJson(SaveData);
        File.WriteAllText(Application.persistentDataPath + "\\savedata.json", json);
        *****************************************************************/
    }

    #region Save Data Getters/Setters
    /// <summary>
    /// Gets useable master volume value for audio sources.
    /// Converts from 0 to 200, to 0 to 1
    /// </summary>
    public float GetMasterVolume()
    {
        return math.remap(0, 200, 0, 1, SaveData.MasterVolume);
    }

    /// <summary>
    /// Gets useable music volume value for audio sources.
    /// Converts from 0 to 200, to 0 to 1
    /// </summary>
    public float GetMusicVolume()
    {
        return math.remap(0, 200, 0, 1, SaveData.MusicVolume);
    }

    /// <summary>
    /// Gets useable sfx volume value for audio sources.
    /// Converts from 0 to 200, to 0 to 1
    /// </summary>
    public float GetSfxVolume()
    {
        return math.remap(0, 200, 0, 1, SaveData.SfxVolume);
    }
    #endregion

    #endregion
}