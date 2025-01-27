using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Contains all button functionality and credits swapping located on the main menu UI scene.
/// </summary>
public class MainMenuHandler : MonoBehaviour
{
    private void Awake()
    {
        // configure whether resume button shows up at all
    }

    #region Main Menu Buttons
    public void NewGameButton()
    {

    }

    public void ResumeButton()
    {

    }

    /// <summary>
    /// Options Button Functionality.
    /// Opens Controls/Audio options, skipping the pause menu segment of the Pause Canvas.
    /// </summary>
    public void OptionsButton()
    {
        // Modified Pause Canvas is able to enable itself when this is toggled on
        GameManager.Instance.IsPaused = true;
    }

    public void CreditsButton()
    {

    }

    /// <summary>
    /// Quit Button functionality.
    /// Exits play mode (in editor) or closes application (in build)
    /// </summary>
    public void QuitButton()
    {
#if UNITY_EDITOR
        // quits play mode if in editor
        EditorApplication.ExitPlaymode();
#endif
        Application.Quit();
    }
    #endregion
}
