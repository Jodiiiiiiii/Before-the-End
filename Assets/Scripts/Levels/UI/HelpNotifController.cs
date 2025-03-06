using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Aligns move text to match controls on ability help book panel.
/// </summary>
public class HelpNotifController : MonoBehaviour
{
    #region Enable/Disable
    [Header("Enable/Disable")]
    [SerializeField, Tooltip("List of game objects contained in the help notification to enable/disable based on game manager state.")]
    private GameObject[] _objects;

    private bool _isNotif;

    private void Awake()
    {
        // initial configuration
        _isNotif = GameManager.Instance.SaveData.HelpNotif;
        foreach (GameObject obj in _objects)
            obj.SetActive(_isNotif);
    }

    private void Update()
    {
        // allow popup to disappear once the variable changes
        if (GameManager.Instance.SaveData.HelpNotif != _isNotif)
        {
            _isNotif = GameManager.Instance.SaveData.HelpNotif;
            foreach (GameObject obj in _objects)
                obj.SetActive(_isNotif);
        }

        // update controls text if pause menu is opened (account for potential remapping happening)
        if (GameManager.Instance.IsPaused)
            _text.text = "Press '" + ReadAction() + "'\nFor Help";
    }
    #endregion

    #region Text Alignment
    [Header("Text Alignment")]
    [SerializeField, Tooltip("Used to update text")]
    private TextMeshProUGUI _text;
    [SerializeField, Tooltip("Used to read action bindings")]
    private InputActionReference _action;

    /// <summary>
    /// Returns corresponding binding associated with action.
    /// Prioritizes first binding, and returns "Not Bound" if no bindiing found.
    /// </summary>
    private string ReadAction()
    {
        string binding1 = InputSystem.actions.FindAction(_action.name).GetBindingDisplayString(0, InputBinding.DisplayStringOptions.DontIncludeInteractions);
        string binding2 = InputSystem.actions.FindAction(_action.name).GetBindingDisplayString(1, InputBinding.DisplayStringOptions.DontIncludeInteractions);
        if (binding1 != "")
            return binding1;
        else if (binding2 != "")
            return binding2;
        else
            return "Unbound";
    }
    #endregion
}