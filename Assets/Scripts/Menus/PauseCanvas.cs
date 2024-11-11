using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PauseCanvas : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField, Tooltip("Used to disable/enable the entire canvas based on pause state.")]
    private Canvas _canvas;
    [SerializeField, Tooltip("Enabled/disabled when switching between pause and options menus")]
    private GameObject _pauseMenu;
    [SerializeField, Tooltip("Enabled/disabled when switching between pause and options menus")]
    private GameObject _optionsMenu;

    private void OnEnable()
    {
        // necessary to make sure controls data is loaded before initialization
        _canvas.enabled = GameManager.Instance.IsPaused;

        // set initial configuration of all text elements
        InitializeDisplayTexts();
    }

    public void OnDisable()
    {
        // prevent memory leak
        _rebind?.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        _canvas.enabled = GameManager.Instance.IsPaused;

        // ensure pausing always opens up to pause menu, not options.
        if(_canvas.enabled == false)
        {
            _pauseMenu.SetActive(true);
            _optionsMenu.SetActive(false);
        }
    }

    #region PAUSE MENU
    /// <summary>
    /// Interfaces with the Game Manager to unpause the game level.
    /// </summary>
    public void Unpause()
    {
        GameManager.Instance.IsPaused = false;
    }

    /// <summary>
    /// Opens the options menu (from the pause menu).
    /// </summary>
    public void ToOptions()
    {
        _pauseMenu.SetActive(false);
        _optionsMenu.SetActive(true);
    }

    public void Exit()
    {
        // TODO: when in level -> go to level select

        // TODO: when in level select -> go to start menu
    }
    #endregion

    #region OPTIONS MENU

    /// <summary>
    /// Opens the pause menu (from the options menu).
    /// </summary>
    public void ToPause()
    {
        _pauseMenu.SetActive(true);
        _optionsMenu.SetActive(false);
    }

    #region CONTROLS REMAPPING
    [Header("Controls Remapping")]
    [SerializeField, Tooltip("Must be assigned in same order as ControlsType enum definition.")]
    public InputActionReference[] _actionReferences;
    [SerializeField, Tooltip("Must be assigned in same order as ControlsType enum definition.")]
    public TextMeshProUGUI[] _displayTexts;
    [SerializeField, Tooltip("Must be assigned in same order as ControlsType enum definition.")]
    public TextMeshProUGUI[] _altDisplayTexts;

    private InputActionRebindingExtensions.RebindingOperation _rebind;

    /// <summary>
    /// Function called by remap button on click to remap controls.
    /// </summary>
    public void RemapButtonClicked(int controlToRemap)
    {
        RemapOperation(controlToRemap, false);
    }

    /// <summary>
    /// Function called by alt remap button on click to remap controls.
    /// </summary>
    public void AltRemapButtonClicked(int controlToRemap)
    {
        RemapOperation(controlToRemap, true);
    }

    /// <summary>
    /// Returns this control to default controls (removes overrides).
    /// Resets both main binding AND alt binding.
    /// </summary>
    public void ResetControl(int controlToReset)
    {
        string actionToReset = _actionReferences[controlToReset].action.name;

        // restore default behavior
        //_actionReferences[controlToReset].action.RemoveAllBindingOverrides();
        InputSystem.actions.FindAction(actionToReset).RemoveAllBindingOverrides();

        // update main binding text
        _displayTexts[controlToReset].text =
        //        _actionReferences[controlToReset].action.GetBindingDisplayString(0, InputBinding.DisplayStringOptions.DontIncludeInteractions);
            InputSystem.actions.FindAction(actionToReset).GetBindingDisplayString(0, InputBinding.DisplayStringOptions.DontIncludeInteractions);

        // update alt binding text
        _altDisplayTexts[controlToReset].text =
            //_actionReferences[controlToReset].action.GetBindingDisplayString(1, InputBinding.DisplayStringOptions.DontIncludeInteractions);
            InputSystem.actions.FindAction(actionToReset).GetBindingDisplayString(1, InputBinding.DisplayStringOptions.DontIncludeInteractions);
        // update string if empty binding
        if (_altDisplayTexts[controlToReset].text == "")
            _altDisplayTexts[controlToReset].text = "--";
    }

    /// <summary>
    /// Returns ALL controls to default controls (including main and alt bindings).
    /// </summary>
    public void ResetAllControls()
    {
        // iterate through all controls to reset
        for (int i = 0; i < _actionReferences.Length; i++)
            ResetControl(i);
    }

    /// <summary>
    /// Starts remapping operation for provided control, handling next input press as the new input binding.
    /// Allows for remapping of alternate binding as well.
    /// </summary>
    public void RemapOperation(int controlToRemap, bool isAlt = false)
    {
        string actionToRemap = _actionReferences[controlToRemap].action.name;

        // ensure not being changed while enabled (crashes)
        //_actionReferences[controlToRemap].action.Disable();
        InputSystem.actions.FindAction(actionToRemap).Disable();

        // configure rebinding operation
        //_rebind = _actionReferences[controlToRemap].action.PerformInteractiveRebinding(isAlt ? 1 : 0)
        _rebind = InputSystem.actions.FindAction(actionToRemap).PerformInteractiveRebinding(isAlt ? 1 : 0)
            .WithCancelingThrough("<Mouse>/leftButton")
            .OnCancel(_ => RemoveBinding(controlToRemap, isAlt))
            .OnComplete(_ => RemappingComplete(controlToRemap, isAlt));

        _rebind.Start();
    }

    /// <summary>
    /// Unbinds any override and creates a new empty (null) override.
    /// This is distinct from default controls; it overides default with no control
    /// </summary>
    private void RemoveBinding(int controlToUnbind, bool isAlt)
    {
        string actionToUnbind = _actionReferences[controlToUnbind].action.name;

        //_actionReferences[controlToUnbind].action.RemoveBindingOverride(isAlt ? 1 : 0);
        InputSystem.actions.FindAction(actionToUnbind).RemoveBindingOverride(isAlt ? 1 : 0);
        //_actionReferences[controlToUnbind].action.ApplyBindingOverride(isAlt ? 1 : 0, "");
        InputSystem.actions.FindAction(actionToUnbind).ApplyBindingOverride(isAlt ? 1 : 0, "");

        // update text accordingly
        if (isAlt)
            _altDisplayTexts[controlToUnbind].text = "--";
        else
            _displayTexts[controlToUnbind].text = "--";

        // so it can actually be used again - if this wasn't here, it would break one control when the other was unbound
        //_actionReferences[controlToUnbind].action.Enable();
        InputSystem.actions.FindAction(actionToUnbind).Enable();
    }

    /// <summary>
    /// Handles all functionality once the rebinding override process is complete.
    /// Updates corresponding UI text.
    /// Checks for duplicate bindings to clear.
    /// Re-enables input actions.
    /// </summary>
    private void RemappingComplete(int controlToUpdate, bool isAlt)
    {
        string actionToUpdate = _actionReferences[controlToUpdate].action.name;

        //string newInput = _actionReferences[controlToUpdate].action
        string newInput = InputSystem.actions.FindAction(actionToUpdate)
            .GetBindingDisplayString(isAlt ? 1 : 0, InputBinding.DisplayStringOptions.DontIncludeInteractions);

        // Escape should undo binding
        if (newInput == "Esc")
        {
            RemoveBinding(controlToUpdate, isAlt);
        }
        else // valid binding
        {
            // Update text
            if (isAlt) // alt binding
            {
                _altDisplayTexts[controlToUpdate].text = newInput;
                // indicate no binding rather than empty string
                if (_altDisplayTexts[controlToUpdate].text == "")
                    _altDisplayTexts[controlToUpdate].text = "--";
            }
            else // first binding
            {
                _displayTexts[controlToUpdate].text = newInput;
                // indicate no binding rather than empty string
                if (_displayTexts[controlToUpdate].text == "")
                    _displayTexts[controlToUpdate].text = "--";
            }

            // Delete duplicate bindings
            DuplicateBindingCheck(controlToUpdate, isAlt);
        }

        // so it can actually be used again
        //_actionReferences[controlToUpdate].action.Enable();
        InputSystem.actions.FindAction(actionToUpdate).Enable();
    }

    /// <summary>
    /// Checks for cases where the player has set the same control for two different functions.
    /// Removes binding on overriden control and removes text accordingly.
    /// </summary>
    private void DuplicateBindingCheck(int remappedControl, bool isAlt)
    {
        string remappedAction = _actionReferences[remappedControl].action.name;

        //string binding = _actionReferences[remappedControl].action
        string binding = InputSystem.actions.FindAction(remappedAction)
            .GetBindingDisplayString(isAlt ? 1 : 0, InputBinding.DisplayStringOptions.DontIncludeInteractions);

        // check for duplicate binding
        for (int i = 0; i < _actionReferences.Length; i++)
        {
            string otherAction = _actionReferences[i].action.name;

            // check both first and alt bindings
            for (int j = 0; j < 2; j++)
            {
                string otherBinding = InputSystem.actions.FindAction(otherAction)/*_actionReferences[i].action*/
                    .GetBindingDisplayString(j, InputBinding.DisplayStringOptions.DontIncludeInteractions);

                // check for matched duplicate binding (but make sure it isn't itself)
                if ((i != remappedControl || (j==1) != isAlt ) && otherBinding == binding)
                {

                    // override any potential overrides and then set a new override to no binding
                    //_actionReferences[i].action.RemoveBindingOverride(j);
                    InputSystem.actions.FindAction(otherAction).RemoveBindingOverride(j);
                    //_actionReferences[i].action.ApplyBindingOverride(j, "");
                    InputSystem.actions.FindAction(otherAction).ApplyBindingOverride(j, "");

                    // also update text accordingly
                    if (j == 1) // alt
                        _altDisplayTexts[i].text = "--";
                    else // first binding
                        _displayTexts[i].text = "--";

                    // duplicate found, stop checking for more
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Sets text in all controls boxes to match current controls.
    /// Used for initial configuration
    /// </summary>
    private void InitializeDisplayTexts()
    {
        // error checking
        if (_actionReferences.Length != _displayTexts.Length || _displayTexts.Length != _altDisplayTexts.Length)
            throw new System.Exception("Improperly configured options UI. Action References, Display Texts, & Alt Display Texts all must be the same length");

        // initialize each display texts (and alt)
        for (int i = 0; i < _displayTexts.Length; i++)
        {
            string actionToRead = _actionReferences[i].action.name;

            _displayTexts[i].text =
                InputSystem.actions.FindAction(actionToRead).GetBindingDisplayString(0, InputBinding.DisplayStringOptions.DontIncludeInteractions);
                //_actionReferences[i].action.GetBindingDisplayString(0, InputBinding.DisplayStringOptions.DontIncludeInteractions);

            // indicate no binding rather than empty string
            if (_displayTexts[i].text == "")
                _displayTexts[i].text = "--";

            _altDisplayTexts[i].text =
                InputSystem.actions.FindAction(actionToRead).GetBindingDisplayString(1, InputBinding.DisplayStringOptions.DontIncludeInteractions);
                //_actionReferences[i].action.GetBindingDisplayString(1, InputBinding.DisplayStringOptions.DontIncludeInteractions);

            // indicate no binding rather than empty string
            if (_altDisplayTexts[i].text == "")
                _altDisplayTexts[i].text = "--";
        }
    }
    #endregion

    #endregion
}
