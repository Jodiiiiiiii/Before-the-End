using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerControls;
using static ObjectData;

public static class PlayerActionChecks
{
    #region PLAYER MOVEMENT
    /// <summary>
    /// Determines if the player is able to move in the specified direction and handles all according behavior.
    /// Input MUST have only one non-zero value and it must be either -1 or 1.
    /// Accounts for visibility checks and player interactions/obstructions with different object types.
    /// </summary>
    public static void TryPlayerMove(PlayerControls player, Vector2Int moveDir)
    {
        #region PRECONDITION VALIDATION
        // Ensure target position is validly only one unit away
        if (moveDir.magnitude != 1 || (moveDir.x != 1 && moveDir.x != -1 && moveDir.x != 0) || (moveDir.y != 1 && moveDir.y != -1 && moveDir.y != 0))
            throw new Exception("Input of CanMove function MUST have only one non-zero value and it must be eiether -1 or 1.");

        if (!player.TryGetComponent(out ObjectMover objMover))
            throw new Exception("Player object MUST have ObjectMover component.");

        Vector2Int currPos = objMover.GetGlobalGridPos();
        Vector2Int targetPos = currPos + moveDir;
        
        // Ensure player current visibility
        if (!VisibilityCheck.IsVisible(player, currPos.x, currPos.y))
            return;
        #endregion

        #region SPRITE FLIPPING
        bool hasFlipped = false;
        int prevFrame = UndoHandler.GetGlobalFrame();
        if (player.IsFacingRight() && moveDir == Vector2Int.left)
        {
            player.SetFacingRight(false);
            hasFlipped = true;
        }
        else if (!player.IsFacingRight() && moveDir == Vector2Int.right)
        {
            player.SetFacingRight(true);
            hasFlipped = true;
        }
        #endregion

        // try all panel pushing and moving w/ objects action possibilities
        // ensure save stack frame happens in finally section if hasFlipped is true
        try
        {
            #region PANEL PUSHING
            // Check for current panel visibility of target/adjacent pos
            if (!VisibilityCheck.IsVisible(player, targetPos.x, targetPos.y))
            {
                // try to find which panel is visible at target/adjacent pos
                for (int i = 0; i <= SortingOrderHandler.MaxPanelOrder; i++)
                {
                    // Topmost panel at pos found
                    if (VisibilityCheck.IsVisible(i, targetPos.x, targetPos.y))
                    {
                        // retrieve current panel's panel order (through SortingOrderHandler)
                        if (player.transform.parent is not null && player.transform.parent.parent is not null
                            && player.transform.parent.parent.TryGetComponent(out SortingOrderHandler currSortHandler))
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
                            if (pushedPanel.OriginX + moveDir.x >= parentPanel.OriginX
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
                                // action fully completed
                                UndoHandler.SaveFrame();
                                return;
                            }
                        }
                        else
                            throw new Exception("Player object MUST be contained within Objects child object of a panel AND the panel must have a SortingOrderHandler component.");

                        // no need to check more panels
                        break;
                    }
                }
                // Player moved into a panel so, if any action occurs, it has already happened
                return;
            }
            #endregion

            #region OBJECTS / PLAYER MOVEMENT
            // Check for object in current panel at target position
            ObjectState obj = GetObjectAtPos(player, targetPos.x, targetPos.y);
            if (obj is null)
            {
                // Move action complete
                ConfirmPlayerMove(player, objMover, moveDir);
                return;
            }

            // Player collides immediately with WHICH object type??
            switch(obj.ObjData.ObjType)
            {
                case ObjectType.Log:

                    // generate list of all logs to be pushed by the potential player move
                    List<ObjectState> logs = new List<ObjectState>();
                    logs.Add(obj);

                    // Find all logs in a chain
                    bool currIsLog = true;
                    while (currIsLog)
                    {
                        targetPos += moveDir;

                        // Check for log's obstruction by higher-ordered panel
                        if (!VisibilityCheck.IsVisible(player, targetPos.x, targetPos.y))
                            return;

                        // check for object at next position
                        obj = GetObjectAtPos(player, targetPos.x, targetPos.y);
                        if (obj is null) // no object blocking the log
                            currIsLog = false;
                        else if (obj.ObjData.ObjType == ObjectType.Log) // add another log and keep checking for more
                            logs.Add(obj);
                        else if (obj.ObjData.ObjType == ObjectType.Water)
                        {
                            // log can be pushed over water with rock/log
                            if (obj.ObjData.WaterHasRock || obj.ObjData.WaterHasLog)
                                currIsLog = false;
                            else // if water has nothing in it, add log; if water has log, push new log in and keep log in water
                            {
                                // exit loop
                                currIsLog = false;

                                // update water data state
                                obj.ObjData.WaterHasLog = true;

                                // disable last most log in the list (it was pushed into water!)
                                logs[logs.Count - 1].ObjData.IsDisabled = true;
                            }
                        }
                        else // obstructed rock/tallRock/bush/tallBush/Tunnel/Pickup
                            return; // handle other cases here as necessary
                    }

                    // if we got this far, then all logs in the chain CAN move
                    // AND we therefore know player can move too

                    // move logs
                    foreach (ObjectState log in logs)
                    {
                        // increment each log
                        if (log.TryGetComponent(out ObjectMover logMover))
                            logMover.Increment(moveDir);
                        else
                            throw new Exception("All log objects MUST have an ObjectMover component");
                    }

                    // Move action complete
                    ConfirmPlayerMove(player, objMover, moveDir);
                    return;

                case ObjectType.Water:
                    // allow movement if water has an object in it
                    if(obj.ObjData.WaterHasLog || obj.ObjData.WaterHasRock)
                    {
                        // Move action complete
                        ConfirmPlayerMove(player, objMover, moveDir);
                        return;
                    }

                    return;
                case ObjectType.Rock:
                    return; // unimplemented
                case ObjectType.TallRock:
                    return; // unimplemented
                case ObjectType.Bush:
                    return; // unimplemented
                case ObjectType.TallBush:
                    return; // unimplemented
                case ObjectType.Tunnel:
                    return; // unimplemented
                case ObjectType.Pickup:
                    return; // unimplemented
            }
            #endregion
        }
        finally
        {
            // check if flip happened but new undo frame has not been saved yet
            if (hasFlipped && prevFrame == UndoHandler.GetGlobalFrame())
                UndoHandler.SaveFrame();
        }

