using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton manager that handles incrementing time ticks to be accessed by all animation scripts to get the universal animation frame to use.
/// </summary>
public class AnimationManager : MonoBehaviour
{
    #region Singleton Setup
    // private singleton instance
    private static AnimationManager _instance;

    // public accessor of instance
    public static AnimationManager Instance
    {
        get
        {
            // setup GameManager as a singleton class
            if (_instance == null)
            {
                // create new game manager object
                GameObject newManager = new();
                newManager.name = "Animation Manager";
                newManager.AddComponent<AnimationManager>();
                DontDestroyOnLoad(newManager);
                _instance = newManager.GetComponent<AnimationManager>();
            }
            // return new/existing instance
            return _instance;
        }
    }
    #endregion

    #region Animation Counter
    public static float TIME_PER_FRAME = 0.5f;

    private int _frameNum = 0;

    // Update is called once per frame
    void Update()
    {
        // frame number as 0 or 1
        // handling it this way instead of a timer prevents animation timer drift based on frame rate (important to sync with tilemap animations)
        _frameNum = Mathf.RoundToInt(Time.timeSinceLevelLoad / TIME_PER_FRAME) % 2;
    }

    /// <summary>
    /// Returns current frame number for 2-frame animations.
    /// Returns ONLY 0 or 1.
    /// </summary>
    public int GetFrameNum()
    {
        // Output Condition: frame number MUST be 0 or 1
        if (_frameNum != 0 && _frameNum != 1)
            throw new System.Exception("Error in Animation Manager. How is frame number anything other than 0 or 1");

        return _frameNum;
    }
    #endregion
}