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
    [Header("Credits Navigation")]
    [SerializeField, Tooltip("Enabled/Disabled to swap between main menu and credits.")]
    private GameObject _mainMenuContainer;
    [SerializeField, Tooltip("Enabled/Disabled to swap between main menu and credits.")]
    private GameObject _creditsContainer;

    [Header("Scene Transitions")]
    [SerializeField, Tooltip("Used to make calls to smooth scene transitions.")]
    private SceneTransitionHandler _transitionHandler;
    [SerializeField, Tooltip("Scene name of level select scene.")]
    private string _levelSelectSceneName;
    [SerializeField, Tooltip("Scene name of the first level. Bypass level select on new game.")]
    private string _firstLevelName;

    [Header("New Game Functionality")]
    [SerializeField, Tooltip("Used to enable/disable resume button based on whether there is a save to resume.")]
    private GameObject _resumeButton;
    [SerializeField, Tooltip("Enabled if new game would clear save data.")]
    private GameObject _newGameConfirmation;

    private void Awake()
    {
        // Only show resume button if there is save data to resume with
        if (!GameManager.Instance.SaveData.NewGameStarted)
        {
            _resumeButton.SetActive(false);
        }
    }

    #region Main Menu Buttons
    /// <summary>
    /// Functionality for New Game Button.
    /// Either loads level select on new save (if no save data present) OR shows confirmation popup before overwriting save
    /// </summary>
    public void NewGameButton()
    {
        // overriding previous save -> confirmation popup
        if (GameManager.Instance.SaveData.NewGameStarted)
        {
            _newGameConfirmation.SetActive(true);
        }
        // no save data being overriden
        else
        {
            GameManager.Instance.ResetProgressionData(); // new progression data
            GameManager.Instance.SaveData.NewGameStarted = true;
            _transitionHandler.LoadScene(_firstLevelName);
        }
    }

    /// <summary>
    /// Resume Button Functionality.
    /// Loads level select scene.
    /// </summary>
    public void ResumeButton()
    {
        _transitionHandler.LoadScene(_levelSelectSceneName);
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

    /// <summary>
    /// Credits Button functionality.
    /// Disables main manu container and enables credits container.
    /// </summary>
    public void CreditsButton()
    {
        _creditsContainer.SetActive(true);
        _mainMenuContainer.SetActive(false);
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

    #region Credits Buttons
    /// <summary>
    /// Button used for returning back to the main menu (from credits).
    /// Disables credits container and enables main menu container.
    /// </summary>
    public void BackButton()
    {
        _creditsContainer.SetActive(false);
        _mainMenuContainer.SetActive(true);
    }
    #endregion

    #region New Game Confirmation
    /// <summary>
    /// Loads new game without any stipulations, this is the confirmation function.
    /// </summary>
    public void ConfirmNewGame()
    {
        GameManager.Instance.ResetProgressionData(); // new progression data
        GameManager.Instance.SaveData.NewGameStarted = true;
        _transitionHandler.LoadScene(_firstLevelName);
    }

    /// <summary>
    /// Cancels the confirmation popup for creating a new game.
    /// Can be used by other buttons to ensure popup is closed.
    /// </summary>
    public void AbortNewGame()
    {
        _newGameConfirmation.SetActive(false);
    }
    #endregion
}
