using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{

    // statically set variable for locking player controls (i.e. during panel dragging or timed actions)
    public static bool IsPlayerLocked = false;

    // Update is called once per frame
    void Update()
    {
        if(!IsPlayerLocked)
        {
            // Player grid movement
            if(!_isPreparingAbility) // when preparing ability, movement inputs are ignored (treated as action inputs later)
                HandleMovementInputs();
            // Player ability inputs
            HandleAbilityInputs();
            // Undoing player actions
            HandleUndoInputs();
        }
    }

    #region MOVEMENT
    [Header("Movement")]
    [SerializeField, Tooltip("used to actually cause the player to move")] 
    private ObjectMover _objMover;
    [SerializeField, Tooltip("used to apply visual sprite swapping changes to the player")] 
    private ObjectFlipper _objFlipper;
    [SerializeField, Tooltip("x scale (of child sprite object) that corresponds to right facing player")]
    private int _rightScaleX = -1;

    // Controls constants
    private const KeyCode MOVE_UP = KeyCode.W;
    private const KeyCode MOVE_RIGHT = KeyCode.D;
    private const KeyCode MOVE_DOWN = KeyCode.S;
    private const KeyCode MOVE_LEFT = KeyCode.A;

    private List<KeyCode> _moveInputStack = new();

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
        if (_objMover.IsStationary() && _moveInputStack.Count > 0)
        {
            if (_moveInputStack[0] == MOVE_UP)
                TryMoveUp();
            else if (_moveInputStack[0] == MOVE_DOWN)
                TryMoveDown();
            else if (_moveInputStack[0] == MOVE_RIGHT)
                TryMoveRight();
            else if (_moveInputStack[0] == MOVE_LEFT)
                TryMoveLeft();
        }
    }

    /// <summary>
    /// Handles flipping the sprite, checking for a valid right move, and moving the player.
    /// </summary>
    private void TryMoveRight()
    {
        bool hasChanged = false;

        // flip even if no movement occurs (indicates attempt) -> still requires player visibility
        Vector2Int currPos = _objMover.GetGlobalGridPos();
        // if previouslt facing left (AND visible)
        if (_objFlipper.GetScaleX() == -_rightScaleX && VisibilityCheck.IsVisible(this, currPos.x, currPos.y))
        {
            _objFlipper.SetScaleX(_rightScaleX);
            hasChanged = true;
        }

        // Check right one unit for validity
        if (MovementCheck.CanPlayerMove(this, Vector2Int.right))
        {
            _objMover.Increment(Vector2Int.right);
            hasChanged = true;
        }

        // save frame as long as visible change occurred
        if(hasChanged)
            UndoHandler.SaveFrame();
    }

    /// <summary>
    /// handles flipping the sprite, checking for a valid left move, and moving the player.
    /// </summary>
    private void TryMoveLeft()
    {
        bool hasChanged = false;

        // flip even if no movement occurs (indicates attempt) -> still requires player visbility
        Vector2Int currPos = _objMover.GetGlobalGridPos();
        // if previously facing right (AND visible)
        if (_objFlipper.GetScaleX() == _rightScaleX && VisibilityCheck.IsVisible(this, currPos.x, currPos.y))
        {
            _objFlipper.SetScaleX(-_rightScaleX); // faces opposite to right dir
            hasChanged = true;
        }

        // Check left one unit for validity
        if (MovementCheck.CanPlayerMove(this, Vector2Int.left))
        {
            _objMover.Increment(Vector2Int.left);
            hasChanged = true;
        }

        // save frame as long as scale was flipped (visible change)
        if(hasChanged)
            UndoHandler.SaveFrame();
        
    }

    /// <summary>
    /// handles checking for a valid upwards move, and moving the player.
    /// </summary>
    private void TryMoveUp()
    {
        // Check up one unit for validity
        if(MovementCheck.CanPlayerMove(this, Vector2Int.up))
        {
            _objMover.Increment(Vector2Int.up);
            UndoHandler.SaveFrame();
        }
    }

    /// <summary>
    /// handles checking for a valid downwards move, and moving the player.
    /// </summary>
    private void TryMoveDown()
    {
        // Check down one unit for validity
        if (MovementCheck.CanPlayerMove(this, Vector2Int.down))
        {
            _objMover.Increment(Vector2Int.down);
            UndoHandler.SaveFrame();
        }
    }

    /// <summary>
    /// returns a boolean of whether the player is currently facing right or left
    /// </summary>
    public bool IsFacingRight()
    {
        if (_objFlipper.GetScaleX() == _rightScaleX)
            return true;

        if (_objFlipper.GetScaleX() == -_rightScaleX)
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

    #region ACTIONS
    // Inspector Variables
    [Header("Actions")]
    [SerializeField, Tooltip("Animated game object indicators for ability")]
    private GameObject _abilityIndicator;

    // Controls Constants
    private const KeyCode INITIATE_ACTION = KeyCode.Space;

    private bool _isPreparingAbility = false;

    public enum DinoType
    {
        Stego,
        Trike,
        Anky,
        Dilo,
        Bary,
        Ptero,
        Compy,
        Pachy
    }
    private DinoType _dinoType = DinoType.Stego;

    // TODO: will need to make a list structure for storing charges of a particular dinoType ability remaining (if limited?)

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
                TryAbility(Vector2Int.up);
                _isPreparingAbility = false;
            }
            else if (Input.GetKeyDown(MOVE_DOWN))
            {
                TryAbility(Vector2Int.down);
                _isPreparingAbility = false;
            }
            else if (Input.GetKeyDown(MOVE_RIGHT))
            {
                TryAbility(Vector2Int.right);
                _isPreparingAbility = false;
            }
            else if (Input.GetKeyDown(MOVE_LEFT))
            {
                TryAbility(Vector2Int.left);
                _isPreparingAbility = false;
            }
            else if (Input.GetKeyDown(INITIATE_ACTION)) // cancel ability if space pressed again
            {
                _isPreparingAbility = false;
                canceledPrepareThisFrame = true;
                // disable indicator
                _abilityIndicator.SetActive(false);
            }
        }
        else
        {
            // hide directional indicators that show preparing action state
            _abilityIndicator.SetActive(false);
        }

        // start ability state (stationary && no movement queued up && pressing action key)
        if (_objMover.IsStationary() && _moveInputStack.Count == 0 && Input.GetKeyDown(INITIATE_ACTION) 
            && !_isPreparingAbility && !canceledPrepareThisFrame)
        {
            _isPreparingAbility = true;

            // make directional indicators visible to visually show preparing action state
            _abilityIndicator.SetActive(true);
        }
    }

    private void TryAbility(Vector2Int dir)
    {
        // do ability check depending on current dinosaur type
        switch (_dinoType)
        {
            case DinoType.Stego:
                // Check for object at indicated direction of ability
                Vector2Int abilityPos = _objMover.GetGlobalGridPos() + dir;
                ObjectState adjacentObj = MovementCheck.GetObjectAtPos(this, abilityPos.x, abilityPos.y);
                if(adjacentObj is not null)
                {
                    // mark object as quantum (or unmark)
                    adjacentObj.ToggleQuantum();
                    // action successful (save undo frame)
                    UndoHandler.SaveFrame();
                }
                else
                {
                    // play ability failure sound effect
                }

                break;
            case DinoType.Trike:
                break;
            case DinoType.Anky:
                break;
            case DinoType.Dilo:
                break;
            case DinoType.Bary:
                break;
            case DinoType.Ptero:
                break;
            case DinoType.Compy:
                break;
            case DinoType.Pachy:
                break;
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
