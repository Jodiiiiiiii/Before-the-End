using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionHelper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Useful scene management function to allow basic level transitions before the proper level select functionality is created.
    /// Loads after a brief delay to allow the player time to move to the goal position
    /// </summary>
    public static void LoadNextScene()
    {
        // Parse current scene's name
        string currScene = SceneManager.GetActiveScene().name;
        int currSceneNum = Mathf.RoundToInt((float) Char.GetNumericValue(currScene[currScene.Length - 1]));

        // exit if load scene will not be valid (doesn't end in integer)
        if (currSceneNum == -1)
            return;

        // load next scene
        string nextScene = currScene.Substring(0, currScene.Length - 1) + "" + (currSceneNum + 1);
        SceneManager.LoadScene(nextScene);
    }
}
