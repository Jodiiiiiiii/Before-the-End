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
    [SerializeField, Tooltip("Used to reload the scene.")]
    private SceneTransitionHandler _transitionHandler;

    /// <summary>
    /// Sets all level completion states to true for testers to test different iterations more quickly.
    /// </summary>
    public void UnlockAllLevels()
    {
        // set all levels to complete
        for (int i = 0; i < GameManager.Instance.SaveData.LevelsComplete.Length; i++)
            GameManager.Instance.SaveData.LevelsComplete[i] = true;

        // reload current scene
        _transitionHandler.LoadScene(SceneManager.GetActiveScene().name);
    }
}
