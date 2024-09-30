using System;
using System.Collections.Generic;
using UnityEngine;
using static PlayerControls;

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

        if (!player.TryGetComponent(out Mover playerObjMover))
            throw new Exception("Player object MUST have Mover component.");

        Vector2Int currPos = playerObjMover.GetGlobalGridPos();
        Vector2Int targetPos = currPos + moveDir;
        
        // Ensure player current visibility
        if (!VisibilityChecks.IsVisible(player, currPos.x, currPos.y))
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
            if (!VisibilityChecks.IsVisible(player, targetPos.x, targetPos.y))
            {
                // try to find which panel is visible at target/adjacent pos
                for (int i = 0; i <= SortingOrderHandler.MaxPanelOrder; i++)
                {
                    // Topmost panel at pos found
                    if (VisibilityChecks.IsVisible(i, targetPos.x, targetPos.y))
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
                                if (!pushedPanel.TryGetComponent(out Mover panelMover))
                                    throw new Exception("ALL subpanels MUST have an Mover component.");

                                // shuffle quantum objects just before moving panel
                                QuantumState.ShuffleHiddenQuantumObjects();
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
            QuantumState objState = VisibilityChecks.GetObjectAtPos(playerObjMover, targetPos.x, targetPos.y);
            if (objState is null)
            {
                // Move action complete
                ConfirmPlayerMove(playerObjMover, moveDir);
                return;
            }

            // Player collides immediately with WHICH object type??
            switch(objState.ObjData.ObjType)
            {
                case ObjectType.Log:

                    // push logs as possible
                    if(PushLogsInSeries(playerObjMover, playerObjMover.GetGlobalGridPos() + moveDir, moveDir))
                        ConfirmPlayerMove(playerObjMover, moveDir); // move if logs were actually pushed

                    // action attempt is complete whether move happened or not
                    return;
                case ObjectType.Water:
                    // allow movement if water has an object in it
                    if(objState.ObjData.WaterHasLog || objState.ObjData.WaterHasRock)
                    {
                        // Move action complete
                        ConfirmPlayerMove(playerObjMover, moveDir);
                        return;
                    }

                    return;
                case ObjectType.Rock:
                    return; // CANNOT move into rocks
                case ObjectType.TallRock:
                    return; // CANNOT move into tall rocks
                case ObjectType.Bush:
                    return; // unimplemented
                case ObjectType.TallBush:
                    return; // unimplemented
                case ObjectType.Tunnel:
                    
                    // require the player to be moving UP into the tunnel (other directions are just an obstruction)
                    if (moveDir != Vector2Int.up)
                        return;

                    // retrieve mover component of other tunnel
                    QuantumState otherTunnel = objState.ObjData.OtherTunnel;
                    if (!otherTunnel.TryGetComponent(out Mover otherTunnelMover))
                        throw new Exception("Level Objects MUST have a mover component.");
                    
                    // Requirement: tunnel is visible
                    Vector2Int otherTunnelPos = otherTunnelMover.GetGlobalGridPos();
                    if (!VisibilityChecks.IsVisible(otherTunnel.gameObject, otherTunnelPos.x, otherTunnelPos.y))
                        return;
                    
                    // Requirement: visibility of exit pos
                    Vector2Int exitPos = otherTunnelPos + Vector2Int.down;
                    if (!VisibilityChecks.IsVisible(otherTunnel.gameObject, exitPos.x, exitPos.y))
                        return;

                    // Requirement: exit pos is clear of obstructions (anything)
                    QuantumState exitObj = VisibilityChecks.GetObjectAtPos(otherTunnelMover, exitPos.x, exitPos.y);
                    if (exitObj is not null)
                        return;
                    
                    // ALL PRECONDITIONS ARE MET -> player can move

                    // Check for log sinking before moving through tunnel
                    QuantumState logSinkCheck = VisibilityChecks.GetObjectAtPos(playerObjMover, currPos.x, currPos.y);
                    if (logSinkCheck != null && logSinkCheck.ObjData.ObjType == ObjectType.Water && logSinkCheck.ObjData.WaterHasLog)
                        logSinkCheck.ObjData.WaterHasLog = false;

                    // move player to panel of the other tunnel
                    player.transform.parent = otherTunnel.transform.parent;

                    // move player to position beneath other tunnel
                    playerObjMover.SetGlobalGoal(exitPos.x, exitPos.y);

                    // visually flip the player
                    PlayerSpriteSwapper spriteSwapper = player.GetComponentInChildren<PlayerSpriteSwapper>();
                    if (spriteSwapper is null)
                        throw new Exception("Player MUST have PlayerSpriteSwapper component.");
                    spriteSwapper.RequireFlip();

                    // completed player movement action
                    UndoHandler.SaveFrame();
                    
                    return;
                case ObjectType.Clock:
                    // confirm move FIRST
                    ConfirmPlayerMove(playerObjMover, moveDir);
                    // then make call for transitioning level
                    // TODO: will be replaced instead later with:
                    // 1. updating completed level state in game manager
                    // 2. transition BACK to level select at pos. indicated by current compelted level
                    SceneTransitionHelper.LoadNextScene();

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
    private static void ConfirmPlayerMove(Mover objMover, Vector2Int moveDir)
    {
        // If player was on log, sink log along with player movement
        Vector2Int currPos = objMover.GetGlobalGridPos();
        QuantumState logSinkCheck = VisibilityChecks.GetObjectAtPos(objMover, currPos.x, currPos.y);
        if (logSinkCheck != null && logSinkCheck.ObjData.ObjType == ObjectType.Water && logSinkCheck.ObjData.WaterHasLog)
            logSinkCheck.ObjData.WaterHasLog = false;

        // move player
        objMover.Increment(moveDir);

        // completed player movement action
        UndoHandler.SaveFrame();
    }

    /// <summary>
    /// Handles iterating through adjacent log objects to see how many in a series can be pushed. 
    /// Accounts for interactions with water as necessary.
    /// Includes bool crushEndLog so this can be reused during strong push ability.
    /// Returns true/false whether a change in log states (push/crush) actually occurred).
    /// </summary>
    /// <param name="mover">Any Mover component on the same panel as the logs being pushed.</param>
    private static bool PushLogsInSeries(Mover mover, Vector2Int initialCheckPos, Vector2Int dir, bool crushEndLog = false)
    {
        // check for first log
        Vector2Int adjacentPos = initialCheckPos;
        QuantumState adjacentObj = VisibilityChecks.GetObjectAtPos(mover, adjacentPos.x, adjacentPos.y);
        if (adjacentObj.ObjData.ObjType != ObjectType.Log)
            return false;

        // generate list of all logs to be pushed by the potential player move
        List<QuantumState> logs = new List<QuantumState>();
        logs.Add(adjacentObj);

        // Find all logs in a chain
        bool currIsLog = true;
        bool obstructed = true;
        while (currIsLog)
        {
            adjacentPos += dir;

            // Check for log's obstruction by higher-ordered panel
            if (!VisibilityChecks.IsVisible(mover.gameObject, adjacentPos.x, adjacentPos.y))
                break; // no more logs to add, we hit a panel

            // check for object at next position
            adjacentObj = VisibilityChecks.GetObjectAtPos(mover, adjacentPos.x, adjacentPos.y);
            if (adjacentObj is null) // no object blocking the log
            {
                currIsLog = false;
                obstructed = false; // allow push
            }
            else if (adjacentObj.ObjData.ObjType == ObjectType.Log) // add another log, then keep checking for more
                logs.Add(adjacentObj);
            else if (adjacentObj.ObjData.ObjType == ObjectType.Water)
            {
                // log can be pushed over water with rock/log
                if (adjacentObj.ObjData.WaterHasRock || adjacentObj.ObjData.WaterHasLog)
                {
                    currIsLog = false;
                    obstructed = false; // allow push
                }
                else // if water has nothing in it, add log
                {
                    // exit loop
                    currIsLog = false;
                    obstructed = false; // allow push

                    // update water data state
                    adjacentObj.ObjData.WaterHasLog = true;

                    // make water quantum if log was (transferring state)
                    if (logs[logs.Count - 1].IsQuantum())
                        adjacentObj.SetQuantum(true);

                    // disable last most log in the list (it was pushed into water!)
                    logs[logs.Count - 1].ObjData.IsDisabled = true;
                }
            }
            else if(adjacentObj.ObjData.ObjType == ObjectType.Clock)
            {
                // exit loop
                currIsLog = false;
                obstructed = false; // allow push

                // disable last most log in the list (it was pushed into clock rift)
                logs[logs.Count - 1].ObjData.IsDisabled = true;
            }
            else // obstructed rock/tallRock/bush/tallBush/Tunnel
                break;
        }

        if(!obstructed) // move logs normally
        {
            // move logs
            foreach (QuantumState log in logs)
            {
                // increment each log
                if (log.TryGetComponent(out Mover logMover))
                    logMover.Increment(dir);
                else
                    throw new Exception("All log objects MUST have an Mover component");
            }

            // indicate logs have been pushed (original adjacentPos is now open)
            return true;
        }
        else if(crushEndLog) // with obstruction, push then crush end log
        {
            // increment each log UNTIL the last
            for(int i = 0; i < logs.Count-1; i++)
            {
                // increment each log
                if (logs[i].TryGetComponent(out Mover logMover))
                    logMover.Increment(dir);
                else
                    throw new Exception("All log objects MUST have an Mover component");
            }
            // crush/disable final log in list
            logs[logs.Count - 1].ObjData.IsDisabled = true;

            // indicate logs have been pushed (original adjacentPos is now open)
            return true;
        }

        // no actual change has occurred
        return false;
    }
    #endregion

    #region PLAYER ABILITY CHECKS
    /// <summary>
    /// Attempts to activate player ability in given direction for a certain DinoType, ensuring there are enough charges.
    /// </summary>
    public static void TryPlayerAbility(PlayerControls player, Vector2Int dir, DinoType type, int charges)
    {
        if (!player.TryGetComponent(out Mover objMover))
            throw new Exception("Player MUST have Mover component.");

        // return if out of charges
        if (charges == 0)
            return; // TODO: ability failure effect

        // do ability check depending on current dinosaur type
        switch (type) // current type
        {
            case DinoType.Stego:
                TryStegoAbility(player, objMover, dir);
                break;
            case DinoType.Trike:
                TryTrikeAbility(player, objMover, dir);
                break; // unimplemented
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
    }

    /// <summary>
    /// Handles QUANTUM ABILITY of Stegosaurus.
    /// Marks (or unmarks) adjacent object as quantum.
    /// </summary>
    private static void TryStegoAbility(PlayerControls player, Mover mover, Vector2Int dir)
    {
        // Check for object at indicated direction of ability (immediate neighbor)
        Vector2Int abilityPos = mover.GetGlobalGridPos() + dir;
        QuantumState adjacentObj = VisibilityChecks.GetObjectAtPos(mover, abilityPos.x, abilityPos.y);

        // object present and visible
        if (adjacentObj is not null && VisibilityChecks.IsVisible(player, abilityPos.x, abilityPos.y))
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
    }

    /// <summary>
    /// Handles STRONG PUSH ability of Triceratops.
    /// Pushing logs: behaves same as standard movement pushing, except will crush the end log in the case of an obstruction.
    /// Pushing rocks: can push rocks in series, followed by logs if necessary, crushing logs between other rocks/obstructions.
    /// </summary>
    private static void TryTrikeAbility(PlayerControls player, Mover mover, Vector2Int dir)
    {
        // Check for object at indicated direction of ability (immediate neighbor)
        Vector2Int adjacentPos = mover.GetGlobalGridPos() + dir;
        QuantumState adjacentObj = VisibilityChecks.GetObjectAtPos(mover, adjacentPos.x, adjacentPos.y);

        // object present and visible AND rock
        if (adjacentObj is not null && VisibilityChecks.IsVisible(player, adjacentPos.x, adjacentPos.y))
        {
            if (adjacentObj.ObjData.ObjType == ObjectType.Log)
            {
                // push/crush logs
                PushLogsInSeries(mover, adjacentPos, dir, true);

                // decrement charges
                player.UseAbilityCharge();
                // action successful (save undo frame)
                UndoHandler.SaveFrame();
                return;
            }
            else if (adjacentObj.ObjData.ObjType == ObjectType.Rock)
            {
                // generate list of all logs to be pushed by the potential player move
                List<QuantumState> rocks = new List<QuantumState>();
                rocks.Add(adjacentObj);

                // Find all rocks in a chain
                bool currIsRock = true;
                while (currIsRock)
                {
                    adjacentPos += dir;

                    // Check for rock's obstruction by higher-ordered panel
                    if (!VisibilityChecks.IsVisible(player, adjacentPos.x, adjacentPos.y))
                        return; // obstruction to rock = NO PUSH/ACTION

                    // check for object at next position
                    adjacentObj = VisibilityChecks.GetObjectAtPos(mover, adjacentPos.x, adjacentPos.y);
                    if (adjacentObj is null) // no object blocking the log
                        currIsRock = false;
                    else if (adjacentObj.ObjData.ObjType == ObjectType.Rock) // add another log, then keep checking for more
                        rocks.Add(adjacentObj);
                    else if (adjacentObj.ObjData.ObjType == ObjectType.Water) // sinking check handled later
                        currIsRock = false;
                    else if (adjacentObj.ObjData.ObjType == ObjectType.Log)
                    {
                        // exit loop
                        currIsRock = false;

                        // push/crush logs
                        PushLogsInSeries(mover, adjacentPos, dir, true);
                    }
                    else if (adjacentObj.ObjData.ObjType == ObjectType.Clock)
                    {
                        // exit loop
                        currIsRock = false;

                        // destroy rock pushed into clock vortex
                        rocks[rocks.Count - 1].ObjData.IsDisabled = true;
                    }
                    else // obstructed bush/tallBush/Tunnel
                        return; // obstruction to rock = NO PUSH/ACTION
                }

                // if we got this far, rock push is guaranteed
                foreach (QuantumState rock in rocks)
                {
                    // increment each log
                    if (rock.TryGetComponent(out Mover rockMover))
                    {
                        rockMover.Increment(dir);

                        // check for rock sinking into water (if a rock is not already there)
                        Vector2Int rockPos = rockMover.GetGlobalGridPos();
                        QuantumState lowerObjCheck = VisibilityChecks.GetObjectAtPos(mover, rockPos.x, rockPos.y, true);
                        if(lowerObjCheck.ObjData.ObjType == ObjectType.Water && !lowerObjCheck.ObjData.WaterHasRock)
                        {
                            lowerObjCheck.ObjData.WaterHasLog = false;
                            lowerObjCheck.ObjData.WaterHasRock = true;

                            // make water quantum if rock was (transferring state)
                            if (rocks[rocks.Count - 1].IsQuantum())
                                adjacentObj.SetQuantum(true);

                            rock.ObjData.IsDisabled = true;
                        }
                    }
                    else
                        throw new Exception("All log objects MUST have an Mover component");
                }

                // decrement charges
                player.UseAbilityCharge();
                // action successful (save undo frame)
                UndoHandler.SaveFrame();
                return;
            }
        }
    }
    #endregion
}
