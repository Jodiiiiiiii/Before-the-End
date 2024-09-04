using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implements SaveStackFrame and UndoStackFrame functions for a subpanel object.
/// </summary>
public class UndoSubpanel : UndoHandler
{
    // localFrame (total stack frame), local position, sibling order
    private Stack<(int, Vector2Int, int)> _undoStack = new Stack<(int, Vector2Int, int)>();

    protected override void SaveStackFrame()
    {
        Vector2Int newPos = _objectMover.GetLocalGridPos();

        // retrieve sibling order (0 for panels without siblings)
        int siblingOrder;
        if (TryGetComponent(out SiblingOrderHandler siblingHandler))
            siblingOrder = siblingHandler.SiblingOrder;
        else
            siblingOrder = 0;

        // No need to compare if new frame is FIRST frame
        if(_undoStack.Count == 0)
            _undoStack.Push((_localFrame, newPos, siblingOrder));

        // Compare old values to current
        Vector2Int oldPos = _undoStack.Peek().Item2;
        int oldSiblingOrder = _undoStack.Peek().Item3;
        // update stack ONLY if any change in THIS object has actually occurred
        if(!newPos.Equals(oldPos) || siblingOrder != oldSiblingOrder)
            _undoStack.Push((_localFrame, newPos, siblingOrder));
    }

    protected override void UndoStackFrame()
    {
        // CANNOT undo past first move, therefore count of at least 2 is required to undo
        // Remove current position (if a change actually was registered before)
        if(_undoStack.Count > 1 && _undoStack.Peek().Item1 > _localFrame)
        {
            int currSiblingOrder = _undoStack.Peek().Item3;

            _undoStack.Pop();

            // update actual panel position
            Vector2Int newPos = _undoStack.Peek().Item2;
            _objectMover.SetLocalGoal(newPos.x, newPos.y);

            // update panel sorting order (if necessary)
            int newSiblingOrder = _undoStack.Peek().Item3;
            if (TryGetComponent(out SiblingOrderHandler siblingHandler))
            {
                if (Mathf.Abs(newSiblingOrder - currSiblingOrder) > 1) // Invalid Undo (more than 1 change
                    throw new System.Exception("Invalid Undo Operation. Somehow sibling order changed by more than one index in one action.");
                else if (newSiblingOrder < currSiblingOrder) // Lower
                    siblingHandler.Lower();
                else if (newSiblingOrder > currSiblingOrder) // Raise
                    siblingHandler.Raise();
            }
            // no updates due to sibling order if it is not a panel with siblings
        }
    }
}
