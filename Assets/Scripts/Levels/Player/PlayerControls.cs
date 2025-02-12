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

    #region CONTROLS BINDINGS
    private InputActionAsset _actions;

    private void Start()
    {
        // configuration validation
        if (_dinoCharges.Length != _dinoTypes.Length)
            throw new Exception("Player configuration error: dino charges and types lists must be equal length.");

        // Instantiate compy reference if present
        // having a reference from the start is important so that the undo system still works
        if (_compyReference is null)
        {
            for (int i = 0; i < _dinoTypes.Length; i++)
            {
                if (_dinoTypes[i] == DinoType.Compy)
                {
                    // create reference
                    GameObject compy = Instantiate(_compyPrefab, transform.parent);

                    // assign reference
                    if (!compy.TryGetComponent(out QuantumState compyObj))
                        throw new Exception("Compy prefab MUST have QuantumState component.");
                    _compyReference = compyObj;

                    // ensure disabled by default
                    _compyReference.ObjData.IsDisabled = true;

                    // ensure player alignment before any move action has occurred 
                    StartCoroutine(DoInitCompyPosConfigure());

                    break;
                }
            }
        }

        _actions = InputSystem.actions;

        // move presses (press and release, to handle queuing move commands)
        _actions.actionMaps[0].FindAction("Up").started += UpInput;
        _actions.actionMaps[0].FindAction("Up").canceled += UpInput;
        _actions.actionMaps[0].FindAction("Down").started += DownInput;
        _actions.actionMaps[0].FindAction("Down").canceled += DownInput;
        _actions.actionMaps[0].FindAction("Left").started += LeftInput;
        _actions.actionMaps[0].FindAction("Left").canceled += LeftInput;
        _actions.actionMaps[0].FindAction("Right").started += RightInput;
        _actions.actionMaps[0].FindAction("Right").canceled += RightInput;

        // Undo (press and release, to handle holding to undo)
        _actions.actionMaps[0].FindAction("Undo").started += Undo;
        _actions.actionMaps[0].FindAction("Undo").canceled += Undo;

        // Ability activate/deactivate (only enabled if starting dino has some charges) - useful for locking controls in tutorial
        if(_dinoCharges[0] != 0)
            _actions.actionMaps[0].FindAction("Ability").started += ToggleAbilityActive;

        // Only add swap controls if there are at least two dinos
        if (_dinoTypes.Length > 1)
        {
            _actions.actionMaps[0].FindAction("CycleLeft").started += CycleLeft;
            _actions.actionMaps[0].FindAction("CycleRight").started += CycleRight;

            // assign swap operations (for present dinos)
            _actions.actionMaps[0].FindAction("Swap1").started += Swap1;
            _actions.actionMaps[0].FindAction("Swap2").started += Swap2;
            if (_dinoTypes.Length > 2) _actions.actionMaps[0].FindAction("Swap3").started += Swap3;
            if (_dinoTypes.Length > 3) _actions.actionMaps[0].FindAction("Swap4").started += Swap4;
            if (_dinoTypes.Length > 4) _actions.actionMaps[0].FindAction("Swap5").started += Swap5;
            if (_dinoTypes.Length > 5) _actions.actionMaps[0].FindAction("Swap6").started += Swap6;
            if (_dinoTypes.Length > 6) _actions.actionMaps[0].FindAction("Swap7").started += Swap7;
        }

        // pause
        _actions.actionMaps[0].FindAction("Pause").started += PauseToggle;

        // fading panels
        _actions.actionMaps[0].FindAction("FadePanels").started += FadePanels;
        _actions.actionMaps[0].FindAction("FadePanels").canceled += FadePanels;

        _actions.actionMaps[0].Enable();
    }

    private void OnDisable()
    {
        // remove move input bindings
        _actions.actionMaps[0].FindAction("Up").started -= UpInput;
        _actions.actionMaps[0].FindAction("Up").canceled -= UpInput;
        _actions.actionMaps[0].FindAction("Down").started -= DownInput;
        _actions.actionMaps[0].FindAction("Down").canceled -= DownInput;
        _actions.actionMaps[0].FindAction("Left").started -= LeftInput;
        _actions.actionMaps[0].FindAction("Left").canceled -= LeftInput;
        _actions.actionMaps[0].FindAction("Right").started -= RightInput;
        _actions.actionMaps[0].FindAction("Right").canceled -= RightInput;
        // remove undo bindings
        _actions.actionMaps[0].FindAction("Undo").started -= Undo;
        _actions.actionMaps[0].FindAction("Undo").canceled -= Undo;
        // remove ability toggle binding
        _actions.actionMaps[0].FindAction("Ability").started -= ToggleAbilityActive;
        // remove type cycle bindings
        _actions.actionMaps[0].FindAction("CycleLeft").started -= CycleLeft;
        _actions.actionMaps[0].FindAction("CycleRight").started -= CycleRight;
        // remove swap bindings
        _actions.actionMaps[0].FindAction("Swap1").started -= Swap1;
        _actions.actionMaps[0].FindAction("Swap2").started -= Swap2;
        _actions.actionMaps[0].FindAction("Swap3").started -= Swap3;
        _actions.actionMaps[0].FindAction("Swap4").started -= Swap4;
        _actions.actionMaps[0].FindAction("Swap5").started -= Swap5;
        _actions.actionMaps[0].FindAction("Swap6").started -= Swap6;
        _actions.actionMaps[0].FindAction("Swap7").started -= Swap7;
        // remove pause binding
        _actions.actionMaps[0].FindAction("Pause").started -= PauseToggle;
        // remove fading panels binding
        _actions.actionMaps[0].FindAction("FadePanels").started -= FadePanels;
        _actions.actionMaps[0].FindAction("FadePanels").canceled -= FadePanels;

        // disable player actions altogether
        _actions.actionMaps[0].Disable();
    }
    #endregion

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

        // must have remaining ability uses to prepare to use ability
        if (_dinoCharges[_currDino] == 0)
            return;

        // Compy Swapping: unique case that doesn't use preparing indicator
        if (_dinoTypes[_currDino] == DinoType.Compy && _dinoCharges[_currDino] == -1)
        {
            SwapWithCompy();
        }
        else
        {
            // flip ability preparing state
            _isPreparingAbility = !_isPreparingAbility;

            // Update ability indicator
            _abilityIndicator.SetAbilityActive(_isPreparingAbility);
        }
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
    /// Returns a copy of the list (necessary for undo system to work properly)
    /// </summary>
    public int[] GetAbilityCharges()
    {
        // create copy of charges
        int[] chargesCopy = new int[_dinoCharges.Length];
        for (int i = 0; i < _dinoCharges.Length; i++)
            chargesCopy[i] = _dinoCharges[i];

        return chargesCopy;
    }

    /// <summary>
    /// Used by the undo handler to save ALL charges.
    /// This is needed for cases where two charges change in same frame (i.e. compy collecting)
    /// </summary>
    public void SetAbilityCharges(int[] newCharges)
    {
        if (_dinoCharges.Length != newCharges.Length)
            throw new Exception("New ability charges list MUST be the same length.");

        // update each element
        for (int i = 0; i < newCharges.Length; i++)
            _dinoCharges[i] = newCharges[i];
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

    #region PANEL FADING
    /// <summary>
    /// Handles updating fade panels variable in game manager based on press and release of key binding.
    /// </summary>
    private void FadePanels(InputAction.CallbackContext context)
    {
        // start panel fade
        if (context.started)
            GameManager.Instance.IsFading = true;
        // end panel fade
        else if (context.canceled)
            GameManager.Instance.IsFading = false;
    }
    #endregion

    #region COMPY
    [Header("Compy")]
    [SerializeField, Tooltip("Prefab used to spawn compy object.")]
    private GameObject _compyPrefab;
    [SerializeField, Tooltip("Reference to the compy object in the scene. If this is not assigned, it will be created disabled by default if the player has the compy in this level.")]
    private QuantumState _compyReference = null; // initialized in start IF necessary

    /// <summary>
    /// Spawns compy prefab at adjacent position and saves the compy instance.
    /// </summary>
    public void SpawnCompy(Vector2Int dir)
    {
        // REQUIREMENT: only ONE compy in scene at once (charges should never exceed 1)
        if (_compyReference is null)
            throw new Exception("Compy reference MUST be initialized at start. Why was it not?");

        // ensure compy is moved to the correct panel (same panel as player using ability)
        _compyReference.transform.parent = transform.parent;

        // flip pair to match current facing direction of player
        if (_compyReference.transform.GetChild(0) is null || !_compyReference.transform.GetChild(0).TryGetComponent(out SpriteFlipper compyFlipper))
            throw new System.Exception("Invalid Compy Pair Object: MUST have a SpriteFlipper on the first child.");
        compyFlipper.SetScaleX(IsFacingRight() ? _rightScaleX : -_rightScaleX);

        // Determine spawn pos
        Vector2Int spawnPos = _mover.GetGlobalGridPos() + dir;

        // Update compy reference position
        if (!_compyReference.TryGetComponent(out Mover compyMover))
            throw new Exception("Compy Prefab/Reference MUST have Mover component.");
        compyMover.SetGlobalGoal(spawnPos.x, spawnPos.y);

        // enable compy object
        _compyReference.ObjData.IsDisabled = false;

        // set ability charge to infinite now that it is placed (infinite swapping)
        if (_dinoTypes[_currDino] != DinoType.Compy)
            throw new Exception("Spawning Compy while player is a different dinosaur should be impossible. something is wrong.");
        _dinoCharges[_currDino] = -1;

        // no need to save, this is handled in PlayerAbilityChecks
    }

    /// <summary>
    /// Handles restoring an ability charge AND destroying the compy object and reference.
    /// </summary>
    public void CollectCompy()
    {
        // find which index is the compy
        int compyIndex = -1;
        for (int i = _dinoTypes.Length - 1; i >= 0; i--)
        {
            if (_dinoTypes[i] == DinoType.Compy)
            {
                compyIndex = i;
                break;
            }
        }
        if (compyIndex == -1)
            throw new Exception("How is the player collecting a compy when they don't even have the compy on this level?");

        // restore charge to place another compy pair down
        _dinoCharges[compyIndex] = 1;

        // destroy reference since it has been collected
        _compyReference.ObjData.IsDisabled = true;

        // visually flip player sprite - only flip if current dino is compy, otherwise it looks weird
        if (_dinoTypes[_currDino] == DinoType.Compy)
        {
            PlayerSpriteSwapper flipper = GetComponentInChildren<PlayerSpriteSwapper>();
            if (flipper is null)
                throw new Exception("Player must have PlayerSpriteSwapper component on one of its children.");
            flipper.RequireFlip();
        }

        // does not save frame here since frame saving is handled where player movement is handled
    }

    /// <summary>
    /// Swaps positions (and parent transform) of player and compy.
    /// </summary>
    public void SwapWithCompy()
    {
        // CANNOT swap if compy reference is null or disabled
        if (_compyReference is null || _compyReference.ObjData.IsDisabled)
            throw new Exception("Cannot swap with compy if reference is null or disabled. This should not have been called.");

        if (!_compyReference.TryGetComponent(out Mover compyMover))
            throw new Exception("Compy prefab/reference MUST have Mover component.");

        // require compy to be visible in order to swap
        Vector2Int compyPos = compyMover.GetGlobalGridPos();
        if (!VisibilityChecks.IsVisible(_compyReference.gameObject, compyPos.x, compyPos.y))
        {
            // TODO: failure effect at location of obstructed compy (compyPos)

            return;
        }

        // temporary swapping data
        Transform compyParent = _compyReference.transform.parent;
        if (_compyReference.transform.GetChild(0) is null || !_compyReference.transform.GetChild(0).TryGetComponent(out SpriteFlipper compyFlipper))
            throw new System.Exception("Invalid Compy Pair Object: MUST have a SpriteFlipper on the first child.");
        int compyScaleX = compyFlipper.GetGoalScaleX();

        // update compy
        _compyReference.transform.parent = transform.parent;
        compyMover.SetGlobalGoal(_mover.GetGlobalGridPos().x, _mover.GetGlobalGridPos().y);
        compyMover.SnapToGoal(); // prevent appearance of swapping/sliding
        compyFlipper.SetScaleX(IsFacingRight() ? _rightScaleX : -_rightScaleX); // set compy to player facing
        compyFlipper.SnapToGoal(); // prevent horizontal flipping effect when change occurs

        // update player
        transform.parent = compyParent;
        _mover.SetGlobalGoal(compyPos.x, compyPos.y);
        _mover.SnapToGoal(); // prevent appearance of swapping/sliding
        SetFacingRight(compyScaleX == _rightScaleX); // set player to compy facing
        _objFlipper.SnapToGoal(); // prevent horizontal flipping effect when change occurs

        // swapping counts as an action so frame must be saved
        UndoHandler.SaveFrame();
    }

    /// <summary>
    /// Returns whether or not the compy is currently split between two pairs.
    /// </summary>
    public bool IsCompySplit()
    {
        int compyIndex = -1;
        for (int i = 0; i < _dinoTypes.Length; i++)
        {
            if (_dinoTypes[i] == DinoType.Compy)
            {
                compyIndex = i;
                break;
            }
        }

        // compy is NOT split since there is no compy ability in play
        if (compyIndex == -1)
            return false;

        // true split state only if current compy charges are -1
        return _dinoCharges[compyIndex] == -1;
    }

    /// <summary>
    /// Ensures compy is position aligned to the player every frame in case it is placed (so that it comes out of the player AND undoes without position weirdness).
    /// </summary>
    public void SnapInactiveCompyToPlayer()
    {
        // only conduct snapping if compy pair is inactive (not yet placed)
        if (!IsCompySplit())
        {
            Vector2Int playerPos = _mover.GetGlobalGridPos();
            _compyReference.ObjMover.SetGlobalGoal(playerPos.x, playerPos.y);
            _compyReference.ObjMover.SnapToGoal();
        }
    }

    /// <summary>
    /// To be called in start - but the delay ensures that the initial position is accurate to the configured player pos in start of Mover.cs
    /// </summary>
    private IEnumerator DoInitCompyPosConfigure()
    {
        yield return new WaitForSeconds(0.1f);
        SnapInactiveCompyToPlayer();
    }
    #endregion

    #region SCENE TRANSITION
    [Header("Scene Transitions")]
    [SerializeField, Tooltip("Used to call scene transitions with animations.")]
    private SceneTransitionHandler _transitionHandler;
    [SerializeField, Tooltip("Scene name of level select.")]
    private string _levelSelectSceneName;

    /// <summary>
    /// Handles logging of current level as complete.
    /// </summary>
    public void LevelComplete()
    {
        // mark level as complete for progression
        GameManager.Instance.LevelComplete();

        // laod scene
        _transitionHandler.LoadScene(_levelSelectSceneName);
    }
    #endregion
}