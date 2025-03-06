using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Aligns move text to match controls on move help book panel.
/// </summary>
public class AlignMoveText : MonoBehaviour
{
    [SerializeField, Tooltip("Used to update text")]
    private TextMeshProUGUI _text;
    [SerializeField, Tooltip("Used to read action bindings")]
    private InputActionReference[] _actions;

    private void OnEnable()
    {
        _text.text = "Controls:\n'" + ReadAction(0) + "', '" + ReadAction(1) + "', '" + ReadAction(2) + "', '" + ReadAction(3) + "'";
    }

    /// <summary>
    /// Returns corresponding binding associated with action.
    /// Prioritizes first binding, and returns "Not Bound" if no bindiing found.
    /// </summary>
    private string ReadAction(int index)
    {
        string binding1 = InputSystem.actions.FindAction(_actions[index].name).GetBindingDisplayString(0, InputBinding.DisplayStringOptions.DontIncludeInteractions);
        string binding2 = InputSystem.actions.FindAction(_actions[index].name).GetBindingDisplayString(1, InputBinding.DisplayStringOptions.DontIncludeInteractions);
        if (binding1 != "")
            return binding1;
        else if (binding2 != "")
            return binding2;
        else
            return "Unbound";
    }
}
