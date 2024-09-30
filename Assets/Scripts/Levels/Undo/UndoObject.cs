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
    private QuantumState _objState;

    // OBJECT TYPE: local frame, ObjectType enum, quantum state, isDisabled, parent transform
    private Stack<(int, ObjectType, bool, bool, Transform)> _objectStack = new Stack<(int, ObjectType, bool, bool, Transform)>();

    // LOGS: local frame, localPosition
    private Stack<(int, Vector2Int)> _logStack = new Stack<(int, Vector2Int)>();
    // ROCKS: local frame, localPosition
    private Stack<(int, Vector2Int)> _rockStack = new Stack<(int, Vector2Int)>();
    // WATER: local frame, hasLog, hasRock
    private Stack<(int, bool, bool)> _waterStack = new Stack<(int, bool, bool)>();
    // TUNNEL: local frame, other tunnel, tunnel index
    private Stack<(int, QuantumState, int)> _tunnelStack = new Stack<(int, QuantumState, int)>();

    protected override void SaveStackFrame()
    {
        // OBJECT STACK
        // Retrieve new values
        ObjectType newType = _objState.ObjData.ObjType;
        bool newQuantumState = _objState.IsQuantum();
        bool newIsDisabled = _objState.ObjData.IsDisabled;
        Transform newParent = _objState.transform.parent;

        // push to stack if currently empty
        if (_objectStack.Count <= 0)
            _objectStack.Push((_localFrame, newType, newQuantumState, newIsDisabled, newParent));

        // retrieve old values
        ObjectType oldType = _objectStack.Peek().Item2;
        bool oldQuantumState = _objectStack.Peek().Item3;
        bool oldIsDisabled = _objectStack.Peek().Item4;
        Transform oldParent = _objectStack.Peek().Item5;

        // push to stack if change occurred
        // ALSO always save state if quantum (to prevent object type re-jumbling while undoing)
        if (newType != oldType || newQuantumState != oldQuantumState || newIsDisabled != oldIsDisabled
            || newParent != oldParent || newQuantumState)
        {
            _objectStack.Push((_localFrame, newType, newQuantumState, newIsDisabled, newParent));
        }

        // SPECIFIC OBJECT STACK
        switch(newType)
        {
            case ObjectType.Log:
                // Retrieve new values
                Vector2Int newLogPos = _mover.GetLocalGridPos();

                // push to stack if stack currently empty
                if(_logStack.Count <= 0)
                    _logStack.Push((_localFrame, newLogPos));

                // retrieve old values
                Vector2Int oldLogPos = _logStack.Peek().Item2;

                // push to stack if a change occurred
                if(!newLogPos.Equals(oldLogPos))
                    _logStack.Push((_localFrame, newLogPos));

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
                // Retrieve new values
                Vector2Int newRockPos = _mover.GetLocalGridPos();

                // push to stack if stack currently empty
                if (_rockStack.Count <= 0)
                    _rockStack.Push((_localFrame, newRockPos));

                // retrieve old values
                Vector2Int oldRockPos = _rockStack.Peek().Item2;

                // push to stack if a change occurred
                if (!newRockPos.Equals(oldRockPos))
                    _rockStack.Push((_localFrame, newRockPos));

                break;
            case ObjectType.TallRock:
                break;
            case ObjectType.Bush:
                break;
            case ObjectType.TallBush:
                break;
            case ObjectType.Tunnel:
                // Retrieve new values
                QuantumState newOtherTunnel = _objState.ObjData.OtherTunnel;
                int newTunnelIndex = _objState.ObjData.TunnelIndex;

                // push to stack if stack currently empty
                if (_tunnelStack.Count <= 0)
                    _tunnelStack.Push((_localFrame, newOtherTunnel, newTunnelIndex));

                // retrieve old values
                QuantumState oldOtherTunnel = _tunnelStack.Peek().Item2;
                int oldTunnelIndex = _tunnelStack.Peek().Item3;

                // push to stack if a change occurred
                if (newOtherTunnel != oldOtherTunnel || newTunnelIndex != oldTunnelIndex)
                    _tunnelStack.Push((_localFrame, newOtherTunnel, newTunnelIndex));

                break;
            case ObjectType.Clock:
                break;
        }
    }

    protected override void UndoStackFrame()
    {
        // get type before we potentially remove it
        ObjectType oldType = _objectStack.Peek().Item2;

        // OBJECT STACK (POP + RESTORE)
        // CANNOT undo past first move, therefore count of at least 2 is required to undo
        // Remove current data and revert type (and quantum state)
        if (_objectStack.Count > 1 && _objectStack.Peek().Item1 > _localFrame)
        {
            _objectStack.Pop();

            // update actual object type & quantum state
            ObjectType newType = _objectStack.Peek().Item2;
            _objState.ObjData.ObjType = newType;
            bool newQuantumState = _objectStack.Peek().Item3;
            _objState.SetQuantum(newQuantumState);
            bool newIsDisabled = _objectStack.Peek().Item4;
            _objState.ObjData.IsDisabled = newIsDisabled;
            Transform newParent = _objectStack.Peek().Item5;
            _objState.transform.parent = newParent;
        }

        // SPECIFIC OBJECT STACK (POP)
        // CANNOT undo past first move, therefore count of at least 2 is required to undo
        // popping is based on type before popping of the type stack
        // still ensure only pop on proper frame when change occurred
        switch (oldType)
        {
            case ObjectType.Log: // pop log
                if(_logStack.Count > 1 && _logStack.Peek().Item1 > _localFrame) 
                    _logStack.Pop();
                break;
            case ObjectType.Water: // pop water
                if (_waterStack.Count > 1 && _waterStack.Peek().Item1 > _localFrame)
                    _waterStack.Pop();
                break;
            case ObjectType.Rock: // pop rock
                if (_rockStack.Count > 1 && _rockStack.Peek().Item1 > _localFrame)
                    _rockStack.Pop();
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
            case ObjectType.Tunnel: // pop tunnel
                if (_tunnelStack.Count > 1 && _tunnelStack.Peek().Item1 > _localFrame)
                    _tunnelStack.Pop();
                break;
            case ObjectType.Clock:
                // pop clock
                break;
        }

        // SPECIFIC OBJECT STACK (RESTORE)
        // Restore undo frame data from new topmost stack frame (may be unchanged).
        switch (_objectStack.Peek().Item2)
        {
            case ObjectType.Log:
                Vector2Int newLogPos = _logStack.Peek().Item2;
                _mover.SetLocalGoal(newLogPos.x, newLogPos.y);

                break;
            case ObjectType.Water:
                bool newHasLog = _waterStack.Peek().Item2;
                _objState.ObjData.WaterHasLog = newHasLog;

                bool newHasRock = _waterStack.Peek().Item3;
                _objState.ObjData.WaterHasRock = newHasRock;

                break;
            case ObjectType.Rock:
                Vector2Int newRockPos = _rockStack.Peek().Item2;
                _mover.SetLocalGoal(newRockPos.x, newRockPos.y);

                break;
            case ObjectType.TallRock:
                break;
            case ObjectType.Bush:
                break;
            case ObjectType.TallBush:
                break;
            case ObjectType.Tunnel:
                QuantumState newOtherTunnel = _tunnelStack.Peek().Item2;
                _objState.ObjData.OtherTunnel = newOtherTunnel;

                int newTunnelIndex = _tunnelStack.Peek().Item3;
                _objState.ObjData.TunnelIndex = newTunnelIndex;

                break;
            case ObjectType.Clock:
                break;
        }
    }
}