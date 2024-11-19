using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles iterating through current fire bushes, spreading to adjacent bushes, and then destroying itself.
/// </summary>
public class FireSpreadHandler : MonoBehaviour
{
    private static List<QuantumState> _fireBushes = new();
    private static List<QuantumState> _addQueue = new();

    /// <summary>
    /// Iterates through all flaming bushes, spreading fire, and then destroying them
    /// </summary>
    public static void UpdateFireTick()
    {
        // update for each stored fire bush
        for (int i = _fireBushes.Count - 1; i >=0; i--)
        {
            if (!_fireBushes[i].TryGetComponent(out Mover bushMover))
                throw new System.Exception("All level objects MUST have Mover component.");

            // skip over burning bushes that are currently hidden
            Vector2Int posToCheck = bushMover.GetGlobalGridPos();
            if (!VisibilityChecks.IsVisible(_fireBushes[i].gameObject, posToCheck.x, posToCheck.y))
                continue;

            // retrieve object in each of four directions
            QuantumState[] checks = new QuantumState[4];
            posToCheck = bushMover.GetGlobalGridPos() + Vector2Int.up;
            checks[0] = VisibilityChecks.GetObjectAtPos(bushMover, posToCheck.x, posToCheck.y);
            posToCheck = bushMover.GetGlobalGridPos() + Vector2Int.right;
            checks[1] = VisibilityChecks.GetObjectAtPos(bushMover, posToCheck.x, posToCheck.y);
            posToCheck = bushMover.GetGlobalGridPos() + Vector2Int.down;
            checks[2] = VisibilityChecks.GetObjectAtPos(bushMover, posToCheck.x, posToCheck.y);
            posToCheck = bushMover.GetGlobalGridPos() + Vector2Int.left;
            checks[3] = VisibilityChecks.GetObjectAtPos(bushMover, posToCheck.x, posToCheck.y);

            // spread fire to each unlit bush neighbor
            foreach (QuantumState check in checks)
            {
                // no updates needed if no object found
                if (check is null)
                    continue;

                // skip spreading fire to adjacent bushes that are not visible
                if (!check.TryGetComponent(out Mover checkMover))
                    throw new System.Exception("all level objects MUST have a Mover component.");
                Vector2Int checkPos = checkMover.GetGlobalGridPos();
                if (!VisibilityChecks.IsVisible(checkMover.gameObject, checkPos.x, checkPos.y))
                    continue;

                // light on fire and add to list for future checks
                if (check.ObjData.ObjType == ObjectType.Bush && !check.ObjData.IsOnFire)
                {
                    check.ObjData.IsOnFire = true;
                    _fireBushes.Add(check);
                }
            }

            // destroy current bush, its spreading has completed
            _fireBushes[i].ObjData.IsDisabled = true;
            _fireBushes.RemoveAt(i);
        }

        // delay the first fire tick frame after adding to avoid instand one-step of burning
        for (int i = _addQueue.Count - 1; i >= 0; i--)
        {
            _fireBushes.Add(_addQueue[i]);
            _addQueue.RemoveAt(i);
        }
    }

    /// <summary>
    /// Adds a newly created fire bush to the list to be used in the next fire tick.
    /// Used to start the process of recursive spreading
    /// </summary>
    public static void AddFireBush(QuantumState newBush)
    {
        _addQueue.Add(newBush);
    }

    /// <summary>
    /// Returns a copy of the fire bush list (to avoid undo issues with reference to list).
    /// </summary>
    public static QuantumState[] GetFireBushes()
    {
        QuantumState[] newList = new QuantumState[_fireBushes.Count];
        for (int i = 0; i < _fireBushes.Count; i++)
            newList[i] = _fireBushes[i];

        return newList;
    }

    /// <summary>
    /// Updates fire bushes list fully. to be used in undo handler.
    /// </summary>
    public static void SetFireBushes(QuantumState[] newBushes)
    {
        _fireBushes = new List<QuantumState>(newBushes);
    }
}