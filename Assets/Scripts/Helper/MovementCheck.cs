using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MovementCheck
{
    /// <summary>
    /// Determines if the player is able to move in the specified direction.
    /// Input MUST have only one non-zero value and it must be either -1 or 1.
    /// Accounts for visibility checks and player interactions/obstructions with different object types.
    /// </summary>
    public static bool CanPlayerMove(PlayerControls currObj, Vector2Int moveDir)
    {
        // Ensure target position is validly only one unit away
        if (moveDir.magnitude != 1 || (moveDir.x != 1 && moveDir.x != -1 && moveDir.x != 0) || (moveDir.y != 1 && moveDir.y != -1 && moveDir.y != 0))
            throw new Exception("Input of CanMove function MUST have only one non-zero value and it must be eiether -1 or 1.");

        if (!currObj.TryGetComponent(out ObjectMover objMover))
            throw new Exception("Player object MUST have ObjectMover component.");

        Vector2Int currPos = objMover.GetGlobalGridPos();
        Vector2Int targetPos = currPos + moveDir;

        // Check for visibility at current position
        if (!VisibilityCheck.IsVisible(currObj, currPos.x, currPos.y))
            return false;

        // Check for immediate obstruction by higher-ordered panel
        if (!VisibilityCheck.IsVisible(currObj, targetPos.x, targetPos.y))
            return false;

        // Check for object in current panel at target position
        ObjectState obj = GetObjectAtPos(currObj, targetPos.x, targetPos.y);
        if (obj is null)
            return true; // NO OBJECT = player CAN move
        else if (obj.ObjType == ObjectState.ObjectType.Log)
        {
            // generate list of all logs to be pushed by the potential player move
            List<ObjectState> logs = new List<ObjectState>();
            logs.Add(obj);

            // Check for more logs
            bool currIsLog = true;
            while (currIsLog)
            {
                targetPos += moveDir;

                // Check for log's obstruction by higher-ordered panel
                if (!VisibilityCheck.IsVisible(currObj, targetPos.x, targetPos.y))
                    return false;

                // check for object at next position
                obj = GetObjectAtPos(currObj, targetPos.x, targetPos.y);
                if (obj is null) // no object blocking the log
                    currIsLog = false;
                else if (obj.ObjType == ObjectState.ObjectType.Log) // add another log and keep checking for more
                    logs.Add(obj);
                else // obstructed by water/rock/tallRock/bush/tallBush/Tunnel/Pickup // TODO: account forother cases later
                    return false;
            }

            // if we got this far, then all logs in the chain CAN move
            foreach (ObjectState log in logs)
            {
                // increment each log
                if (log.TryGetComponent(out ObjectMover logMover))
                    logMover.Increment(moveDir);
                else
                    throw new Exception("All log objects MUST have an ObjectMover component");
            }

            return true; // PUSHABLE LOGS = player CAN move
        }
        else // obstructed by water/rock/tallRock/bush/tallBush/Tunnel/Pickup // TODO: account forother cases later
            return false;
    }

    /// <summary>
    /// Returns the ObjectsStats component of the object at the specified grid position (within the same panel as the player)
    /// </summary>
    private static ObjectState GetObjectAtPos(PlayerControls player, int x, int y)
    {
        if (player.transform.parent is not null)
        {
            // iterate through sibling objects checking for position
            ObjectState[] siblingObjects = player.transform.parent.GetComponentsInChildren<ObjectState>();
            foreach (ObjectState obj in siblingObjects)
            {
                if (obj.TryGetComponent(out ObjectMover objMover))
                {
                    Vector2Int pos = objMover.GetGlobalGridPos();
                    if (pos.x == x && pos.y == y)
                        return obj;
                }
                else
                    throw new Exception("All Objects MUST have an ObjectMover.");
            }
        }
        else
            throw new Exception("Object MUST be a child of the 'Objects' object within a panel");

        // return null if no object at position found (on same panel as player)
        return null;
    }
}
