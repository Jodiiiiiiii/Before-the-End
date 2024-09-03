using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoSubpanel : UndoHandler
{
    // localFrame (total stack frame), local position
    private Stack<(int, Vector2Int)> _undoStack = new Stack<(int, Vector2Int)>();

    protected override void SaveStackFrame()
    {
        Vector2Int newPos = _objectMover.GetLocalGridPos();

        // No need to compare if new frame is FIRST frame
        if(_undoStack.Count == 0)
            _undoStack.Push((_localFrame, newPos));

        Vector2Int oldPos = _undoStack.Peek().Item2;

        // update stack ONLY if a change in THIS object has actually occurred
        if(!newPos.Equals(oldPos))
            _undoStack.Push((_localFrame, newPos));
    }

    protected override void UndoStackFrame()
    {
        // CANNOT undo past first move, therefore count of at least 2 is required to undo
        // Remove current position (if a change actually was registered before)
        if(_undoStack.Count > 1 && _undoStack.Peek().Item1 > _localFrame)
        {
            _undoStack.Pop();

            // update actual panel position
            Vector2Int newPos = _undoStack.Peek().Item2;
            _objectMover.SetLocalGoal(newPos.x, newPos.y);
        }
            
    }
}
