    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class AlignLevelPopupText : MonoBehaviour
{
    [SerializeField, Tooltip("Used to update text")]
    private TextMeshProUGUI _text;
    [SerializeField, Tooltip("Used to read action bindings")]
    private InputActionReference _action;

    private void Start()
    {
        // initial text alignment
        _text.text = "Press '" + ReadAction() + "'";
    }

    private void Update()
    {
        // ensure text alignment even through rebinding
        if (GameManager.Instance.IsPaused)
            _text.text = "Press '" + ReadAction() + "'";
    }

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
}
