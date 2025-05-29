using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Contains function for returning to level select
/// </summary>
public class ExitCutscene : MonoBehaviour
{
    [SerializeField, Tooltip("Scene name of level select to return to.")]
    private string _levelSelectName;
    [SerializeField, Tooltip("Used to call functional scene transition.")]
    private SceneTransitionHandler _transitionHandler;

    public void ReturnToLevelSelect()
    {
        // Steam Achievement - Cutscene 1
        Steamworks.SteamUserStats.SetAchievement("VISION_CUTSCENE");
        Steamworks.SteamUserStats.StoreStats(); // ensure popup comes up right away

        // does not use scene transitions handler since cutscene already ends at full faded out color
        _transitionHandler.LoadScene(_levelSelectName);
    }

    public void RewindCutsceneTransitionOut()
    {
        // Steam Achievement - Cutscene 2
        Steamworks.SteamUserStats.SetAchievement("REWIND_CUTSCENE");
        Steamworks.SteamUserStats.StoreStats(); // ensure popup comes up right away

        // return to start of second timeline
        GameManager.Instance.SaveData.CurrLevel = "Tut0";
        GameManager.Instance.SaveData.isSecondTimeline = true;
        // unlock free traversal between timelines
        GameManager.Instance.SaveData.isSecondTimelineUnlock = true;

        // load directly to first level of second timeline
        _transitionHandler.LoadScene("2-Tutorial0");
    }

    public void ToCredits()
    {
        // Steam Achievement - Cutscene 3
        Steamworks.SteamUserStats.SetAchievement("END_CUTSCENE");
        Steamworks.SteamUserStats.StoreStats(); // ensure popup comes up right away

        // load to end credits
        _transitionHandler.LoadScene("EndCredits");
    }

    #region Audio Functions
    public void CutMusic()
    {
        // stop previously playing music / ambient
        AudioManager.Instance.UnqueueMusic();
    }

    public void PlayVisionStart()
    {
        // vision start SFX
        AudioManager.Instance.PlayVisionStart();
    }

    public void PlayNormalAmbient()
    {
        // plains ambient SFX
        AudioManager.Instance.QueuePlainsAmbient();
    }

    public void AddFire()
    {
        // adds fire SFX to layer on top of other potential music
        AudioManager.Instance.EnableAmbientLevelFire();
    }

    public void RemoveFire()
    {
        // removes layered fire SFX
        AudioManager.Instance.DisableAmbientLevelFire();
    }

    public void PlayRumble()
    {
        // Rumble SFX
        AudioManager.Instance.PlayRumble();
    }

    public void PlayAsteroidImpact()
    {
        // Asteroid Impact SFX
        AudioManager.Instance.PlayAsteroidImpact();
    }

    public void PlayBreeze()
    {
        // Breeze SFX
        AudioManager.Instance.PlayBreeze();
    }
    #endregion
}
