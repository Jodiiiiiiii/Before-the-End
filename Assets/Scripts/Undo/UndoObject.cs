using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implements SaveStackFrame and UndoStackFrame functions for a player object.
/// </summary>
public class UndoObject : UndoHandler
{
    [SerializeField, Tooltip("Contains object type, saved to undo stack")]
    private ObjectState _objState;

    // OBJECT TYPE: local frame, ObjectType enum, quantum state
    private Stack<(int, ObjectState.ObjectType, bool)> _typeStack = new Stack<(int, ObjectState.ObjectType, bool)>();

    // LOGS: local frame, localPosition
    private Stack<(int, Vector2Int)> _logStack = new Stack<(int, Vector2Int)>();

    protected override void SaveStackFrame()
    {
        // Retrieve new values
        ObjectState.ObjectType newType = _objState.ObjType;
        bool newQuantumState = _objState.IsQuantum();

        // add if empty, or a change occurred
        // always save state if quantum (to prevent object type re-jumbling while undoing)
        if (_typeStack.Count == 0 || newType != _typeStack.Peek().Item2 || newQuantumState) 
        {
            _typeStack.Push((_localFrame, newType, newQuantumState));
        }

        // Update specific stack type
        switch(newType)
        {
            case ObjectState.ObjectType.Log:
                // Retrieve new values
                Vector2Int newPos = _objectMover.GetLocalGridPos();

                // add if all empty, or a change occurred
                if(AreObjectStacksEmpty() || !newPos.Equals(_logStack.Peek().Item2))
                        _logStack.Push((_localFrame, newPos));

                break;
            case ObjectState.ObjectType.Water:
                break;
            case ObjectState.ObjectType.Rock:
                break;
            case ObjectState.ObjectType.TallRock:
                break;
            case ObjectState.ObjectType.Bush:
                break;
            case ObjectState.ObjectType.TallBush:
                break;
            case ObjectState.ObjectType.Tunnel:
                break;
            case ObjectState.ObjectType.Pickup:
                break;
        }
    }

    protected override void UndoStackFrame()
    {
        // get type before we potentially remove it
        ObjectState.ObjectType oldType = _typeStack.Peek().Item2;

        // CANNOT undo past first move, therefore count of at least 2 is required to undo
        // Remove current data and revert type (type stack is saved every update frame)
        if (_typeStack.Count > 1 && _typeStack.Peek().Item1 > _localFrame)
        {
            _typeStack.Pop();

            // update actual object type & quantum state
            ObjectState.ObjectType newType = _typeStack.Peek().Item2;
            _objState.ObjType = newType;
            bool newQuantumState = _typeStack.Peek().Item3;
            _objState.SetQuantum(newQuantumState);
        }

        // CANNOT undo past first move, therefore count of at least 2 is required to undo
        // handle popping (from the stack of whatever type is was before undo)
        if (AnyStackGreaterThanOne())
        {
            switch (oldType)
            {
                case ObjectState.ObjectType.Log:
                    if(_logStack.Peek().Item1 > _localFrame) // still ensure only undo on proper frame
                        _logStack.Pop();
                    break;
                case ObjectState.ObjectType.Water:
                    // pop water
                    break;
                case ObjectState.ObjectType.Rock:
                    // pop rock
                    break;
                case ObjectState.ObjectType.TallRock:
                    // pop tall rock
                    break;
                case ObjectState.ObjectType.Bush:
                    // pop bush
                    break;
                case ObjectState.ObjectType.TallBush:
                    // pop tall bush
                    break;
                case ObjectState.ObjectType.Tunnel:
                    // pop tunnel
                    break;
                case ObjectState.ObjectType.Pickup:
                    // pop pickup
                    break;
            }
        }

        // Restore undo frame data from last stack frame
        switch (_typeStack.Peek().Item2)
        {
            case ObjectState.ObjectType.Log:
                // restore/undo position
                Vector2Int newPos = _logStack.Peek().Item2;
                _objectMover.SetLocalGoal(newPos.x, newPos.y);

                break;
            case ObjectState.ObjectType.Water:
                break;
            case ObjectState.ObjectType.Rock:
                break;
            case ObjectState.ObjectType.TallRock:
                break;
            case ObjectState.ObjectType.Bush:
                break;
            case ObjectState.ObjectType.TallBush:
                break;
            case ObjectState.ObjectType.Tunnel:
                break;
            case ObjectState.ObjectType.Pickup:
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