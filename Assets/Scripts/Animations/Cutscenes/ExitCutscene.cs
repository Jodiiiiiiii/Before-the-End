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

    public void ReturnToLevelSelect()
    {
        // does not use scene transitions handler since cutscene already ends at full faded out color
        SceneManager.LoadScene(_levelSelectName);
    }
}
