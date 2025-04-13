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

    private void Start()
    {
        // initial text alignment
        // no need to check in update since controls CANNOT be rebound during cutscene
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
    #endregion
}
