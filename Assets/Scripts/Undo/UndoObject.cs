using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ObjectData;

/// <summary>
/// Implements SaveStackFrame and UndoStackFrame functions for a player object.
/// </summary>
public class UndoObject : UndoHandler
{
    [SerializeField, Tooltip("Contains object type, saved to undo stack")]
    private ObjectState _objState;

    // OBJECT TYPE: local frame, ObjectType enum, quantum state
    private Stack<(int, ObjectType, bool)> _typeStack = new Stack<(int, ObjectType, bool)>();

    // LOGS: local frame, localPosition
    private Stack<(int, Vector2Int)> _logStack = new Stack<(int, Vector2Int)>();
    // WATER: local frame, hasLog, hasRock
    private Stack<(int, bool, bool)> _waterStack = new Stack<(int, bool, bool)>();

    protected override void SaveStackFrame()
    {
        // TYPE STACK
        // Retrieve new values
        ObjectType newType = _objState.ObjData.ObjType;
        bool newQuantumState = _objState.IsQuantum();

        // push to stack if currently empty
        if (_typeStack.Count <= 0)
            _typeStack.Push((_localFrame, newType, newQuantumState));

        // retrieve old values
        ObjectType oldType = _typeStack.Peek().Item2;
        bool oldQuantumState = _typeStack.Peek().Item3;

        // push to stack if change occurred
        // ALSO always save state if quantum (to prevent object type re-jumbling while undoing)
        if (newType != oldType || newQuantumState != oldQuantumState || newQuantumState) 
            _typeStack.Push((_localFrame, newType, newQuantumState));

        // SPECIFIC OBJECT STACK
        switch(newType)
        {
            case ObjectType.Log:
                // Retrieve new values
                Vector2Int newPos = _objectMover.GetLocalGridPos();

                // push to stack if stack currently empty
                if(_logStack.Count <= 0)
                    _logStack.Push((_localFrame, newPos));

                // retrieve old values
                Vector2Int oldPos = _logStack.Peek().Item2;

                // push to stack if a change occurred
                if(!newPos.Equals(oldPos))
                    _logStack.Push((_localFrame, newPos));

                break;
            case ObjectType.Water:
                // Retrieve new values
                bool newHasLog = _objState.ObjData.WaterHasLog;
                bool newHasRock = _objState.ObjData.WaterHasRock;

                // push to stack if currently empty
                if (_waterStack.Count <= 0)
                    _waterStack.Push((_localFrame, newHasLog, newHasRock));

                // retrieve old values
                bool oldHasLog = _waterStack.Peek().Item2;
                bool oldHasRock = _waterStack.Peek().Item3;

                // push to stack if change occurred
                if (newHasLog != oldHasLog || newHasRock != oldHasRock)
                    _waterStack.Push((_localFrame, newHasLog, newHasRock));

                break;
            case ObjectType.Rock:
                break;
            case ObjectType.TallRock:
                break;
            case ObjectType.Bush:
                break;
            case ObjectType.TallBush:
                break;
            case ObjectType.Tunnel:
                break;
            case ObjectType.Pickup:
                break;
        }
    }

    protected override void UndoStackFrame()
    {
        // get type before we potentially remove it
        ObjectType oldType = _typeStack.Peek().Item2;

        // TYPE STACK (POP + RESTORE)
        // CANNOT undo past first move, therefore count of at least 2 is required to undo
        // Remove current data and revert type (and quantum state)
        if (_typeStack.Count > 1 && _typeStack.Peek().Item1 > _localFrame)
        {
            _typeStack.Pop();

            // update actual object type & quantum state
            ObjectType newType = _typeStack.Peek().Item2;
            _objState.ObjData.ObjType = newType;
            bool newQuantumState = _typeStack.Peek().Item3;
            _objState.SetQuantum(newQuantumState);
        }

        // SPECIFIC OBJECT STACK (POP)
        // CANNOT undo past first move, therefore count of at least 2 is required to undo
        // popping is based on type before popping of the type stack
        // still ensure only pop on proper frame when change occurred
        switch (oldType)
        {
            case ObjectType.Log:
                if(_logStack.Count > 1 && _logStack.Peek().Item1 > _localFrame) 
                    _logStack.Pop();
                break;
            case ObjectType.Water:
                if (_waterStack.Count > 1 && _waterStack.Peek().Item1 > _localFrame)
                    _waterStack.Pop();
                break;
            case ObjectType.Rock:
                // pop rock
                break;
            case ObjectType.TallRock:
                // pop tall rock
                break;
            case ObjectType.Bush:
                // pop bush
                break;
            case ObjectType.TallBush:
                // pop tall bush
                break;
            case ObjectType.Tunnel:
                // pop tunnel
                break;
            case ObjectType.Pickup:
                // pop pickup
                break;
        }

        // SPECIFIC OBJECT STACK (RESTORE)
        // Restore undo frame data from new topmost stack frame (may be unchanged).
        switch (_typeStack.Peek().Item2)
        {
            case ObjectType.Log:
                // restore/undo position
                Vector2Int newPos = _logStack.Peek().Item2;
                _objectMover.SetLocalGoal(newPos.x, newPos.y);

                break;
            case ObjectType.Water:
                bool newHasLog = _waterStack.Peek().Item2;
                _objState.ObjData.WaterHasLog = newHasLog;

                bool newHasRock = _waterStack.Peek().Item3;
                _objState.ObjData.WaterHasLog = newHasRock;

                break;
            case ObjectType.Rock:
                break;
            case ObjectType.TallRock:
                break;
            case ObjectType.Bush:
                break;
            case ObjectType.TallBush:
                break;
            case ObjectType.Tunnel:
                break;
            case ObjectType.Pickup:
                break;
        }
    }
}