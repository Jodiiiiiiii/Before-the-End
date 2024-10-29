using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseCanvas : MonoBehaviour
{
    [SerializeField, Tooltip("Used to disable/enable the entire canvas based on pause state.")]
    private Canvas _canvas;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _canvas.enabled = GameManager.Instance.IsPaused;
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
    /// Opens the options menu (from the pause menu)
    /// </summary>
    public void ToOptions()
    {

    }

    public void Exit()
    {
        // TODO: when in level -> go to level select

        // TODO: when in level select -> go to start menu
    }
    #endregion

    #region OPTIONS MENU
    #endregion
}
