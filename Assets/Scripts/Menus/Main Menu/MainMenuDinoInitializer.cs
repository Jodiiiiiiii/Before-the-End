using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enables/Disables the different dinos in the main menu based on current unlock states.
/// </summary>
public class MainMenuDinoInitializer : MonoBehaviour
{
    [SerializeField, Tooltip("Used to enable/disable dino containers")]
    private GameObject[] _dinos;

    void Awake()
    {
        // Requirement: 6 dino objects given
        if (_dinos.Length != 6)
            throw new System.Exception("There MUST be 6 dinos to enable/disable in main menu (all but stego).");

        // go through all non-stego dinos
        InitDino("Trike", 0);
        InitDino("Anky", 1);
        InitDino("Spino", 2);
        InitDino("Pyro", 3);
        InitDino("Ptera", 4);
        InitDino("Compy", 5);
    }

    private void InitDino(string identifier, int index)
    {
        // works regardless of initial editor config state
        if (GameManager.Instance.SaveData.HelpUnlocks.Contains(identifier))
            _dinos[index].SetActive(true);
        else
            _dinos[index].SetActive(false);
    }
}
