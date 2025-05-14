using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles disabling/enabling of the cursor based on pause or scene state.
/// </summary>
public class CursorManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // visible if
        // (1) paused or
        // (2) in main menu
        // Must ALSO have application focus
        if (GameManager.Instance.IsPaused || SceneManager.GetActiveScene().name == "MainMenu")
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        // ALL other conditions - no mouse should show
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