        throw new Exception("Issue with TryPlayerMove. Should have returned at some point but did not.");
    }

    /// <summary>
    /// Contains all necessary functions when player move is confirmed
    /// Handles potential log sinking (on curr position), player movement, and saving an undo frame.
    /// </summary>
    private static void ConfirmPlayerMove(PlayerControls player, ObjectMover objMover, Vector2Int moveDir)
    {
        // If player was on log, sink log along with player movement
        Vector2Int currPos = objMover.GetGlobalGridPos();
        ObjectState logSinkCheck = GetObjectAtPos(player, currPos.x, currPos.y);
        if (logSinkCheck != null && logSinkCheck.ObjData.ObjType == ObjectType.Water && logSinkCheck.ObjData.WaterHasLog)
            logSinkCheck.ObjData.WaterHasLog = false;

        // move player
        objMover.Increment(moveDir);

        // completed player movement action
        UndoHandler.SaveFrame();
    }
    #endregion

    #region PLAYER ABILITY CHECKS
    /// <summary>
    /// Attempts to activate player ability in given direction for a certain DinoType, ensuring there are enough charges.
    /// </summary>
    public static void TryPlayerAbility(PlayerControls player, Vector2Int dir, DinoType type, int charges)
    {
        if (!player.TryGetComponent(out ObjectMover objMover))
            throw new Exception("Player MUST have ObjectMover component.");

        // return if out of charges
        if (charges == 0)
            return; // TODO: ability failure effect

        // do ability check depending on current dinosaur type
        switch (type) // current type
        {
            case DinoType.Stego:
                // Check for object at indicated direction of ability
                Vector2Int abilityPos = objMover.GetGlobalGridPos() + dir;
                ObjectState adjacentObj = GetObjectAtPos(player, abilityPos.x, abilityPos.y);
                if (adjacentObj is not null && VisibilityCheck.IsVisible(player, abilityPos.x, abilityPos.y)) // object present and visible
                {
                    // flip player to face what they set to quantum state (if left or right) - slightly more visual feedback
                    if (dir == Vector2Int.right)
                        player.SetFacingRight(true);
                    else if (dir == Vector2Int.left)
                        player.SetFacingRight(false);

                    // mark object as quantum (or unmark)
                    adjacentObj.ToggleQuantum();

                    // decrement charges
                    player.UseAbilityCharge();
                    // action successful (save undo frame)
                    UndoHandler.SaveFrame();
                    return;
                }
                else
                {
                    // play ability failure effect

                    return;
                }
            case DinoType.Trike:
                return; // unimplemented
            case DinoType.Anky:
                return; // unimplemented
            case DinoType.Dilo:
                return; // unimplemented
            case DinoType.Bary:
                return; // unimplemented
            case DinoType.Ptero:
                return; // unimplemented
            case DinoType.Compy:
                return; // unimplemented
            case DinoType.Pachy:
                return; // unimplemented
        }

        throw new Exception("Issue with TryPlayerAbility. Should have returned at some point but did not.");
    }
    #endregion

    /// <summary>
    /// Returns the ObjectsStats component of the object at the specified grid position (within the same panel as the player).
    /// In the case of two objects on the same grid position, returns the topmost (i.e. log on water w/ log/rock)
    /// </summary>
    public static ObjectState GetObjectAtPos(PlayerControls player, int x, int y)
    {
        //List<ObjectState> objectsAtPos = new List<ObjectState>();
        if (player.transform.parent is not null)
        {
            // iterate through sibling objects checking for position
            ObjectState[] siblingObjects = player.transform.parent.GetComponentsInChildren<ObjectState>();
            foreach (ObjectState obj in siblingObjects)
            {
                if (obj.TryGetComponent(out ObjectMover objMover) && obj.TryGetComponent(out ObjectState objState))
                {
                    Vector2Int pos = objMover.GetGlobalGridPos();
                    if (pos.x == x && pos.y == y && !objState.ObjData.IsDisabled)
                        return obj;
                        //objectsAtPos.Add(obj);
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
