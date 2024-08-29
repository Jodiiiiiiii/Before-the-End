using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Controls constants
    private const KeyCode MOVE_UP = KeyCode.W;
    private const KeyCode MOVE_RIGHT = KeyCode.D;
    private const KeyCode MOVE_DOWN = KeyCode.S;
    private const KeyCode MOVE_LEFT = KeyCode.A;

    // TODO: enum storing Dino state

    [Header("Components")]
    [SerializeField, Tooltip("used to actually cause the player to move")] private ObjectMover _objMover;

    private Vector2Int _pos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _pos = _objMover.GoalPos;

        // Movement inputs
        if(_objMover.IsStationary())
        {
            if (Input.GetKey(MOVE_UP))
                TryMoveUp();
            else if (Input.GetKey(MOVE_DOWN))
                TryMoveDown();
            else if (Input.GetKey(MOVE_RIGHT))
                TryMoveRight();
            else if (Input.GetKey(MOVE_LEFT))
                TryMoveLeft();
        }
        
    }

    private void TryMoveRight()
    {
        // Check right one unit for validity
        if(CanMove(new Vector2Int(1, 0)))
            _objMover.GoalPos.x++;
    }

    private void TryMoveLeft()
    {
        // Check left one unit for validity
        if(CanMove(new Vector2Int(-1, 0)))
            _objMover.GoalPos.x--;
    }

    private void TryMoveUp()
    {
        // Check up one unit for validity
        if(CanMove(new Vector2Int(0, 1)))
            _objMover.GoalPos.y++;
    }

    private void TryMoveDown()
    {
        // Check down one unit for validity
        if(CanMove(new Vector2Int(0, -1)))
            _objMover.GoalPos.y--;
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

        Vector2Int targetPos = _objMover.GoalPos + moveDir;

        // Check for visibility of player at current position
        if (!VisibilityCheck.IsVisible(this, _objMover.GoalPos.x, _objMover.GoalPos.y))
            return false;

        // Check for obstruction by higher-ordered panel
        if (!VisibilityCheck.IsVisible(this, targetPos.x, targetPos.y))
            return false;

        // Passed all failure checks, so the player can move here
        return true;
    }
}
