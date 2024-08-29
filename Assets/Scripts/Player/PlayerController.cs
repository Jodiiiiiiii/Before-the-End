using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // TODO: enum storing Dino state

    // Update is called once per frame
    void Update()
    {
        HandleMovementInputs();
    }

    #region MOVEMENT
    [Header("Movement")]
    [SerializeField, Tooltip("used to actually cause the player to move")] private ObjectMover _objMover;

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

    private void TryMoveRight()
    {
        // Check right one unit for validity
        if(CanMove(new Vector2Int(1, 0)))
            _objMover.GridPos.x++;
    }

    private void TryMoveLeft()
    {
        // Check left one unit for validity
        if(CanMove(new Vector2Int(-1, 0)))
            _objMover.GridPos.x--;
    }

    private void TryMoveUp()
    {
        // Check up one unit for validity
        if(CanMove(new Vector2Int(0, 1)))
            _objMover.GridPos.y++;
    }

    private void TryMoveDown()
    {
        // Check down one unit for validity
        if(CanMove(new Vector2Int(0, -1)))
            _objMover.GridPos.y--;
    }

    /// <summary>
    /// Determines if the player is able to move in the specified direction.
    /// Input MUST have only one non-zero value and it must be either -1 or 1.
    /// </summary>
    private bool CanMove(Vector2Int moveDir)
    {
        // Ensure target position is validly only one unit away
        if (moveDir.magnitude != 1 || (moveDir.x != 1 && moveDir.x != -1 && moveDir.x != 0) || (moveDir.y != 1 && moveDir.y != -1 && moveDir.y != 0))
            throw new Exception("Input of CanMove function MUST have only one non-zero value and it must be eiether -1 or 1.");

        Vector2Int targetPos = _objMover.GridPos + moveDir;

        // Check for visibility of player at current position
        if (!VisibilityCheck.IsVisible(this, _objMover.GridPos.x, _objMover.GridPos.y))
            return false;

        // Check for obstruction by higher-ordered panel
        if (!VisibilityCheck.IsVisible(this, targetPos.x, targetPos.y))
            return false;

        // Passed all failure checks, so the player can move here
        return true;
    }
    #endregion
}
