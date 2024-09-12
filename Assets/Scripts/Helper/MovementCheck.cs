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
        #region PRECONDITION VALIDATION
        // Ensure target position is validly only one unit away
        if (moveDir.magnitude != 1 || (moveDir.x != 1 && moveDir.x != -1 && moveDir.x != 0) || (moveDir.y != 1 && moveDir.y != -1 && moveDir.y != 0))
            throw new Exception("Input of CanMove function MUST have only one non-zero value and it must be eiether -1 or 1.");

        if (!currObj.TryGetComponent(out ObjectMover objMover))
            throw new Exception("Player object MUST have ObjectMover component.");

        Vector2Int currPos = objMover.GetGlobalGridPos();
        Vector2Int targetPos = currPos + moveDir;

        // Ensure player current visibility
        if (!VisibilityCheck.IsVisible(currObj, currPos.x, currPos.y))
            return false;
        #endregion

        #region PANEL PUSHING
        // Check for current panel visibility of target/adjacent pos
        if (!VisibilityCheck.IsVisible(currObj, targetPos.x, targetPos.y))
        {
            // try to find which panel is visible at target/adjacent pos
            for (int i = 0; i <= SortingOrderHandler.MaxPanelOrder; i++)
            {
                // Topmost panel at pos found
                if(VisibilityCheck.IsVisible(i, targetPos.x, targetPos.y))
                {
                    // retrieve current panel's panel order (through SortingOrderHandler)
                    if (currObj.transform.parent is not null && currObj.transform.parent.parent is not null
                        && currObj.transform.parent.parent.TryGetComponent(out SortingOrderHandler currSortHandler))
                    {
                        // Retrieve PanelStats of panel to be pushed AND its parent
                        PanelStats pushedPanel;
                        PanelStats parentPanel;
                        // should always enter one of the if statements above otherwise we wouldn't have gotten this far in the logic.
                        // this logic SHOULD prevent the pushed panel from ever being the main panel since the main panel CANNOT move.
                        if (i > currSortHandler.PanelOrder) // pushing external edge of other panel
                        {
                            // retrieve PanelStats of panel to be pushed
                            pushedPanel = SortingOrderHandler.GetPanelOfOrder(i);
                            // retrieve parent panel's panel stats
                            if (pushedPanel.transform.parent is null || !pushedPanel.transform.parent.TryGetComponent(out parentPanel))
                                throw new Exception("ALL Subpanels MUST have a parent panel with a PanelStats component.");
                        }
                        else if (i < currSortHandler.PanelOrder) // pushing internal edge of current panel
                        {
                            // retrieve current panel's PanelStats
                            if (!currSortHandler.TryGetComponent(out pushedPanel))
                                throw new Exception("ALL panels MUST have a PanelStats component.");
                            // retrieve parent panel's panel stats
                            if (pushedPanel.transform.parent is null || !pushedPanel.transform.parent.TryGetComponent(out parentPanel))
                                throw new Exception("ALL Subpanels MUST have a parent panel with a PanelStats component.");
                        }
                        else
                            throw new Exception("It should be impossible to see this exception unless something is fundamentally wrong with VisibilityCheck");
                        
                        // check if the pushed panel can move in the moveDir within its parent panel
                        if(pushedPanel.OriginX + moveDir.x >= parentPanel.OriginX
                            && pushedPanel.OriginX + moveDir.x + pushedPanel.Width <= parentPanel.OriginX + parentPanel.Width
                            && pushedPanel.OriginY + moveDir.y >= parentPanel.OriginY
                            && pushedPanel.OriginY + moveDir.y + pushedPanel.Height <= parentPanel.OriginY + parentPanel.Height)
                        {
                            if (!pushedPanel.TryGetComponent(out ObjectMover panelMover))
                                throw new Exception("ALL subpanels MUST have an ObjectMover component.");

                            // shuffle quantum objects just before moving panel
                            ObjectState.ShuffleHiddenQuantumObjects();
                            // Apply movement to pushed panel
                            panelMover.Increment(moveDir);
                            // Save undo stack frame since a change/action occurred
                            UndoHandler.SaveFrame();
                        }
                    }
                    else
                        throw new Exception("Player object MUST be contained within Objects child object of a panel AND the panel must have a SortingOrderHandler component.");

                    // no need to check more panels
                    break;
                }
            }

            // Player moved into a panel so, if any action occurs, it will not involve the player moving
            return false;
        }
        #endregion

        #region OBJECTS
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
        #endregion
    }

    /// <summary>
    /// Returns the ObjectsStats component of the object at the specified grid position (within the same panel as the player).
    /// Returns null if no object found.
    /// </summary>
    public static ObjectState GetObjectAtPos(PlayerControls player, int x, int y)
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
