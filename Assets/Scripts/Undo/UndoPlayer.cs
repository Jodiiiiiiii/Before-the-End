using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implements SaveStackFrame and UndoStackFrame functions for a player object.
/// </summary>
public class UndoPlayer : UndoHandler
{
    [SerializeField, Tooltip("Used to determine facing direction of player")]
    private PlayerControls _playerControls;

    // local frame, localPosition, facing direction
    private Stack<(int, Vector2Int, bool)> _undoStack = new Stack<(int, Vector2Int, bool)>();

    protected override void SaveStackFrame()
    {
        // retrieve new stack frame values
        Vector2Int newPos = _objectMover.GetLocalGridPos();
        bool newFacing = _playerControls.IsFacingRight();

        // No need to compare if new frame is FIRST frame
        if (_undoStack.Count == 0)
            _undoStack.Push((_localFrame, newPos, newFacing));

        // Compare old values to current
        Vector2Int oldPos = _undoStack.Peek().Item2;
        bool oldFacing = _undoStack.Peek().Item3;
        // update stack ONLY if any change in THIS object has actually occurred
        if (!newPos.Equals(oldPos) || newFacing != oldFacing)
            _undoStack.Push((_localFrame, newPos, newFacing));
    }

    protected override void UndoStackFrame()
    {
        // CANNOT undo past first move, therefore count of at least 2 is required to undo
        // Remove current position (if a change actually was registered before)
        if (_undoStack.Count > 1 && _undoStack.Peek().Item1 > _localFrame)
        {
            // Remove current stack frame (no longer needed)
            _undoStack.Pop();

            // update actual panel position
            Vector2Int newPos = _undoStack.Peek().Item2;
            _objectMover.SetLocalGoal(newPos.x, newPos.y);

            // update player facing direction
            bool newFacing = _undoStack.Peek().Item3;
            _playerControls.SetFacingRight(newFacing);
        }
    }
}
