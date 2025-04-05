using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles iterating through current fire bushes, spreading to adjacent bushes, and then destroying itself.
/// </summary>
public class FireSpreadHandler : MonoBehaviour
{
    private const int ACTIONS_TO_DESTROY = 5; // number of actions a bush must be burning before it is destroyed
    private const int ACTIONS_TILL_SPREAD = 2; // number of actions a bush must be burning before it can spread fire

    // contains bush object itself and ticker for when it is time to destroy it
    private static List<(QuantumState, int)> _fireBushes = new();
    private static List<QuantumState> _addQueue = new();

    public static void ResetBushLists()
    {
        // ensure lists properly cleared upon entering new level - called from UndoFireSpreadHandler
        // otherwise there are cases where referencing an object from a previous sceenc an cause a crash.
        _fireBushes = new();
        _addQueue = new();
    }

    /// <summary>
    /// Iterates through all flaming bushes, spreading fire, and then destroying them
    /// </summary>
    public static void UpdateFireTick()
    {
        // update for each stored fire bush
        for (int i = _fireBushes.Count - 1; i >=0; i--)
        {
            // skip over burning bushes that are currently hidden
            if (!_fireBushes[i].Item1.TryGetComponent(out Mover bushMover))
                throw new System.Exception("All level objects MUST have Mover component.");
            Vector2Int posToCheck = bushMover.GetGlobalGridPos();
            if (!VisibilityChecks.IsVisible(_fireBushes[i].Item1.gameObject, posToCheck.x, posToCheck.y))
                continue;

            // tick each burning bush by one action
            _fireBushes[i] = (_fireBushes[i].Item1, _fireBushes[i].Item2 + 1);

            // Only spread fire after a certain number of actions have happened
            if (_fireBushes[i].Item2 >= ACTIONS_TILL_SPREAD)
            {
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
                        _fireBushes.Add((check, 0));

                        // play fire spread - bush has been lit BY ANOTHER BUSH (more subtle sound)
                        AudioManager.Instance.PlayFireSpread();
                    }
                }
            }

            // destroy current bush after certain number of ticks, its spreading has completed
            if (_fireBushes[i].Item2 >= ACTIONS_TO_DESTROY)
            {
                _fireBushes[i].Item1.ObjData.IsDisabled = true;
                _fireBushes.RemoveAt(i);
            }
        }

        // delay the first fire tick frame after adding to avoid instand one-step of burning
        for (int i = _addQueue.Count - 1; i >= 0; i--)
        {
            _fireBushes.Add((_addQueue[i], 0));
            _addQueue.RemoveAt(i);

            // play fire ignite SFX - bush has been lit
            // handling sound here ensures each log-lit bush plays SFX once
            AudioManager.Instance.PlayFireIgnite();
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
    /// Directly adds fire bush to fire bush list.
    /// This bypasses the addQueue and is useful specifically for initialization of fire bushes that start in the scene.
    /// </summary>
    public static void AddFireBushDirect(QuantumState newBush)
    {
        _fireBushes.Add((newBush, 0));
    }

    /// <summary>
    /// Replaces one bush with another in the fire bushes list (and add queue).
    /// Used when quantum swapping a fire bush with another object (otherwise the random other object would continue burning INSTEAD of the fire bush)
    /// </summary>
    public static void SwapFireBush(QuantumState bush1, QuantumState bush2)
    {
        // replace old bush with new bush in FireBushes
        for(int i = 0; i < _fireBushes.Count; i++)
        {
            if (_fireBushes[i].Item1 == bush1)
            {
                _fireBushes[i] = (bush2, _fireBushes[i].Item2);
                return;
            }
        }

        // if we got this far, no oldBush was found, which is incorrect use of the function
        throw new System.Exception("Invalid use of SwapFireBush: oldBush reference not found in fire bush list OR queue list.");
    }

    /// <summary>
    /// Returns a copy of the fire bush list (to avoid undo issues with reference to list).
    /// </summary>
    public static (QuantumState, int)[] GetFireBushes()
    {
        (QuantumState, int)[] newList = new (QuantumState, int)[_fireBushes.Count];
        for (int i = 0; i < _fireBushes.Count; i++)
            newList[i] = _fireBushes[i];

        return newList;
    }

    /// <summary>
    /// Updates fire bushes list fully. to be used in undo handler.
    /// </summary>
    public static void SetFireBushes((QuantumState,int)[] newBushes)
    {
        _fireBushes = new List<(QuantumState, int)>(newBushes);
    }
}