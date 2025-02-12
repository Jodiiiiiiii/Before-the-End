using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ObjectData;

/// <summary>
/// Implements SaveStackFrame and UndoStackFrame functions for a player object.
/// </summary>
public class UndoObject : UndoHandler
{
    [SerializeField, Tooltip("Contains all ObjectData and quantum data")]
    private QuantumState _objState;
    [SerializeField, Tooltip("Whether the object instantly snaps to any undo for a position change - useful for compy pair to avoid weird position sliding.")]
    private bool _instantSnapPos = false;

    // local frame, Grid pos, parent transform, quantum state, ObjectData
    private Stack<(int, Vector2Int, Transform, bool, ObjectData)> _undoStack = new Stack<(int, Vector2Int, Transform, bool, ObjectData)>();

    protected override void SaveStackFrame()
    {
        // Retrieve new values
        Vector2Int newPos = _mover.GetLocalGridPos();
        Transform newParent = _objState.transform.parent;
        bool newQuantumState = _objState.IsQuantum();
        ObjectData newObjData = _objState.ObjData;

        // push to stack if currently empty
        if (_undoStack.Count <= 0)
            _undoStack.Push((_localFrame, newPos, newParent, newQuantumState, newObjData));

        // retrieve old values
        Vector2Int oldPos = _undoStack.Peek().Item2;
        Transform oldParent = _undoStack.Peek().Item3;
        bool oldQuantumState = _undoStack.Peek().Item4;
        ObjectData oldObjData = _undoStack.Peek().Item5;

        // push to stack if change occurred
        // ALSO always save state if quantum (to prevent object type re-jumbling while undoing)
        if (oldPos != newPos || newParent != oldParent || newQuantumState != oldQuantumState || !newObjData.Equals(oldObjData))
        {
            _undoStack.Push((_localFrame, newPos, newParent, newQuantumState, newObjData));
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

            // grid pos
            Vector2Int newPos = _undoStack.Peek().Item2;
            _mover.SetLocalGoal(newPos.x, newPos.y);

            // parent transform
            Transform newParent = _undoStack.Peek().Item3;
            _objState.transform.parent = newParent;

            // quantum state
            bool newQuantumState = _undoStack.Peek().Item4;
            _objState.SetQuantum(newQuantumState);

            // Object Data
            ObjectData oldData = _objState.ObjData;
            ObjectData newObjData = _undoStack.Peek().Item5;
            _objState.ObjData = newObjData;

            // instant snapping
            if (_instantSnapPos)
                _mover.SnapToGoal();

            // visually flip if any component of object data changed
            if (!oldData.Equals(newObjData))
            {
                ObjectSpriteSwapper flipper = GetComponentInChildren<ObjectSpriteSwapper>();
                if (flipper is null)
                    throw new System.Exception("Player must have PlayerSpriteSwapper component on one of its children.");
                flipper.RequireFlip();
            }
        }
    }
}