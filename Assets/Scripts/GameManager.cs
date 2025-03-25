using System.Collections;
using System.Collections.Generic;
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

                // ensure default values are loaded/read on start NO MATTER WHAT
                // important for editor since the actual json data is never read in a level scene which interferes with playerPref initialization
                _instance.InitializeSaveData();
            }
            // return new/existing instance
            return _instance;
        }
    }

    #region SINGLE SCENE DATA
    public bool IsPaused = false;
    public bool IsFading = false;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Resets Single Scene Data, happens at the start of each new scene.
    /// For example, ensures that pause state is always false at the start of a new scene
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        IsPaused = false;
        IsFading = false;
    }
    #endregion

    #region SAVE DATA
    // Options data
    // stored separately because it uses PlayerPrefs and does not interface through json at all which the entire PersistentData class does
    public int MasterVolume;    // 0 to 200
    public int MusicVolume;     // 0 to 200
    public int SfxVolume;       // 0 to 200

    // save data (saved between sessions)
    [System.Serializable]
    public class PersistentData
    {
        // Progression data - saved via .json file

        public bool NewGameStarted;

        public List<string> LevelsComplete;

        public bool isSecondTimeline;
        public string CurrLevel;

        public List<string> HelpUnlocks;
        public bool HelpNotif;
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
        ResetProgressionData();

        // initialize and load save data
        PersistentData newSaveData = Instance.SaveData; // retrieves default initialization data
        
        // Read save data from PlayerPrefs (or assign default values)
        MasterVolume = PlayerPrefs.GetInt("masterVolume", 100);
        MusicVolume = PlayerPrefs.GetInt("musicVolume", 100);
        SfxVolume = PlayerPrefs.GetInt("sfxVolume", 100);

        // Read progression data
        string path = Application.persistentDataPath + "\\ProgressionData.json";
        if (File.Exists(path))
        {
            // read json file into data object
            string json = File.ReadAllText(path);
            newSaveData = JsonUtility.FromJson<PersistentData>(json);
        }

        // Apply read/initialized data to instance
        Instance.SaveData = newSaveData;
    }

    /// <summary>
    /// Clears progression data saved in ProgressionData.json.
    /// Also used when initializing data before reading from .json.
    /// </summary>
    public void ResetProgressionData()
    {
        PersistentData newSaveData = new PersistentData();

        // default progression data 
        // new save data
        newSaveData.NewGameStarted = false;

        // level complete
        newSaveData.LevelsComplete = new List<string>(); // empty by default

        // tracking current level
        newSaveData.CurrLevel = "Tut0"; // first level
        newSaveData.isSecondTimeline = false; // timeline 1

        // help unlocks for help book
        newSaveData.HelpUnlocks = new List<string>(); // empty by default
        newSaveData.HelpUnlocks.Add("Move"); // first level unlock
        newSaveData.HelpNotif = true; // move panel is unlocked by default

        Instance.SaveData = newSaveData;
    }

    private void OnApplicationQuit()
    {
        // OPTIONS DATA - PlayerPrefs
        // save controls rebindings
        string rebindsJson = InputSystem.actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebindsJson);

        // Store save data in PlayerPrefs
        PlayerPrefs.SetInt("masterVolume", MasterVolume);
        PlayerPrefs.SetInt("musicVolume", MusicVolume);
        PlayerPrefs.SetInt("sfxVolume", SfxVolume);

        PlayerPrefs.Save();

        // PROGRESSION DATA - .json file
        string json = JsonUtility.ToJson(SaveData);
        File.WriteAllText(Application.persistentDataPath + "\\ProgressionData.json", json);
    }

    #region Save Data Getters/Setters
    /// <summary>
    /// Gets useable master volume value for audio sources.
    /// Converts from 0 to 200, to 0 to 1
    /// </summary>
    public float GetMasterVolume()
    {
        return math.remap(0, 200, 0, 1, MasterVolume);
    }

    /// <summary>
    /// Gets useable music volume value for audio sources.
    /// Converts from 0 to 200, to 0 to 1
    /// </summary>
    public float GetMusicVolume()
    {
        return math.remap(0, 200, 0, 1, MusicVolume) * GetMasterVolume();
    }

    /// <summary>
    /// Gets useable sfx volume value for audio sources.
    /// Converts from 0 to 200, to 0 to 1
    /// </summary>
    public float GetSfxVolume()
    {
        return math.remap(0, 200, 0, 1, SfxVolume) * GetMasterVolume();
    }

    /// <summary>
    /// Marks the currently selected level as complete for progression.
    /// </summary>
    public void LevelComplete()
    {
        // avoid editor case of saving invalid level name assignment to save data (but prevent crash in editor)
        if (SaveData.CurrLevel is null)
            return;

        // only add level to complete levels if it has not already been completed
        string levelIdentifier = (SaveData.isSecondTimeline ? "2-" : "1-") + SaveData.CurrLevel;
        if (!SaveData.LevelsComplete.Contains(levelIdentifier))
            SaveData.LevelsComplete.Add(levelIdentifier);
    }
    #endregion

    #endregion
}