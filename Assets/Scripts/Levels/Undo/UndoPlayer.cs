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

    // local frame, localPosition, facing direction, Dino Type, dino charges, parent transform, swimming state
    private Stack<(int, Vector2Int, bool, DinoType, int[], Transform, bool)> _undoStack = 
        new Stack<(int, Vector2Int, bool, DinoType, int[], Transform, bool)>();

    protected override void SaveStackFrame()
    {
        // retrieve new stack frame values
        Vector2Int newPos = _mover.GetLocalGridPos();
        bool newFacing = _playerControls.IsFacingRight();
        DinoType newType = _playerControls.GetCurrDinoType();
        int[] newCharges = _playerControls.GetAbilityCharges();
        Transform newParent = _mover.transform.parent;
        bool newSwimming = _playerControls.IsSwimming;
        
        // No need to compare if new frame is FIRST frame
        if (_undoStack.Count == 0)
            _undoStack.Push((_localFrame, newPos, newFacing, newType, newCharges, newParent, newSwimming));

        // Compare old values to current
        Vector2Int oldPos = _undoStack.Peek().Item2;
        bool oldFacing = _undoStack.Peek().Item3;
        DinoType oldType = _undoStack.Peek().Item4;
        int[] oldCharges = _undoStack.Peek().Item5;
        Transform oldParent = _undoStack.Peek().Item6;
        bool oldSwimming = _undoStack.Peek().Item7;
        
        // update stack ONLY if any change in THIS object has actually occurred
        if (!newPos.Equals(oldPos) || newFacing != oldFacing || newType != oldType
            || !ListsEqual(newCharges, oldCharges) || newParent != oldParent || newSwimming != oldSwimming)
        {
            _undoStack.Push((_localFrame, newPos, newFacing, newType, newCharges, newParent, newSwimming));
        }
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
            Vector2Int oldPos = _mover.GetLocalGridPos();
            Vector2Int newPos = _undoStack.Peek().Item2;
            _mover.SetLocalGoal(newPos.x, newPos.y);

            // update player facing direction
            bool newFacing = _undoStack.Peek().Item3;
            _playerControls.SetFacingRight(newFacing);

            // update player dino type
            DinoType newType = _undoStack.Peek().Item4;
            _playerControls.SetDinoType(newType);

            // update player ability charges
            int[] newCharges = _undoStack.Peek().Item5;
            _playerControls.SetAbilityCharges(newCharges);

            // update parent transform
            Transform oldParent = _mover.transform.parent;
            Transform newParent = _undoStack.Peek().Item6;
            _mover.transform.parent = newParent;

            // update swimming state
            bool newSwimming = _undoStack.Peek().Item7;
            _playerControls.IsSwimming = newSwimming;

            // instant snapping - occurs for between panels OR multi-tile movement
            if (oldParent != newParent || Vector2Int.Distance(oldPos, newPos) > 1.1f) // slightly above 1 for deadband safety
            {
                Debug.Log("Big move");
                _mover.SnapToGoal();

                // also visually flip to avoid jarring nature of instantaneous move
                PlayerSpriteSwapper flipper = GetComponentInChildren<PlayerSpriteSwapper>();
                if (flipper is null)
                    throw new System.Exception("Player must have PlayerSpriteSwapper component on one of its children.");
                flipper.RequireFlip();
            }
        }
    }

    /// <summary>
    /// Returns true only if each list has the same contents in the same order.
    /// </summary>
    private bool ListsEqual(int[] list1, int[] list2)
    {
        // automatically not equal if different number of elements
        if (list1.Length != list2.Length)
            return false;

        // check individual elements
        for (int i = 0; i < list1.Length; i++)
        {
            if (list1[i] != list2[i])
                return false;
        }

        // if we got this far, the lists are equal.
        return true;
    }
}
