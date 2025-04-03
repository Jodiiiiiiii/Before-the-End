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

            // Start of object checks
            QuantumState adjacentObj = VisibilityChecks.GetObjectAtPos(playerMover, targetPos.x, targetPos.y);

            // SWIMMING CHECK: do swimming version of move checks instead of standard move checks below?
            if (player.IsSwimming)
            {
                TrySwimmingMove(player, playerMover, moveDir);
                return;
            }

            // NO OBSTRUCTION
            if (adjacentObj is null)
            {
                // Move action complete -> no obstruction
                ConfirmPlayerMove(player, playerMover, moveDir);

                // move audio
                AudioManager.Instance.PlayMove();

                return;
            }

            // HANDLE SPECIFIC OBSTRUCTION TYPE
            switch(adjacentObj.ObjData.ObjType)
            {
                case ObjectType.Log:
                    TryMoveIntoLog(player, playerMover, moveDir);
                    // action attempt is complete whether move happened or not
                    return;
                case ObjectType.Water:
                    TryMoveIntoWater(player, playerMover, moveDir, adjacentObj);
                    // action attempt is complete whether move happened or not
                    return;
                case ObjectType.Rock:
                    return; // CANNOT move into rocks
                case ObjectType.Bush:
                    return; // CANNOT move into bushes
                case ObjectType.Tunnel:
                    TryMoveIntoTunnel(player, playerMover, moveDir, adjacentObj);
                    // action attempt is complete whether move happened or not
                    return;
                case ObjectType.Tree:
                    return; // CANNOT move into trees
                case ObjectType.Clock:
                    // player moves into object, which is vertically shrunk to 0 (destroyed)
                    adjacentObj.ObjData.IsDisabled = true;
                    ConfirmPlayerMove(player, playerMover, moveDir);

                    player.LevelComplete();
                    // action attempt is complete (and the entire level!)
                    return;
                case ObjectType.Fire:
                    return; // CANNOT move into fire
                case ObjectType.Void:
                    return; // CANNOT move into void
                case ObjectType.Compy:
                    // can ALWAYS move into compy
                    player.CollectCompy();
                    ConfirmPlayerMove(player, playerMover, moveDir);
                    // action attempt is complete (and the entire level)
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

                        // play push SFX
                        AudioManager.Instance.PlayPushPanel();

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
    /// Attempts to move within water to other water tiles that are not obstructed by contained rocks or logs.
    /// If attempting to move into a submerged log, then it will attempt to push it in series.
    /// </summary>
    private static void TrySwimmingMove(PlayerControls player, Mover playerMover, Vector2Int moveDir)
    {
        // fetch first water object
        Vector2Int currPos = playerMover.GetGlobalGridPos();
        Vector2Int targetPos = currPos + moveDir;
        QuantumState adjacentObj = VisibilityChecks.GetObjectAtPos(playerMover, targetPos.x, targetPos.y, true);
        QuantumState adjacentTopObj = VisibilityChecks.GetObjectAtPos(playerMover, targetPos.x, targetPos.y, false);

        // REQUIREMENT: there MUST be a tile and it MUST be water
        if (adjacentObj is null || adjacentObj.ObjData.ObjType != ObjectType.Water)
            return;

        // REQUIREMENT: cannot move into submerged rock
        if (adjacentObj.ObjData.WaterHasRock)
            return;

        // no obstruction! (empty water)
        if (!adjacentObj.ObjData.WaterHasLog)
        {
            // swim SFX
            AudioManager.Instance.PlaySwim();

            // Move action complete -> no obstruction
            ConfirmPlayerMove(player, playerMover, moveDir);
            return;
        }
        // Pushable submerged logs?
        else
        {
            Vector2Int adjacentPos = playerMover.GetGlobalGridPos() + moveDir;

            // generate list of all logs to be pushed by the potential player move
            List<QuantumState> pushList = new List<QuantumState>();
            pushList.Add(adjacentObj);
            
            // top list contains all logs and compy pair objects that are pushed on top of some log being pushed
            List<QuantumState> topObjList = new List<QuantumState>();
            if (adjacentObj != adjacentTopObj)
                topObjList.Add(adjacentTopObj);

            // Find all logs in a series
            bool currIsLog = true;
            while (currIsLog)
            {
                adjacentPos += moveDir;
                adjacentObj = VisibilityChecks.GetObjectAtPos(playerMover, adjacentPos.x, adjacentPos.y, true); // fetch lower object (in case of object on top)
                adjacentTopObj = VisibilityChecks.GetObjectAtPos(playerMover, adjacentPos.x, adjacentPos.y, false); // fetch lower object (in case of object on top)

                // REQUIREMENT: not pushing submerged log into panel
                if (!VisibilityChecks.IsVisible(playerMover.gameObject, adjacentPos.x, adjacentPos.y))
                    return; // obstructed -> cannot move/push

                // REQUIREMENT: Can ONLY push into adjacent water
                // REQUIREMENT: Can NOT push into submerged rocks
                if (adjacentObj is null || adjacentObj.ObjData.ObjType != ObjectType.Water || adjacentObj.ObjData.WaterHasRock)
                    return; // obstructed -> cannot move/push

                // add another log to list to push, then loop again
                if (adjacentObj.ObjData.WaterHasLog)
                {
                    pushList.Add(adjacentObj);
                }
                // no more obstruction! -> can push/move
                else
                {
                    pushList.Add(adjacentObj); // add final empty water tile to add a log into

                    currIsLog = false;
                }

                // add top objects to list
                if (adjacentObj != adjacentTopObj)
                    topObjList.Add(adjacentTopObj);
            }

            // if we have gotten this far, the logs can be all pushed, AND the player can move

            // shift quantum and log state values between log chain
            bool prevQuantum = false;
            bool prevHasLog = false;
            for (int i = 0; i < pushList.Count; i++)
            {
                // swap operations
                bool currQuantum = pushList[i].IsQuantum();
                bool currHasLog = pushList[i].ObjData.WaterHasLog;

                // for last element (empty water) - take quantum of previous log OR of current water
                // i.e. pushing log into quantum water creates a quantum submerged log (better internal consistency of rules)
                if (i == pushList.Count - 1)
                    pushList[i].SetQuantum(pushList[i].IsQuantum() || prevQuantum);
                // normal behavior for all other logs (just transfer the previous state)
                else
                    pushList[i].SetQuantum(prevQuantum);

                pushList[i].ObjData.WaterHasLog = prevHasLog;

                prevQuantum = currQuantum;
                prevHasLog = currHasLog;

                // visually flip ALL logs (ignore the first one which just becomes water
                if (i == pushList.Count-1)
                {
                    ObjectSpriteSwapper flipper = pushList[i].GetComponentInChildren<ObjectSpriteSwapper>();
                    if (flipper is null)
                        throw new Exception("All level objects MUST have ObjectSpriteSwapper component on a child object.");
                    flipper.RequireFlip();
                }

            }

            // push top objects accordingly (logs and compy)
            for (int i = 0; i < topObjList.Count; i++)
            {
                topObjList[i].ObjMover.Increment(moveDir);
            }

            // play swim SFX
            AudioManager.Instance.PlaySwim();

            // confirm movement of player
            ConfirmPlayerMove(player, playerMover, moveDir);
            return;
        }
    }

    /// <summary>
    /// Attempts to push logs in series, moving the player.
    /// </summary>
    private static void TryMoveIntoLog(PlayerControls player, Mover playerMover, Vector2Int moveDir)
    {
        // push logs as possible
        if (PushLogsInSeries(playerMover, playerMover.GetGlobalGridPos() + moveDir, moveDir))
        {
            ConfirmPlayerMove(player, playerMover, moveDir); // move if logs were actually pushed
        }
    }

    /// <summary>
    /// Attempts to move onto water tile. Only possible if water has a log or a rock in it.
    /// </summary>
    private static void TryMoveIntoWater(PlayerControls player, Mover playerMover, Vector2Int moveDir, QuantumState adjacentWater)
    {
        // allow movement if water has an object in it
        if (adjacentWater.ObjData.WaterHasLog || adjacentWater.ObjData.WaterHasRock)
        {
            // Move action complete
            ConfirmPlayerMove(player, playerMover, moveDir);

            // move audio
            AudioManager.Instance.PlayMove();

            return;
        }
    }

    /// <summary>
    /// Attempts to move into tunnel. Only possible if moving up into tunnel and other tunnel is visible and clear of obstructions.
    /// </summary>
    private static void TryMoveIntoTunnel(PlayerControls player, Mover playerMover, Vector2Int moveDir, QuantumState adjacentTunnel)
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

        // REQUIREMENT: exit pos is clear of obstructions (or default to more specific obstruction checks)
        QuantumState exitObj = VisibilityChecks.GetObjectAtPos(otherTunnelMover, exitPos.x, exitPos.y);
        if (exitObj is not null)
        {
            // log -> must be pushable to move
            if (exitObj.ObjData.ObjType == ObjectType.Log)
            {
                if (!PushLogsInSeries(otherTunnelMover, exitPos, Vector2Int.down))
                    return;
            }
            // water -> must contain submerged log or rock
            else if (exitObj.ObjData.ObjType == ObjectType.Water)
            {
                if (!exitObj.ObjData.WaterHasLog && !exitObj.ObjData.WaterHasRock)
                    return;
            }
            else if (exitObj.ObjData.ObjType == ObjectType.Compy)
            {
                // college compy before handling motion
                if (!playerMover.TryGetComponent(out PlayerControls playerControls))
                    throw new Exception("Player MUST have PlayerControls script.");
                playerControls.CollectCompy();

                // no return since moving into a compy will ALWAYS work
            }
            else // other object -> never traversable
                return;
        }

        // ALL PRECONDITIONS ARE MET -> player can move

        // visually flip the player
        PlayerSpriteSwapper spriteSwapper = playerMover.GetComponentInChildren<PlayerSpriteSwapper>();
        if (spriteSwapper is null)
            throw new Exception("Player MUST have PlayerSpriteSwapper component.");
        spriteSwapper.RequireFlip();

        // LOG SINKING (when player leaves floating log)
        // MUST CHECK THIS HERE (instead of in ConfirmPlayerMove) DUE TO TUNNEL SNAPPING BEHAVIOR
        Vector2Int currPos = playerMover.GetGlobalGridPos();
        QuantumState logSinkCheck = VisibilityChecks.GetObjectAtPos(playerMover, currPos.x, currPos.y);
        if (logSinkCheck is not null && logSinkCheck.ObjData.ObjType == ObjectType.Water && logSinkCheck.ObjData.WaterHasLog)
            logSinkCheck.ObjData.WaterHasLog = false;

        // snap to tunnel position so it looks like player moves out of tunnel
        Vector2Int snapPos = otherTunnel.ObjMover.GetGlobalGridPos();
        playerMover.SetGlobalGoal(snapPos.x, snapPos.y);
        playerMover.SnapToGoal();

        // move audio
        AudioManager.Instance.PlayMove();

        // finalize player move
        ConfirmPlayerMove(player, playerMover, exitPos, otherTunnel.transform.parent);
    }
    #endregion

    #region OTHER HELPER FUNCTIONS
    /// <summary>
    /// Contains all necessary functions when player move is confirmed
    /// Handles potential log sinking (on curr position), player movement, and saving an undo frame.
    /// </summary>
    public static void ConfirmPlayerMove(PlayerControls playerControls, Mover objMover, Vector2Int moveDir)
    {
        // LOG SINKING (when player leaves floating log)
        Vector2Int currPos = objMover.GetGlobalGridPos();
        QuantumState logSinkCheck = VisibilityChecks.GetObjectAtPos(objMover, currPos.x, currPos.y);
        if (logSinkCheck is not null && logSinkCheck.ObjData.ObjType == ObjectType.Water && logSinkCheck.ObjData.WaterHasLog)
            logSinkCheck.ObjData.WaterHasLog = false;

        // move player
        objMover.Increment(moveDir);

        // compy pair position update
        playerControls.SnapInactiveCompyToPlayer();

        // completed player movement action
        UndoHandler.SaveFrame();
    }

    /// <summary>
    /// Contains all necessary functions when player move is confirmed between panels.
    /// Handles potential log sinking (on curr position), player movement, parent transform changing, and undo frame.
    /// Use null newParent if no parent reassignment is taking place (i.e. raptor & pteranodon abilities).
    /// </summary>
    public static void ConfirmPlayerMove(PlayerControls playerControls, Mover mover, Vector2Int movePos, Transform newParent)
    {
        // Check for log sinking before moving through tunnel
        Vector2Int currPlayerPos = mover.GetGlobalGridPos();
        QuantumState logSinkCheck = VisibilityChecks.GetObjectAtPos(mover, currPlayerPos.x, currPlayerPos.y);
        if (logSinkCheck != null && logSinkCheck.ObjData.ObjType == ObjectType.Water && logSinkCheck.ObjData.WaterHasLog)
            logSinkCheck.ObjData.WaterHasLog = false;

        // move player to panel of the other tunnel (reassign to new parent transform)
        if(newParent is not null)
            mover.transform.parent = newParent;

        // move player to position beneath other tunnel
        mover.SetGlobalGoal(movePos.x, movePos.y);

        // compy pair position update
        playerControls.SnapInactiveCompyToPlayer();

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
        if (adjacentObj is null || adjacentObj.ObjData.ObjType != ObjectType.Log)
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

                    // Only place log in water if it was NOT on fire, otherwise just destroy the log
                    if (!logs[logs.Count-1].ObjData.IsOnFire)
                    {
                        // update water data state
                        adjacentObj.ObjData.WaterHasLog = true;

                        // make water quantum if log was (transferring state)
                        if (logs[logs.Count - 1].IsQuantum())
                            adjacentObj.SetQuantum(true);

                        // visually flip (smoother appearance)
                        ObjectSpriteSwapper flipper = adjacentObj.GetComponentInChildren<ObjectSpriteSwapper>();
                        if (flipper is null)
                            throw new System.Exception("ALL Level Objects MUST have ObjectSpriteSwapper component on a child object.");
                        flipper.RequireFlip();

                        // Play water splash sound for log
                        AudioManager.Instance.PlayObjectSplash();
                    }

                    // disable last most log in the list (it was pushed into water!)
                    logs[logs.Count - 1].ObjData.IsDisabled = true;
                }
            }
            else if (adjacentObj.ObjData.ObjType == ObjectType.Fire)
            {
                // exit loop
                currIsLog = false;
                obstructed = false; // allow push

                // destroy log since it was already burning - don't change fire tile
                if(logs[logs.Count - 1].ObjData.IsOnFire)
                {
                    logs[logs.Count - 1].ObjData.IsDisabled = true;
                }
                // transfer fire from fire tile to log
                else
                {
                    // extinguish fire
                    adjacentObj.ObjData.IsDisabled = true;
                    // light log on fire
                    logs[logs.Count - 1].ObjData.IsOnFire = true;
                }
            }
            else if (adjacentObj.ObjData.ObjType == ObjectType.Bush)
            {
                if (logs[logs.Count - 1].ObjData.IsOnFire)
                {
                    // destroy log
                    logs[logs.Count - 1].ObjData.IsDisabled = true;
                    // transfer fire to bush
                    adjacentObj.ObjData.IsOnFire = true;
                    // add fire bush to fire spread handler
                    FireSpreadHandler.AddFireBush(adjacentObj);

                    // exit loop
                    currIsLog = false;
                    obstructed = false; // allow push
                }
                else // cannot push into bush if log is not on fire
                {
                    // exit loop
                    currIsLog = false;
                }
            }
            else // obstructed rock/tree/Tunnel/Clock
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

            // Play log push SFX (from movement OR Trike ability)
            AudioManager.Instance.PlayPushLog();

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

            // Play crush SFX (Trike Ability ONLY)
            AudioManager.Instance.PlayTrikeCrush();

            // indicate logs have been pushed (original adjacentPos is now open)
            return true;
        }

        // no actual change has occurred
        return false;
    }
    #endregion
}
