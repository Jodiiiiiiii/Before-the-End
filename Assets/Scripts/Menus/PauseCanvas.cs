using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseCanvas : MonoBehaviour
{
    [SerializeField, Tooltip("Used to disable/enable the entire canvas based on pause state.")]
    private Canvas _canvas;
    [SerializeField, Tooltip("Enabled/disabled when switching between pause and options menus")]
    private GameObject _pauseMenu;
    [SerializeField, Tooltip("Enabled/disabled when switching between pause and options menus")]
    private GameObject _optionsMenu;

    // Update is called once per frame
    void Update()
    {
        _canvas.enabled = GameManager.Instance.IsPaused;

        // ensure pausing always opens up to pause menu, not options.
        if(_canvas.enabled == false)
        {
            _pauseMenu.SetActive(true);
            _optionsMenu.SetActive(false);
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

    public void Exit()
    {
        // TODO: when in level -> go to level select

        // TODO: when in level select -> go to start menu
    }
    #endregion

    #region OPTIONS MENU
    /// <summary>
    /// Opens the pause menu (from the options menu).
    /// </summary>
    public void ToPause()
    {
        _pauseMenu.SetActive(true);
        _optionsMenu.SetActive(false);
    }
    #endregion
}
