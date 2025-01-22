using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseCanvas : MonoBehaviour
{
    [Header("Pause Navigation")]
    [SerializeField, Tooltip("Used to disable/enable the entire canvas based on pause state.")]
    private Canvas _canvas;
    [SerializeField, Tooltip("Enabled/disabled when switching between pause and options menus")]
    private GameObject _pauseMenu;
    [SerializeField, Tooltip("Enabled/disabled when switching between pause and options menus")]
    private GameObject _optionsMenu;

    [Header("Scene Transitions")]
    [SerializeField, Tooltip("Used to transition back to level select.")]
    private string _levelSelectName;
    [SerializeField, Tooltip("Used to transition back to start menu.")]
    private string _startMenuName;
    [SerializeField, Tooltip("Used to make actual scene transition calls.")]
    private SceneTransitionHandler _transitionHandler;

    private void OnEnable()
    {
        // necessary to make sure controls data is loaded before initialization
        _canvas.enabled = GameManager.Instance.IsPaused;
    }

    // Update is called once per frame
    void Update()
    {
        _canvas.enabled = GameManager.Instance.IsPaused;

        // ensure pausing always opens up to pause menu, not options.
        if(_canvas.enabled == false)
        {
            // pause menu by default when you pause, not options
            _pauseMenu.SetActive(true);
            _optionsMenu.SetActive(false);

            // controls is default menu for options
            _controlsMenu.SetActive(true);
            _audioMenu.SetActive(false);
        }

        // update options tab sprite states
        if(_controlsMenu.activeSelf)
        {
            _controlsButtonSprite.sprite = _optionsButtonSprites[1];
            _audioButtonSprite.sprite = _optionsButtonSprites[0];
        }
        else if (_audioMenu.activeSelf)
        {
            _controlsButtonSprite.sprite = _optionsButtonSprites[0];
            _audioButtonSprite.sprite = _optionsButtonSprites[1];
        }
    }

    #region PAUSE MENU
    /// <summary>
    /// Interfaces with the Game Manager to unpause the game level.
    /// </summary>
    public void Unpause()
    {
        GameManager.Instance.IsPaused = false;
    }

    /// <summary>
    /// Opens the options menu (from the pause menu).
    /// </summary>
    public void ToOptions()
    {
        _pauseMenu.SetActive(false);
        _optionsMenu.SetActive(true);
    }

    /// <summary>
    /// In a level: returns to level select
    /// In level select: returns to start menu
    /// </summary>
    public void Exit()
    {
        // TODO: special case for pausing in main menu?

        if (SceneManager.GetActiveScene().name == _levelSelectName)
            _transitionHandler.LoadScene(_startMenuName);
        else // level scene
            _transitionHandler.LoadScene(_levelSelectName);
    }
    #endregion

    #region OPTIONS MENU
    [Header("Options Navigation")]
    [SerializeField, Tooltip("Used to enable/disable controls sub-menu.")]
    private GameObject _controlsMenu;
    [SerializeField, Tooltip("Used to enable/disable audio sub-menu.")]
    private GameObject _audioMenu;
    [SerializeField, Tooltip("Used to manually set controls button sprite based on tab state")]
    private Image _controlsButtonSprite;
    [SerializeField, Tooltip("Used to manually set audio button sprite based on tab selected state")]
    private Image _audioButtonSprite;
    [SerializeField, Tooltip("Sprites for options navigation buttons. 0 = unselected; 1 = selected.")]
    private Sprite[] _optionsButtonSprites;

    /// <summary>
    /// Opens the pause menu (from the options menu).
    /// </summary>
    public void ToPause()
    {
        _pauseMenu.SetActive(true);
        _optionsMenu.SetActive(false);

        // controls is default menu for options
        _controlsMenu.SetActive(true);
        _audioMenu.SetActive(false);
    }

    /// <summary>
    /// Shows controls settings and hides all other options.
    /// </summary>
    public void ToControls()
    {
        _controlsMenu.SetActive(true);
        _audioMenu.SetActive(false);
    }

    /// <summary>
    /// Shows audio settings and hides all other options.
    /// </summary>
    public void ToAudio()
    {
        _controlsMenu.SetActive(false);
        _audioMenu.SetActive(true);
    }
    #endregion
}
