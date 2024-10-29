using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    // State variables
    [HideInInspector]
    public bool IsSwimming = false;

    private PlayerInputActions actions;

    private void Start()
    {
        // configuration validation
        if (_dinoCharges.Length != _dinoTypes.Length)
            throw new Exception("Player configuration error: dino charges and types lists must be equal length.");

        actions = new PlayerInputActions();

        // move presses (press and release, to handle queuing move commands)
        actions.Player.Up.started += UpInput;
        actions.Player.Up.canceled += UpInput;
        actions.Player.Down.started += DownInput;
        actions.Player.Down.canceled += DownInput;
        actions.Player.Left.started += LeftInput;
        actions.Player.Left.canceled += LeftInput;
        actions.Player.Right.started += RightInput;
        actions.Player.Right.canceled += RightInput;

        // Undo (press and release, to handle holding to undo)
        actions.Player.Undo.started += Undo;
        actions.Player.Undo.canceled += Undo;

        // Ability activate/deactivate (only enabled if starting dino has some charges) - useful for locking controls in tutorial
        if(_dinoCharges[0] != 0)
            actions.Player.Ability.started += ToggleAbilityActive;

        // Only add swap controls if there are at least two dinos
        if (_dinoTypes.Length > 1)
        {
            actions.Player.CycleLeft.started += CycleLeft;
            actions.Player.CycleRight.started += CycleRight;

            // assign swap operations (for present dinos)
            actions.Player.Swap1.started += Swap1;
            actions.Player.Swap2.started += Swap2;
            if (_dinoTypes.Length > 2) actions.Player.Swap3.started += Swap3;
            if (_dinoTypes.Length > 3) actions.Player.Swap4.started += Swap4;
            if (_dinoTypes.Length > 4) actions.Player.Swap5.started += Swap5;
            if (_dinoTypes.Length > 5) actions.Player.Swap6.started += Swap6;
            if (_dinoTypes.Length > 6) actions.Player.Swap7.started += Swap7;
        }

        actions.Player.Pause.started += PauseToggle;

        actions.Player.Enable();
    }

    private void OnDisable()
    {
        // remove move input bindings
        actions.Player.Up.started -= UpInput;
        actions.Player.Up.canceled -= UpInput;
        actions.Player.Down.started -= DownInput;
        actions.Player.Down.canceled -= DownInput;
        actions.Player.Left.started -= LeftInput;
        actions.Player.Left.canceled -= LeftInput;
        actions.Player.Right.started -= RightInput;
        actions.Player.Right.canceled -= RightInput;
        actions.Player.Swap7.started -= Swap7;
        // remove undo bindings
        actions.Player.Undo.started += Undo;
        actions.Player.Undo.canceled += Undo;
        // remove ability toggle binding
        actions.Player.Ability.started += ToggleAbilityActive;
        // remove type cycle bindings
        actions.Player.CycleLeft.started += CycleLeft;
        actions.Player.CycleRight.started += CycleRight;
        // remove swap bindings
        actions.Player.Swap1.started += Swap1;
        actions.Player.Swap2.started += Swap2;
        actions.Player.Swap3.started += Swap3;
        actions.Player.Swap4.started += Swap4;
        actions.Player.Swap5.started += Swap5;
        actions.Player.Swap6.started += Swap6;
        actions.Player.Swap7.started += Swap7;
        // remove pause binding
        actions.Player.Pause.started -= PauseToggle;

        // disable player actions altogether
        actions.Player.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        // Undoing player actions
        HandleHoldingUndo();

        // Player grid movement
        // avoid moving while ability preparing OR while undoing (can cause clipping)
        if (!_isPreparingAbility && !_isUndoing)
            HandlePlayerMovement();
    }

    #region MOVEMENT
    [Header("Movement")]
    [SerializeField, Tooltip("used to actually cause the player to move.")] 
    private Mover _mover;
    [SerializeField, Tooltip("used to apply visual sprite swapping changes to the player.")] 
    private SpriteFlipper _objFlipper;
    [SerializeField, Tooltip("x scale (of child sprite object) that corresponds to right facing player.")]
    private int _rightScaleX = -1;
    [SerializeField, Tooltip("Time delay between player movements if holding down movement inputs.")]
    private float _moveDelay = 0.25f;
    [SerializeField, Tooltip("Delay after a move input is unregistered. " +
        "Brief window to allow the player to come to a stop and move in the tapped direction before it is removed from the list.")]
    private float _releaseDelay = 0.2f;

    private List<Vector2Int> _moveDirectionQueue = new();

    private float _moveTimer;

    /// <summary>
    /// Handles upward input used for movement or ability activation. 
    /// Called on both button press and release to handle queuing and unqueuing movement commands.
    /// </summary>
    private void UpInput(InputAction.CallbackContext context)
    {
        ProcessDirectionalInput(Vector2Int.up, context.started, context.canceled);
    }

    /// <summary>
    /// Handles downward input used for movement or ability activation. 
    /// Called on both button press and release to handle queuing and unqueuing movement commands.
    /// </summary>
    private void DownInput(InputAction.CallbackContext context)
    {
        ProcessDirectionalInput(Vector2Int.down, context.started, context.canceled);
    }

    /// <summary>
    /// Handles left input used for movement or ability activation. 
    /// Called on both button press and release to handle queuing and unqueuing movement commands.
    /// </summary>
    private void LeftInput(InputAction.CallbackContext context)
    {
        ProcessDirectionalInput(Vector2Int.left, context.started, context.canceled);
    }

    /// <summary>
    /// Handles right input used for movement or ability activation. 
    /// Called on both button press and release to handle queuing and unqueuing movement commands.
    /// </summary>
    private void RightInput(InputAction.CallbackContext context)
    {
        ProcessDirectionalInput(Vector2Int.right, context.started, context.canceled);
    }

    /// <summary>
    /// Handles logic for processing directional input, including queuing movement, unqueuing movement, and attempting ability.
    /// </summary>
    private void ProcessDirectionalInput(Vector2Int dir, bool started, bool canceled)
    {
        if (started)
        {
            if (_isPreparingAbility)
                AttemptAbility(dir);
            else // movement
                QueueMove(dir);
        }
        else if (canceled)
        {
            StartCoroutine(UnqueueMove(dir));
        }
    }

    /// <summary>
    /// adds movement direction to the front of the move direction queue.
    /// </summary>
    private void QueueMove(Vector2Int dir)
    {
        // reset move timer on a new button press
        _moveTimer = 0;

        _moveDirectionQueue.Insert(0, dir);
    }

    /// <summary>
    /// removes movement direction from the back of the move direction queue, after a delay.
    /// </summary>
    private IEnumerator UnqueueMove(Vector2Int dir)
    {
        // delay
        yield return new WaitForSeconds(_releaseDelay);

        // removes the latest instance of the input (leaving potential more recent presses)
        for (int i = _moveDirectionQueue.Count - 1; i >= 0; i--)
        {
            // only remove first found item from the right
            if (_moveDirectionQueue[i] == dir)
            {
                _moveDirectionQueue.RemoveAt(i);
                break;
            }
        }
    }

    /// <summary>
    /// Handles calls to move the player based on generated move input queue
    /// </summary>
    private void HandlePlayerMovement()
    {
        // Don't process movement inputs while paused (they can still be queued though)
        if (GameManager.Instance.IsPaused)
            return;

        // Process most recent move input (if any)
        if (_moveDirectionQueue.Count > 0)
        {
            // ready to move again, and player not currently moving
            // ensures smoothness of movement
            if (_moveTimer <= 0 && _mover.IsStationary())
            {
                PlayerMoveChecks.TryPlayerMove(this, _moveDirectionQueue[0]);

                // reset moveTimer for next input.
                // Resets even if move fails to have any impact - accounts for action such as panel pushing when player doesn't move.
                _moveTimer = _moveDelay;
            }
            else
                _moveTimer -= Time.deltaTime;
        }
        else
            _moveTimer = 0; // no delay on first input
    }

    /// <summary>
    /// returns a boolean of whether the player is currently facing right or left
    /// </summary>
    public bool IsFacingRight()
    {
        if (_objFlipper.GetGoalScaleX() == _rightScaleX)
            return true;

        if (_objFlipper.GetGoalScaleX() == -_rightScaleX)
            return false;

        throw new Exception("Player has invalid facing direction, must be an xScale of 1 or -1. How did this happen?");
    }

    /// <summary>
    /// updates visual facing direction of player. 
    /// true = right; false = left
    /// </summary>
    public void SetFacingRight(bool facing)
    {
        if (facing) // right
            _objFlipper.SetScaleX(_rightScaleX);
        else
            _objFlipper.SetScaleX(-_rightScaleX);
    }
    #endregion

    #region SWAP DINO
    [Header("Dino Swapping")]
    [SerializeField, Tooltip("List of accessible dinosaur types in this level. MUST align with dinoCharges array.")]
    private DinoType[] _dinoTypes;
    [SerializeField, Tooltip("Starting ability charges for each dino. -1 indicates infinite. MUST align with dinoTypes array.")]
    private int[] _dinoCharges;

    public enum DinoType
    {
        Stego,
        Trike,
        Anky,
        Spino,
        Ptera,
        Pyro,
        Compy
    }
    // corresponds to index of dinoTypes and dinoCharges arrays
    private int _currDino = 0; // default 0 = first dino (probably always stego)

    /// <summary>
    /// Swaps to the next dinosaur type (to the left). Only called on button press.
    /// </summary>
    private void CycleLeft(InputAction.CallbackContext context)
    {
        // determine left cycle index
        int index = _currDino - 1;
        if (index < 0) index = _dinoTypes.Length - 1;

        SwapToIndex(index);
    }

    /// <summary>
    /// Swaps to the next dinosaur type (to the right). Only called on button press.
    /// </summary>
    private void CycleRight(InputAction.CallbackContext context)
    {
        // determine right cycle index
        int index = _currDino + 1;
        if (index > _dinoCharges.Length - 1) index = 0;

        SwapToIndex(index);
    }

    /// <summary>
    /// Swaps to first dinosaur type. Only called on button press.
    /// </summary>
    private void Swap1(InputAction.CallbackContext context)
    {
        SwapToIndex(0);
    }

    /// <summary>
    /// Swaps to second dinosaur type. Only called on button press.
    /// </summary>
    private void Swap2(InputAction.CallbackContext context)
    {
        SwapToIndex(1);
    }

    /// <summary>
    /// Swaps to third dinosaur type. Only called on button press.
    /// </summary>
    private void Swap3(InputAction.CallbackContext context)
    {
        SwapToIndex(2);
    }

    /// <summary>
    /// Swaps to fourth dinosaur type. Only called on button press.
    /// </summary>
    private void Swap4(InputAction.CallbackContext context)
    {
        SwapToIndex(3);
    }

    /// <summary>
    /// Swaps to fifth dinosaur type. Only called on button press.
    /// </summary>
    private void Swap5(InputAction.CallbackContext context)
    {
        SwapToIndex(4);
    }

    /// <summary>
    /// Swaps to sixth dinosaur type. Only called on button press.
    /// </summary>
    private void Swap6(InputAction.CallbackContext context)
    {
        SwapToIndex(5);
    }

    /// <summary>
    /// Swaps to seventh dinosaur type. Only called on button press.
    /// </summary>
    private void Swap7(InputAction.CallbackContext context)
    {
        SwapToIndex(6);
    }

    /// <summary>
    /// Handles dino swapping logic used by all specific swap functions.
    /// </summary>
    private void SwapToIndex(int index)
    {
        // Skip input processing when paused
        if (GameManager.Instance.IsPaused)
            return;

        // CANNOT swap dino type while swimming (must leave water first)
        if (IsSwimming)
            return;

        // cannot swap dino type during undo
        if (_isUndoing)
            return;

        // update index
        _currDino = index;

        // save action
        UndoHandler.SaveFrame();
    }

    /// <summary>
    /// Determines and returns current dinosaur type of the player.
    /// Useful for sprite swapper and undo system.
    /// </summary>
    public DinoType GetCurrDinoType()
    {
        return _dinoTypes[_currDino];
    }

    /// <summary>
    /// Updates player's stored dino type (will impact abilities and visual appearance).
    /// </summary>
    public void SetDinoType(DinoType type)
    {
        // find index with the input dino type
        for (int i = 0; i < _dinoTypes.Length; i++)
        {
            if (_dinoTypes[i] == type)
            {
                _currDino = i;
                break;
            }
        }
    }
    #endregion

    #region ABILITY
    // Inspector Variables
    [Header("Actions")]
    [SerializeField, Tooltip("Script handling indicators for ability.")]
    private AbilityIndicatorSprites _abilityIndicator;

    private bool _isPreparingAbility = false;

    /// <summary>
    /// Either enters ability preparing state or exits it. Only called on button press.
    /// </summary>
    private void ToggleAbilityActive(InputAction.CallbackContext context)
    {
        // Skip input processing when paused
        if (GameManager.Instance.IsPaused)
            return;

        // flip ability preparing state
        _isPreparingAbility = !_isPreparingAbility;

        // Update ability indicator
        _abilityIndicator.SetAbilityActive(_isPreparingAbility);
    }

    /// <summary>
    /// Contains logic for attempting ability in given direction.
    /// Checks for remaining charges of the ability.
    /// </summary>
    private void AttemptAbility(Vector2Int dir)
    {
        // Don't use ability while paused
        if (GameManager.Instance.IsPaused)
            return;

        // ensure player has charges remaining
        if (_dinoCharges[_currDino] == 0)
        {
            // TODO: failure sound effect (no visual)

            return;
        }

        // Attempt ability functionality
        PlayerAbilityChecks.TryPlayerAbility(this, dir, _dinoTypes[_currDino], _dinoCharges[_currDino]);
        
        // exit preparing ability state
        _isPreparingAbility = false;
        _abilityIndicator.SetAbilityActive(false);
    }

    /// <summary>
    /// decrements charge counter for the current dinosaur type.
    /// </summary>
    public void UseAbilityCharge()
    {
        _dinoCharges[_currDino]--;
    }

    /// <summary>
    /// Gets number of ability charges of current dinosaur's ability.
    /// </summary>
    public int GetCurrAbilityCharge()
    {
        return _dinoCharges[_currDino];
    }

    /// <summary>
    /// Sets number of ability charges of current dinosaur's ability.
    /// </summary>
    public void SetCurrAbilityCharge(int newCharges)
    {
        _dinoCharges[_currDino] = newCharges;
    }

    /// <summary>
    /// Returns current dinosaur types the player may use.
    /// </summary>
    public DinoType[] GetDinoTypes()
    {
        return _dinoTypes;
    }

    /// <summary>
    /// Returns current dinosaur ability charges the player has left.
    /// A value of -1 indicates infinite uses.
    /// </summary>
    public int[] GetAbilityCharges()
    {
        return _dinoCharges;
    }

    /// <summary>
    /// Returns current dino type's index in the dinoTypes list
    /// </summary>
    public int GetCurrDinoIndex()
    {
        return _currDino;
    }
    #endregion

    #region UNDO
    [Header("Undo")]
    [SerializeField, Tooltip("delay between first and second undo steps. Longer to prevent accidental double undo.")]
    private float _firstUndoDelay = 0.5f;
    [SerializeField, Tooltip("delay between undo steps when undo key is being held.")] 
    private float _undoDelay = 0.2f;

    private float _undoTimer = 0f;
    private bool _isUndoing = false;

    /// <summary>
    /// Handles undoing on one frame on button press, and cancelling the hold state on button release.
    /// </summary>
    private void Undo(InputAction.CallbackContext context)
    {
        // start undo
        if (context.started)
        {
            // start holding state
            _isUndoing = true;

            // Skip undo operation (but still save undoing state in case of unpause)
            if (GameManager.Instance.IsPaused)
                return;

            // cancel ability preparation
            _isPreparingAbility = false;
            _abilityIndicator.SetAbilityActive(false);

            // start/restart delay timer
            _undoTimer = _firstUndoDelay;
            // Undo action
            UndoHandler.UndoFrame();
        }

        // release undo
        if (context.canceled)
            _isUndoing = false;
    }

    /// <summary>
    /// Handles repeatedly undoing after delay intervals, assuming the undo key is still held.
    /// </summary>
    private void HandleHoldingUndo()
    {
        // Don't undo while paused
        if (GameManager.Instance.IsPaused)
            return;

        // Undo is being held 
        if (_isUndoing)
        {
            if (_undoTimer < 0) // ready to undo another frame
            {
                // start/restart delay timer
                _undoTimer = _undoDelay;
                // Undo action
                UndoHandler.UndoFrame();
            }
            else
                _undoTimer -= Time.deltaTime;
        }
    }
    #endregion

    #region PAUSE
    /// <summary>
    /// Interfaces with the game manager to toggle the pause state of the game
    /// </summary>
    private void PauseToggle(InputAction.CallbackContext context)
    {
        // flip paused state
        GameManager.Instance.IsPaused = !GameManager.Instance.IsPaused;

        // also cancel ability preparing so the UI stops blinking
        _isPreparingAbility = false;
        _abilityIndicator.SetAbilityActive(false);
    }
    #endregion
}