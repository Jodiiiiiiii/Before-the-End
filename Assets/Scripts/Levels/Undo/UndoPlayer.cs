using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerControls;

/// <summary>
/// Implements SaveStackFrame and UndoStackFrame functions for a player object.
/// </summary>
public class UndoPlayer : UndoHandler
{
    [SerializeField, Tooltip("Used to determine facing direction of player")]
    private PlayerControls _playerControls;

    // local frame, localPosition, facing direction, Dino Type, dino charges, parent transform
    private Stack<(int, Vector2Int, bool, DinoType, int, Transform)> _undoStack = new Stack<(int, Vector2Int, bool, DinoType, int, Transform)>();

    protected override void SaveStackFrame()
    {
        // retrieve new stack frame values
        Vector2Int newPos = _mover.GetLocalGridPos();
        bool newFacing = _playerControls.IsFacingRight();
        DinoType newType = _playerControls.GetCurrDinoType();
        int newCharges = _playerControls.GetCurrAbilityCharge();
        Transform newParent = _mover.transform.parent;

        // No need to compare if new frame is FIRST frame
        if (_undoStack.Count == 0)
            _undoStack.Push((_localFrame, newPos, newFacing, newType, newCharges, newParent));

        // Compare old values to current
        Vector2Int oldPos = _undoStack.Peek().Item2;
        bool oldFacing = _undoStack.Peek().Item3;
        DinoType oldType = _undoStack.Peek().Item4;
        int oldCharges = _undoStack.Peek().Item5;
        Transform oldParent = _undoStack.Peek().Item6;

        // update stack ONLY if any change in THIS object has actually occurred
        if (!newPos.Equals(oldPos) || newFacing != oldFacing || newType != oldType || newCharges != oldCharges || newParent != oldParent)
            _undoStack.Push((_localFrame, newPos, newFacing, newType, newCharges, newParent));
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
            _mover.SetLocalGoal(newPos.x, newPos.y);

            // update player facing direction
            bool newFacing = _undoStack.Peek().Item3;
            _playerControls.SetFacingRight(newFacing);

            // update player dino type
            DinoType newType = _undoStack.Peek().Item4;
            _playerControls.SetDinoType(newType);

            // update player ability charges
            int newCharges = _undoStack.Peek().Item5;
            _playerControls.SetCurrAbilityCharge(newCharges);

            Transform newParent = _undoStack.Peek().Item6;
            _mover.transform.parent = newParent;
        }
    }
}
