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

    [Header("Remapping")]
    [SerializeField, Tooltip("Must be assigned in same order as ControlsType enum definition.")]
    public InputActionReference[] _actionReferences;
    [SerializeField, Tooltip("Must be assigned in same order as ControlsType enum definition.")]
    public TextMeshProUGUI[] _displayTexts;
    [SerializeField, Tooltip("Must be assigned in same order as ControlsType enum definition.")]
    public TextMeshProUGUI[] _altDisplayTexts;

    private InputActionRebindingExtensions.RebindingOperation _rebind;

    /// <summary>
    /// Opens the pause menu (from the options menu).
    /// </summary>
    public void ToPause()
    {
        _pauseMenu.SetActive(true);
        _optionsMenu.SetActive(false);
    }

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
    /// Starts remapping operation for provided control, handling next input press as the new input binding.
    /// Allows for remapping of alternate binding as well.
    /// </summary>
    public void RemapOperation(int controlToRemap, bool isAlt = false)
    {
        // ensure not being changed while enabled (crashes)
        _actionReferences[controlToRemap].action.Disable();

        // configure rebinding operation
        _rebind = _actionReferences[controlToRemap].action.PerformInteractiveRebinding(isAlt ? 1 : 0)
            .WithControlsExcluding("Mouse")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnCancel(_ => RemoveBinding(controlToRemap, isAlt))
            .OnComplete(_ => RemappingComplete(controlToRemap, isAlt));

        _rebind.Start();
    }

    /// <summary>
    /// Unbinds
    /// </summary>
    /// <param name="controlToUnbind"></param>
    /// <param name="isAlt"></param>
    private void RemoveBinding(int controlToUnbind, bool isAlt)
    {
        _actionReferences[controlToUnbind].action.RemoveBindingOverride(isAlt ? 1 : 0);
        _actionReferences[controlToUnbind].action.ApplyBindingOverride(isAlt ? 1 : 0, "");

        // update text accordingly
        if (isAlt)
            _altDisplayTexts[controlToUnbind].text = "--";
        else
            _displayTexts[controlToUnbind].text = "--";
    }

    /// <summary>
    /// Handles all functionality once the rebinding override process is complete.
    /// Updates corresponding UI text.
    /// Checks for duplicate bindings to clear.
    /// Re-enables input actions.
    /// </summary>
    private void RemappingComplete(int controlToUpdate, bool isAlt)
    {
        // Update text
        if(isAlt) // alt binding
        {
            _altDisplayTexts[controlToUpdate].text =
                _actionReferences[controlToUpdate].action.GetBindingDisplayString(1, InputBinding.DisplayStringOptions.DontIncludeInteractions);
            // indicate no binding rather than empty string
            if (_altDisplayTexts[controlToUpdate].text == "")
                _altDisplayTexts[controlToUpdate].text = "--";
        }
        else // first binding
        {
            _displayTexts[controlToUpdate].text =
                _actionReferences[controlToUpdate].action.GetBindingDisplayString(0, InputBinding.DisplayStringOptions.DontIncludeInteractions);
            // indicate no binding rather than empty string
            if (_displayTexts[controlToUpdate].text == "")
                _displayTexts[controlToUpdate].text = "--";
        }
        

        // Delete duplicate bindings
        DuplicateBindingCheck(controlToUpdate, isAlt);

        // so it can actually be used again
        _actionReferences[controlToUpdate].action.Enable();
    }

    /// <summary>
    /// Checks for cases where the player has set the same control for two different functions.
    /// Removes binding on overriden control and removes text accordingly.
    /// </summary>
    private void DuplicateBindingCheck(int remappedControl, bool isAlt)
    {
        string binding = _actionReferences[remappedControl].action
            .GetBindingDisplayString(isAlt ? 1 : 0, InputBinding.DisplayStringOptions.DontIncludeInteractions);

        // check for duplicate binding
        for (int i = 0; i < _actionReferences.Length; i++)
        {
            // check both first and alt bindings
            for(int j = 0; j < 2; j++)
            {
                // check for matched duplicate binding (but make sure it isn't itself)
                if ((i != remappedControl || (j==1) != isAlt ) && _actionReferences[i].action
                    .GetBindingDisplayString(j, InputBinding.DisplayStringOptions.DontIncludeInteractions) == binding)
                {

                    // override any potential overrides and then set a new override to no binding
                    _actionReferences[i].action.RemoveBindingOverride(j);
                    _actionReferences[i].action.ApplyBindingOverride(j, "");

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
            _displayTexts[i].text = 
                _actionReferences[i].action.GetBindingDisplayString(0, InputBinding.DisplayStringOptions.DontIncludeInteractions);

            // indicate no binding rather than empty string
            if (_displayTexts[i].text == "")
                _displayTexts[i].text = "--";

            _altDisplayTexts[i].text =
                _actionReferences[i].action.GetBindingDisplayString(1, InputBinding.DisplayStringOptions.DontIncludeInteractions);

            // indicate no binding rather than empty string
            if (_altDisplayTexts[i].text == "")
                _altDisplayTexts[i].text = "--";
        }
    }
    #endregion
}
