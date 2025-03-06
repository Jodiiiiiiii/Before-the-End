using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles initialization of help book cards to determine which cards show in any given scene.
/// Only configures on start, since all changes in unlocks would occur at a scene transition.
/// </summary>
public class HelpBookInitializer : MonoBehaviour
{
    [SerializeField, Tooltip("Used to enable/disable cards based on unlocked mechanics.")]
    private GameObject[] _helpPanels;
    [SerializeField, Tooltip("Used to determine if panels are unlocked.")]
    private string[] _helpStrings;

    private void Awake()
    {
        // Precondition: equal array lenghts
        if (_helpPanels.Length != _helpStrings.Length)
            throw new System.Exception("Incorrect Help book configuration: must have equal number of help panels and corresponding strings.");

        // configure startup states on awake
        for (int i = 0; i < _helpPanels.Length; i++)
        {
            if (GameManager.Instance.SaveData.HelpUnlocks.Contains(_helpStrings[i]))
                _helpPanels[i].SetActive(true);
            else
                _helpPanels[i].SetActive(false);
        }
    }
}
