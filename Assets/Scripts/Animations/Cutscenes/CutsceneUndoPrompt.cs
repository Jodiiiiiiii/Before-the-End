using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Aligns undo prompt with undo binding.
/// Resets undo binding if no binding found (to prevent softlock in cutscene).
/// </summary>
public class CutsceneUndoPrompt : MonoBehaviour
{
    #region Enable / Disable Prompt
    [SerializeField, Tooltip("Rate at which canvas fade level changes (increase/decrease)")]
    private float _fadeChangeRate;
    [SerializeField, Tooltip("Used to change fade level of canvas group.")]
    private CanvasGroup _fader;

    private bool _isShowing = false; // not showing by default

    public void EnableUndoPrompt()
    {
        _isShowing = true;
    }

    public void DisableUndoPrompt()
    {
        _isShowing = false;
    }

    private void Update()
    {
        // necessary since bindings are not able to be read with overrides on start or first frame?
        // strange bug case but this works
        _text.text = "Press '" + ReadAction() + "'";

        // increase to full opacity
        if (_isShowing && _fader.alpha < 1f)
        {
            _fader.alpha = Mathf.Clamp(_fader.alpha + _fadeChangeRate * Time.deltaTime, 0f, 1f);
        }
        // decrease to no opacity
        else if (!_isShowing && _fader.alpha > 0f)
        {
            _fader.alpha = Mathf.Clamp(_fader.alpha - _fadeChangeRate * Time.deltaTime, 0f, 1f);
        }
    }
    #endregion

    #region Text Alignment
    [SerializeField, Tooltip("Used to update text")]
    private TextMeshProUGUI _text;
    [SerializeField, Tooltip("Used to read action bindings")]
    private InputActionReference _action;

    private bool _actionReset = false; // by default, assume not reset

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
            return ResetUndo();
    }

    public string ResetUndo()
    {
        // makes sure that bindings are properly UNBOUND after scene ends
        _actionReset = true;

        string actionToReset = _action.name;

        // restore default behavior
        InputSystem.actions.FindAction(actionToReset).RemoveAllBindingOverrides();

        // return new reset binding string
        return InputSystem.actions.FindAction(actionToReset).GetBindingDisplayString(0, InputBinding.DisplayStringOptions.DontIncludeInteractions);
    }

    private void OnDisable()
    {
        // re-unbind actions
        // this is necessary to guarantee 100% that no duplicate bindings persist due to the manual overrides of this specific scene only
        if (_actionReset)
        {
            string actionToUnbind = _action.name;

            // restore override of unbound - primary binding
            InputSystem.actions.FindAction(actionToUnbind).RemoveBindingOverride(0);
            InputSystem.actions.FindAction(actionToUnbind).ApplyBindingOverride(0, "");

            // restore override of unbound - secondary binding
            InputSystem.actions.FindAction(actionToUnbind).RemoveBindingOverride(1);
            InputSystem.actions.FindAction(actionToUnbind).ApplyBindingOverride(1, "");

            // so it can actually be used again - if this wasn't here, it would break one control when the other was unbound
            // not sure if this needs to stay here, but keeping it to be safe
            InputSystem.actions.FindAction(actionToUnbind).Enable();
        }
    }
    #endregion
}
