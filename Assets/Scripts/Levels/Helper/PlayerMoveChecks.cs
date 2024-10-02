using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerMoveChecks
{
    /// <summary>
    /// Determines if the player is able to move in the specified direction and handles all according behavior.
    /// Input MUST have only one non-zero value and it must be either -1 or 1.
    /// Accounts for visibility checks and player interactions/obstructions with different object types.
    /// </summary>
    public static void TryPlayerMove(PlayerControls player, Vector2Int moveDir)
    {
        // REQUIREMENT: moveDir is unit vector
        if (moveDir.magnitude != 1 || (moveDir.x != 1 && moveDir.x != -1 && moveDir.x != 0) || (moveDir.y != 1 && moveDir.y != -1 && moveDir.y != 0))
            throw new Exception("Input of CanMove function MUST have only one non-zero value and it must be eiether -1 or 1.");

        // REQUIREMENT: Player has Mover component
        if (!player.TryGetComponent(out Mover playerMover))
            throw new Exception("Player object MUST have Mover component.");

        Vector2Int currPos = playerMover.GetGlobalGridPos();
        Vector2Int targetPos = currPos + moveDir;
        
        // REQUIREMENT: Player is visible on current panel
        if (!VisibilityChecks.IsVisible(player, currPos.x, currPos.y))
            return;

        // SPRITE FLIPPING
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

        // try all panel pushing and moving w/ objects action possibilities
        // ensure save stack frame happens in finally section if hasFlipped is true
        try
        {
            // PANEL PUSHING
            if (!VisibilityChecks.IsVisible(player, targetPos.x, targetPos.y))
            {
                // Attempt to panel push
                TryPanelPush(player, targetPos, moveDir);
                // Player has either pushed the panel OR no change has occurred. Move action is over.
                return;
            }

            // NO OBSTRUCTION
            QuantumState adjacentObj = VisibilityChecks.GetObjectAtPos(playerMover, targetPos.x, targetPos.y);
            if (adjacentObj is null)
            {
                // Move action complete -> no obstruction
                ConfirmPlayerMove(playerMover, moveDir);
                return;
            }

            // HANDLE SPECIFIC OBSTRUCTION
            switch(adjacentObj.ObjData.ObjType)
            {
                case ObjectType.Log:
                    TryMoveIntoLog(playerMover, moveDir);
                    // action attempt is complete whether move happened or not
                    return;
                case ObjectType.Water:
                    TryMoveIntoWater(playerMover, moveDir, adjacentObj);
                    // action attempt is complete whether move happened or not
                    return;
                case ObjectType.Rock:
                    return; // CANNOT move into rocks
                case ObjectType.TallRock:
                    return; // CANNOT move into tall rocks
                case ObjectType.Bush:
                    return; // CANNOT move into bushes
                case ObjectType.TallBush:
                    return; // CANNOT move into tall bushes
                case ObjectType.Tunnel:
                    TryMoveIntoTunnel(playerMover, moveDir, adjacentObj);
                    // action attempt is complete whether move happened or not
                    return;
                case ObjectType.Clock:
                    TryMoveIntoClock(playerMover, moveDir);
                    // action attempt is complete (and the entire level!)
                    return;
            }
        }
        finally
        {
            // SPRITE FLIP SAVE FRAME (if not already saved)
            if (hasFlipped && prevFrame == UndoHandler.GetGlobalFrame())
                UndoHandler.SaveFrame();
        }
    }

    #region MOVEMENT TYPES
    /// <summary>
    /// Attempts to push (in moveDir direction) by one position the topmost panel located at targetPos.
    /// OR attempts to push current panel (in moveDir direction) - if adjacent panel is of lower order.
    /// </summary>
    /// <param name="player">used to detect current player panel.</param>
    /// <param name="targetPos">position at which the player attempted to move.</param>
    /// <param name="moveDir">unit vector direction in which the player attempted to move.</param>
    private static void TryPanelPush(PlayerControls player, Vector2Int targetPos, Vector2Int moveDir)
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
    }

    /// <summary>
    /// Attempts to push logs in series, moving the player.
    /// </summary>
    private static void TryMoveIntoLog(Mover playerMover, Vector2Int moveDir)
    {
        // push logs as possible
        if (PushLogsInSeries(playerMover, playerMover.GetGlobalGridPos() + moveDir, moveDir))
            ConfirmPlayerMove(playerMover, moveDir); // move if logs were actually pushed
    }

    /// <summary>
    /// Attempts to move onto water tile. Only possible if water has a log or a rock in it.
    /// </summary>
    private static void TryMoveIntoWater(Mover playerMover, Vector2Int moveDir, QuantumState adjacentWater)
    {
        // allow movement if water has an object in it
        if (adjacentWater.ObjData.WaterHasLog || adjacentWater.ObjData.WaterHasRock)
        {
            // Move action complete
            ConfirmPlayerMove(playerMover, moveDir);
            return;
        }

        // TODO: account for baryonyx swimming state
    }

    /// <summary>
    /// Attempts to move into tunnel. Only possible if moving up into tunnel and other tunnel is visible and clear of obstructions.
    /// </summary>
    private static void TryMoveIntoTunnel(Mover playerMover, Vector2Int moveDir, QuantumState adjacentTunnel)
    {
        // PRECONDITION REQUIREMENTS

        // require the player to be moving UP into the tunnel (other directions are just an obstruction)
        if (moveDir != Vector2Int.up)
            return;

        // retrieve mover component of other tunnel
        QuantumState otherTunnel = adjacentTunnel.ObjData.OtherTunnel;
        if (!otherTunnel.TryGetComponent(out Mover otherTunnelMover))
            throw new Exception("Level Objects MUST have a mover component.");

        // REQUIREMENT: tunnel is visible
        Vector2Int otherTunnelPos = otherTunnelMover.GetGlobalGridPos();
        if (!VisibilityChecks.IsVisible(otherTunnel.gameObject, otherTunnelPos.x, otherTunnelPos.y))
            return;

        // REQUIREMENT: visibility of exit pos
        Vector2Int exitPos = otherTunnelPos + Vector2Int.down;
        if (!VisibilityChecks.IsVisible(otherTunnel.gameObject, exitPos.x, exitPos.y))
            return;

        // REQUIREMENT: exit pos is clear of obstructions (anything)
        QuantumState exitObj = VisibilityChecks.GetObjectAtPos(otherTunnelMover, exitPos.x, exitPos.y);
        if (exitObj is not null)
            return;

        // ALL PRECONDITIONS ARE MET -> player can move

        // Check for log sinking before moving through tunnel
        Vector2Int currPlayerPos = playerMover.GetGlobalGridPos();
        QuantumState logSinkCheck = VisibilityChecks.GetObjectAtPos(playerMover, currPlayerPos.x, currPlayerPos.y);
        if (logSinkCheck != null && logSinkCheck.ObjData.ObjType == ObjectType.Water && logSinkCheck.ObjData.WaterHasLog)
            logSinkCheck.ObjData.WaterHasLog = false;

        // move player to panel of the other tunnel
        playerMover.transform.parent = otherTunnel.transform.parent;

        // move player to position beneath other tunnel
        playerMover.SetGlobalGoal(exitPos.x, exitPos.y);

        // visually flip the player
        PlayerSpriteSwapper spriteSwapper = playerMover.GetComponentInChildren<PlayerSpriteSwapper>();
        if (spriteSwapper is null)
            throw new Exception("Player MUST have PlayerSpriteSwapper component.");
        spriteSwapper.RequireFlip();

        // completed player movement action
        UndoHandler.SaveFrame();
    }

    /// <summary>
    /// confirms movement of the player and then handles loads out of current level scene.
    /// </summary>
    private static void TryMoveIntoClock(Mover playerMover, Vector2Int moveDir)
    {
        // confirm move FIRST
        ConfirmPlayerMove(playerMover, moveDir);

        // then make call for transitioning level
        // TODO: will be replaced instead later with:
        // 1. updating completed level state in game manager
        // 2. transition BACK to level select at pos. indicated by current compelted level
        SceneTransitionHelper.LoadNextScene();
    }
    #endregion

    #region OTHER HELPER FUNCTIONS
    /// <summary>
    /// Contains all necessary functions when player move is confirmed
    /// Handles potential log sinking (on curr position), player movement, and saving an undo frame.
    /// </summary>
    private static void ConfirmPlayerMove(Mover objMover, Vector2Int moveDir)
    {
        // LOG SINKING (when player leaves floating log)
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
    /// Accounts for interactions with water/obstructions as necessary.
    /// Does NOT handle saving undo frame (must be handled wherever this is called).
    /// </summary>
    /// <param name="mover">Any Mover component on the same panel as the logs being pushed.</param>
    /// <param name="crushEndLog">Modifies logic to crush the log at the end in the case of an obstruction, rather than returning false.</param>
    /// <returns>returns true/false whether a change in log states (push/crush) actually occurred.</returns>
    public static bool PushLogsInSeries(Mover mover, Vector2Int initialCheckPos, Vector2Int dir, bool crushEndLog = false)
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
            else // obstructed rock/tallRock/bush/tallBush/Tunnel/Clock
                break;
        }

        if (!obstructed) // move logs normally
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
        else if (crushEndLog) // with obstruction, push then crush end log
        {
            // increment each log UNTIL the last
            for (int i = 0; i < logs.Count - 1; i++)
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
}
