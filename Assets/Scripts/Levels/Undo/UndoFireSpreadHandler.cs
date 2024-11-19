using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles properly undoing the fire bush stack of the FireSpreadHandler.
/// </summary>
public class UndoFireSpreadHandler : UndoHandler
{
    // local frame, list of fire bushes
    private Stack<(int, QuantumState[])> _undoStack =
        new Stack<(int, QuantumState[])>();

    protected override void SaveStackFrame()
    {
        // retrieve new values
        QuantumState[] newFireBushes = FireSpreadHandler.GetFireBushes();

        // No need to compare if new frame is FIRST frame
        if (_undoStack.Count == 0)
            _undoStack.Push((_localFrame, newFireBushes));

        // Compare old values to current
        QuantumState[] oldFireBushes = _undoStack.Peek().Item2;

        // update stack ONLY if any change in THIS object has actually occurred
        if (!ListsEqual(newFireBushes, oldFireBushes))
        {
            _undoStack.Push((_localFrame, newFireBushes));
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

            // update fire bushes list
            QuantumState[] newFireBushes = _undoStack.Peek().Item2;
            FireSpreadHandler.SetFireBushes(newFireBushes);
        }
    }

    /// <summary>
    /// Returns true on if both lists are the same size AND contain the same element references.
    /// </summary>
    private bool ListsEqual(QuantumState[] list1, QuantumState[] list2)
    {
        // compare lengths
        if (list1.Length != list2.Length)
            return false;

        // compare individual elements
        for (int i = 0; i < list1.Length; i++)
        {
            if (list1[i] != list2[i])
                return false;
        }

        // if we got this far, all elements have the same references
        return true;
    }
}