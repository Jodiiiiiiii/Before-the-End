using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implements SaveStackFrame and UndoStackFrame functions for a player object.
/// </summary>
public class UndoObject : UndoHandler
{
    [SerializeField, Tooltip("Contains object type, saved to undo stack")]
    private ObjectStats _objStats;

    // OBJECT TYPE: local frame, ObjectType enum
    private Stack<(int, ObjectStats.ObjectType)> _typeStack = new Stack<(int, ObjectStats.ObjectType)>();

    // LOGS: local frame, localPosition
    private Stack<(int, Vector2Int)> _logStack = new Stack<(int, Vector2Int)>();

    protected override void SaveStackFrame()
    {
        // Retrieve new values
        ObjectStats.ObjectType newType = _objStats.ObjType;

        // add if empty, or a change occurred
        if (_typeStack.Count == 0 || newType != _typeStack.Peek().Item2)
            _typeStack.Push((_localFrame, newType));

        // Update specific stack type
        switch(newType)
        {
            case ObjectStats.ObjectType.Log:
                // Retrieve new values
                Vector2Int newPos = _objectMover.GetLocalGridPos();

                // add if all empty, or a change occurred
                if(AreObjectStacksEmpty() || !newPos.Equals(_logStack.Peek().Item2))
                        _logStack.Push((_localFrame, newPos));

                break;
            case ObjectStats.ObjectType.Water:
                break;
            case ObjectStats.ObjectType.Rock:
                break;
            case ObjectStats.ObjectType.TallRock:
                break;
            case ObjectStats.ObjectType.Bush:
                break;
            case ObjectStats.ObjectType.TallBush:
                break;
            case ObjectStats.ObjectType.Tunnel:
                break;
            case ObjectStats.ObjectType.Pickup:
                break;
        }
    }

    protected override void UndoStackFrame()
    {
        // get type before we potentially remove it
        ObjectStats.ObjectType oldType = _typeStack.Peek().Item2;

        // CANNOT undo past first move, therefore count of at least 2 is required to undo
        // Remove current data and revert type (if a change actually was registered the frame before)
        if (_typeStack.Count > 1 && _typeStack.Peek().Item1 > _localFrame)
        {
            _typeStack.Pop();

            // update actual object type
            ObjectStats.ObjectType newType = _typeStack.Peek().Item2;
            _objStats.ObjType = newType;
        }

        // CANNOT undo past first move, therefore count of at least 2 is required to undo
        // handle popping (from the stack of whatever type is was before undo)
        if (AnyStackGreaterThanOne())
        {
            switch (oldType)
            {
                case ObjectStats.ObjectType.Log:
                    if(_logStack.Peek().Item1 > _localFrame) // still ensure only undo on proper frame
                        _logStack.Pop();
                    break;
                case ObjectStats.ObjectType.Water:
                    // pop water
                    break;
                case ObjectStats.ObjectType.Rock:
                    // pop rock
                    break;
                case ObjectStats.ObjectType.TallRock:
                    // pop tall rock
                    break;
                case ObjectStats.ObjectType.Bush:
                    // pop bush
                    break;
                case ObjectStats.ObjectType.TallBush:
                    // pop tall bush
                    break;
                case ObjectStats.ObjectType.Tunnel:
                    // pop tunnel
                    break;
                case ObjectStats.ObjectType.Pickup:
                    // pop pickup
                    break;
            }
        }

        // Restore undo frame data from last stack frame
        switch (_typeStack.Peek().Item2)
        {
            case ObjectStats.ObjectType.Log:
                // restore/undo position
                Vector2Int newPos = _logStack.Peek().Item2;
                _objectMover.SetLocalGoal(newPos.x, newPos.y);

                break;
            case ObjectStats.ObjectType.Water:
                break;
            case ObjectStats.ObjectType.Rock:
                break;
            case ObjectStats.ObjectType.TallRock:
                break;
            case ObjectStats.ObjectType.Bush:
                break;
            case ObjectStats.ObjectType.TallBush:
                break;
            case ObjectStats.ObjectType.Tunnel:
                break;
            case ObjectStats.ObjectType.Pickup:
                break;
        }
    }

    /// <summary>
    /// Returns true if every stack within UndoObject (except _typeStack) is empty.
    /// </summary>
    private bool AreObjectStacksEmpty()
    {
        return _logStack.Count == 0; // Add "&& _otherStack.Count == 0" as object types are added.
    }

    /// <summary>
    /// returns true if any stack (including the type stack) contains more than one item (i.e. an undo operation should be possible)
    /// </summary>
    /// <returns></returns>
    private bool AnyStackGreaterThanOne()
    {
        return _typeStack.Count > 1 || _logStack.Count > 1; // Must add "|| _newStack.Count > 1" for all future added types.
    }
}