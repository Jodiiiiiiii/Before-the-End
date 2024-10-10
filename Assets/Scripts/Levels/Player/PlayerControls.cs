using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{

    // statically set variable for locking player controls
    // (i.e. during timed events, such as pterodactyl ability when the player cannot do anything)
    public static bool IsPlayerLocked;

    private void Start()
    {
        // ensure locked state doesn't carry between scenes
        IsPlayerLocked = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsPlayerLocked)
        {
            // Player grid movement
            if(!_isPreparingAbility) // when preparing ability, movement inputs are ignored (treated as action inputs later)
                HandleMovementInputs();
            // Swapping between different dino types (only affects ability type)
            HandleDinoSwap();
            // Player ability inputs
            HandleAbilityInputs();
            // Undoing player actions
            HandleUndoInputs();
        }
    }

    #region MOVEMENT
    [Header("Movement")]
    [SerializeField, Tooltip("used to actually cause the player to move")] 
    private Mover _mover;
    [SerializeField, Tooltip("used to apply visual sprite swapping changes to the player")] 
    private SpriteFlipper _objFlipper;
    [SerializeField, Tooltip("x scale (of child sprite object) that corresponds to right facing player")]
    private int _rightScaleX = -1;
    [SerializeField, Tooltip("Time delay between player movements if holding down movement inputs")]
    private float _moveDelay = 0.25f;

    // Controls constants
    private const KeyCode MOVE_UP = KeyCode.W;
    private const KeyCode MOVE_RIGHT = KeyCode.D;
    private const KeyCode MOVE_DOWN = KeyCode.S;
    private const KeyCode MOVE_LEFT = KeyCode.A;

    private List<KeyCode> _moveInputStack = new();

    private float _moveTimer;

    private void HandleMovementInputs()
    {
        // Add new input to start of structure when pressed
        if (Input.GetKeyDown(MOVE_UP))
            _moveInputStack.Insert(0, MOVE_UP);
        else if (Input.GetKeyDown(MOVE_DOWN))
            _moveInputStack.Insert(0, MOVE_DOWN);
        else if (Input.GetKeyDown(MOVE_RIGHT))
            _moveInputStack.Insert(0, MOVE_RIGHT);
        else if (Input.GetKeyDown(MOVE_LEFT))
            _moveInputStack.Insert(0, MOVE_LEFT);

        // Remove any inputs upon release
        for (int i = _moveInputStack.Count - 1; i >= 0; i--)
        {
            if (!Input.GetKey(_moveInputStack[i]))
                _moveInputStack.RemoveAt(i);
        }

        // Process most recent move input (if any)
        if (_moveInputStack.Count > 0)
        {
            if (_moveTimer <= 0)
            {
                if (_moveInputStack[0] == MOVE_UP)
                    PlayerMoveChecks.TryPlayerMove(this, Vector2Int.up);
                else if (_moveInputStack[0] == MOVE_DOWN)
                    PlayerMoveChecks.TryPlayerMove(this, Vector2Int.down);
                else if (_moveInputStack[0] == MOVE_RIGHT)
                    PlayerMoveChecks.TryPlayerMove(this, Vector2Int.right);
                else if (_moveInputStack[0] == MOVE_LEFT)
                    PlayerMoveChecks.TryPlayerMove(this, Vector2Int.left);

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
    private const KeyCode CYCLE_LEFT = KeyCode.Q;
    private const KeyCode CYCLE_RIGHT = KeyCode.E;

    [Header("Dino Swapping")]
    [SerializeField, Tooltip("List of accessible dinosaur types in this level. MUST align with dinoCharges array")]
    private DinoType[] _dinoTypes;
    [SerializeField, Tooltip("Starting ability charges for each dino. -1 indicates infinite. MUST align with dinoTypes array")]
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

    private void HandleDinoSwap()
    {
        // No dino swapping possible if only one dinosaur is in swapping pool
        if (_dinoTypes.Length <= 1)
            return;

        // cycle dino type to one lower
        if(Input.GetKeyDown(CYCLE_LEFT))
        {
            int newIndex = _currDino - 1;
            if (newIndex == -1) // left from index 0 goes to end
                newIndex = _dinoCharges.Length - 1;

            _currDino = newIndex;
            // save action
            UndoHandler.SaveFrame();
        }
        // cycle dino type to one higher
        if(Input.GetKeyDown(CYCLE_RIGHT))
        {
            int newIndex = _currDino + 1;
            if (newIndex == _dinoCharges.Length) // right from final index goes back to 0
                newIndex = 0;

            _currDino = newIndex;
            // save action
            UndoHandler.SaveFrame();
        }
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
    #endregion

    #region ABILITY
    // Inspector Variables
    [Header("Actions")]
    [SerializeField, Tooltip("Script handling indicators for ability")]
    private AbilityIndicatorSprites _abilityIndicator;

    // Controls Constants
    private const KeyCode INITIATE_ACTION = KeyCode.Space;

    private bool _isPreparingAbility = false;

    private void HandleAbilityInputs()
    {
        // required so that cancelling with space doesn't start new ability prepare in the same frame
        bool canceledPrepareThisFrame = false;

        // Ready for directional inputs
        if (_isPreparingAbility)
        {
            // Try ability in appropriate direction and cancel preparing ability state
            if (Input.GetKeyDown(MOVE_UP))
            {
                PlayerAbilityChecks.TryPlayerAbility(this, Vector2Int.up, _dinoTypes[_currDino], _dinoCharges[_currDino]);
                _isPreparingAbility = false;
            }
            else if (Input.GetKeyDown(MOVE_DOWN))
            {
                PlayerAbilityChecks.TryPlayerAbility(this, Vector2Int.down, _dinoTypes[_currDino], _dinoCharges[_currDino]);
                _isPreparingAbility = false;
            }
            else if (Input.GetKeyDown(MOVE_RIGHT))
            {
                PlayerAbilityChecks.TryPlayerAbility(this, Vector2Int.right, _dinoTypes[_currDino], _dinoCharges[_currDino]);
                _isPreparingAbility = false;
            }
            else if (Input.GetKeyDown(MOVE_LEFT))
            {
                PlayerAbilityChecks.TryPlayerAbility(this, Vector2Int.left, _dinoTypes[_currDino], _dinoCharges[_currDino]);
                _isPreparingAbility = false;
            }
            else if (Input.GetKeyDown(INITIATE_ACTION)) // cancel ability if space pressed again
            {
                _isPreparingAbility = false;
                canceledPrepareThisFrame = true;
                // disable indicator
                _abilityIndicator.SetAbilityActive(false);
            }
        }
        else
        {
            // hide directional indicators that show preparing action state
            _abilityIndicator.SetAbilityActive(false);
        }

        // start ability state (no movement queued up && pressing action key)
        if (_moveInputStack.Count == 0 && Input.GetKeyDown(INITIATE_ACTION) 
            && !_isPreparingAbility && !canceledPrepareThisFrame)
        {
            // ensure player has charges remaining
            if(_dinoCharges[_currDino] == 0)
            {
                // TODO: failure sound effect (no visual)
            }
            else
            {
                _isPreparingAbility = true;

                // make directional indicators visible to visually show preparing action state
                _abilityIndicator.SetAbilityActive(true);
            }
        }
    }
    #endregion

    #region UNDO
    // Controls constants
    private const KeyCode UNDO = KeyCode.R;

    [Header("Undo")]
    [SerializeField, Tooltip("delay between first and second undo steps. Longer to prevent accidental double undo")]
    private float _firstUndoDelay = 0.5f;
    [SerializeField, Tooltip("delay between undo steps when undo key is being held")] 
    private float _undoDelay = 0.2f;

    private float _undoTimer = 0f;

    private void HandleUndoInputs()
    {
        // Process undo press
        if(Input.GetKeyDown(UNDO))
        {
            // start/restart delay timer
            _undoTimer = _firstUndoDelay;
            // Undo action
            UndoHandler.UndoFrame();
        }

        // Process holding input
        if(Input.GetKey(UNDO))
        {
            if(_undoTimer < 0) // ready to undo another frame
            {
                // start/restart delay timer
                _undoTimer = _undoDelay;
                // Undo action
                UndoHandler.UndoFrame();
            }

            _undoTimer -= Time.deltaTime;
        }
    }
    #endregion
}
