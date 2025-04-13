using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles functionality for game cheats.
/// e.g. unlocking all progression levels for quicker testing
/// </summary>
public class CheatsControls : MonoBehaviour
{
    public delegate void UnlockLevelsCheat();
    public static event UnlockLevelsCheat UnlockLevels;

    [SerializeField, Tooltip("Used to reload the scene.")]
    private SceneTransitionHandler _transitionHandler;

    /// <summary>
    /// Sets all level completion states to true for testers to test different iterations more quickly.
    /// </summary>
    public void UnlockAllLevels()
    {
        // Click UI SFX
        AudioManager.Instance.PlayClickUI();

        // cheats now unlock second timeline
        //GameManager.Instance.SaveData.isSecondTimelineUnlock = true;

        UnlockLevels?.Invoke();

        StartCoroutine(ReloadAfterDelay());
    }

    /// <summary>
    /// Used to guarantee all levels are properly unlocked BEFORE reloading
    /// </summary>
    public IEnumerator ReloadAfterDelay()
    {
        yield return new WaitForSeconds(0.25f);

        // reload current scene
        _transitionHandler.LoadScene(SceneManager.GetActiveScene().name);
    }
}
