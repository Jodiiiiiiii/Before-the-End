using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseCanvas : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField, Tooltip("Allows bypassing the pause menu, which is useful in the main menu.")]
    private bool _bypassPause = false;

    [Header("Pause Navigation")]
    [SerializeField, Tooltip("Used to disable/enable the entire canvas based on pause state.")]
    private Canvas _canvas;
    [SerializeField, Tooltip("Enabled/disabled when switching between pause and options menus.")]
    private GameObject _pauseMenu;
    [SerializeField, Tooltip("Enabled/disabled when switching between pause and options menus.")]
    private GameObject _optionsMenu;
    [SerializeField, Tooltip("Enabled/disabled when switching between pause and help book menus.")]
    private GameObject _helpMenu;

    [Header("Scene Transitions")]
    [SerializeField, Tooltip("Used to transition back to level select.")]
    private string _levelSelectName;
    [SerializeField, Tooltip("Used to transition back to start menu.")]
    private string _startMenuName;
    [SerializeField, Tooltip("Used to make actual scene transition calls.")]
    private SceneTransitionHandler _transitionHandler;

    private InputActionAsset _actions;

    private void OnEnable()
    {
        // necessary to make sure controls data is loaded before initialization
        _canvas.enabled = GameManager.Instance.IsPaused;

        _actions = InputSystem.actions;
        if (!_bypassPause)
            _actions.actionMaps[0].FindAction("Help").started += ToggleHelp;
    }

    private void OnDisable()
    {
        if (!_bypassPause)
        _actions.actionMaps[0].FindAction("Help").started -= ToggleHelp;
    }


    /// <summary>
    /// Called on hotkey press for help binding.
    /// Handles both opening and closing (for hotkey press) - bypassing pause menu entirely.
    /// </summary>
    private void ToggleHelp(InputAction.CallbackContext context)
    {
        // no opening pause menu while in scene transition
        if (SceneTransitionHandler.IsTransitioningOut)
            return;

        // close help menu
        if (_helpMenu.activeInHierarchy)
        {
            Unpause();
        }
        //  open help menu
        else
        {
            // ensure paused before navigating to help (allows opening help directly and skipping pause menu)
            GameManager.Instance.IsPaused = true;

            ToHelp();
        }
    }

    // Update is called once per frame
    void Update()
    {
        _canvas.enabled = GameManager.Instance.IsPaused;

        // ensure pausing always opens up to pause menu, not options.
        if(_canvas.enabled == false && !_bypassPause)
        {
            // pause menu by default when you pause, not options
            _pauseMenu.SetActive(true);
            _optionsMenu.SetActive(false);
            _helpMenu.SetActive(false);

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
        // Click UI SFX
        AudioManager.Instance.PlayClickUI();

        GameManager.Instance.IsPaused = false;
    }

    /// <summary>
    /// Opens the help book menu (from the pause menu).
    /// </summary>
    public void ToHelp()
    {
        // Click UI SFX
        AudioManager.Instance.PlayClickUI();

        // clear help notification on opening help book
        GameManager.Instance.SaveData.HelpNotif = false;

        _pauseMenu.SetActive(false);
        _optionsMenu.SetActive(false);
        _helpMenu.SetActive(true);
    }

    /// <summary>
    /// Opens the options menu (from the pause menu).
    /// </summary>
    public void ToOptions()
    {
        // Click UI SFX
        AudioManager.Instance.PlayClickUI();

        _pauseMenu.SetActive(false);
        _optionsMenu.SetActive(true);
        _helpMenu.SetActive(false);
    }

    /// <summary>
    /// In a level: returns to level select
    /// In level select: returns to start menu
    /// </summary>
    public void Exit()
    {
        // level exit SFX
        AudioManager.Instance.PlayLevelExit();

        if (SceneManager.GetActiveScene().name == _levelSelectName + "1" || SceneManager.GetActiveScene().name == _levelSelectName + "2")
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
        // Click UI SFX
        AudioManager.Instance.PlayClickUI();

        _pauseMenu.SetActive(true);
        _optionsMenu.SetActive(false);
        _helpMenu.SetActive(false);

        // controls is default menu for options
        _controlsMenu.SetActive(true);
        _audioMenu.SetActive(false);
    }

    /// <summary>
    /// Shows controls settings and hides all other options.
    /// </summary>
    public void ToControls()
    {
        // Click UI SFX
        if (!_controlsMenu.activeSelf) // don't play sound if already on that tab
            AudioManager.Instance.PlayClickUI();

        _controlsMenu.SetActive(true);
        _audioMenu.SetActive(false);
    }

    /// <summary>
    /// Shows audio settings and hides all other options.
    /// </summary>
    public void ToAudio()
    {
        // Click UI SFX
        if (!_audioMenu.activeSelf) // don't play sound if already on that tab
            AudioManager.Instance.PlayClickUI();

        _controlsMenu.SetActive(false);
        _audioMenu.SetActive(true);
    }
    #endregion
}
