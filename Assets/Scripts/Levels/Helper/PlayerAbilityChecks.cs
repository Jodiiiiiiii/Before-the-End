using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerControls;

public static class PlayerAbilityChecks
{
    /// <summary>
    /// Attempts to activate player ability in given direction for a certain DinoType, ensuring there are enough charges.
    /// </summary>
    public static void TryPlayerAbility(PlayerControls player, Vector2Int dir, DinoType type, int charges)
    {
        if (!player.TryGetComponent(out Mover objMover))
            throw new System.Exception("Player MUST have Mover component.");

        // return if out of charges
        if (charges == 0)
            return;

        // do ability check depending on current dinosaur type
        switch (type) // current type
        {
            case DinoType.Stego:
                TryStegoAbility(player, objMover, dir);
                break;
            case DinoType.Trike:
                TryTrikeAbility(player, objMover, dir);
                break;
            case DinoType.Anky:
                TryAnkyAbility(player, objMover, dir);
                return;
            case DinoType.Spino:
                TrySpinoAbility(player, objMover, dir);
                return;
            case DinoType.Ptera:
                TryPteraAbility(player, objMover, dir);
                return;
            case DinoType.Pyro:
                TryPyroAbility(player, objMover, dir);
                return;
            case DinoType.Compy:
                TryCompyAbility(player, objMover, dir);
                return;
        }
    }

    #region DINO TYPE FUNCTIONS
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
        // CANNOT unmark quantum objects
        // CANNOT mark certain objects as quantum: clock, compy, void
        if (adjacentObj is not null && VisibilityChecks.IsVisible(player, abilityPos.x, abilityPos.y)
            && !adjacentObj.IsQuantum()
            && adjacentObj.ObjData.ObjType != ObjectType.Clock 
            && adjacentObj.ObjData.ObjType != ObjectType.Compy 
            && adjacentObj.ObjData.ObjType != ObjectType.Void
            && adjacentObj.ObjData.ObjType != ObjectType.Tree)
        {
            // mark object as quantum (or unmark)
            adjacentObj.ToggleQuantum();

            // set facing direction
            FaceDirection(player, dir);
            // decrement charges
            if (adjacentObj.IsQuantum()) // consume charge ONLY if an object was marked as quantum, not if it was unmarked - this is redundant now
                player.UseAbilityCharge();

            // SFX
            AudioManager.Instance.PlayStegoAbility();

            // action successful (save undo frame)
            UndoHandler.SaveFrame();
            return;
        }
        else
        {
            // play ability failure effect
            AbilityFailureVFXManager.PlayFailureVFX(abilityPos);

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

        // REQUIREMENT: object must be present AND visible.
        // REQUIREMENT: object MUST be EITHER a log OR a rock.
        if (adjacentObj is null || !VisibilityChecks.IsVisible(player, adjacentPos.x, adjacentPos.y)
            || (adjacentObj.ObjData.ObjType != ObjectType.Log && adjacentObj.ObjData.ObjType != ObjectType.Rock))
        {
            // failure effect at adjacent tile
            AbilityFailureVFXManager.PlayFailureVFX(adjacentPos);

            return;
        }

        // log = guaranteed success
        if (adjacentObj.ObjData.ObjType == ObjectType.Log)
        {
            // push/crush logs
            PlayerMoveChecks.PushLogsInSeries(mover, adjacentPos, dir, true);

            // set facing direction
            FaceDirection(player, dir);
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
                {
                    // failure effect on object which cannot be pushed due to higher order panel
                    AbilityFailureVFXManager.PlayFailureVFX(adjacentPos);

                    return; // obstruction to rock = NO PUSH/ACTION
                }

                // check for object at next position
                adjacentObj = VisibilityChecks.GetObjectAtPos(mover, adjacentPos.x, adjacentPos.y);
                if (adjacentObj is null) // no object blocking the log
                    currIsRock = false;
                else if (adjacentObj.ObjData.ObjType == ObjectType.Rock) // add another log, then keep checking for more
                    rocks.Add(adjacentObj);
                else if (adjacentObj.ObjData.ObjType == ObjectType.Water) // sinking check handled later
                    currIsRock = false;
                else if (adjacentObj.ObjData.ObjType == ObjectType.Fire) // extinguish fire
                {
                    // exit loop
                    currIsRock = false;

                    // extinguish fire
                    adjacentObj.ObjData.IsDisabled = true;
                }
                else if (adjacentObj.ObjData.ObjType == ObjectType.Log)
                {
                    // exit loop
                    currIsRock = false;

                    // push/crush logs
                    PlayerMoveChecks.PushLogsInSeries(mover, adjacentPos, dir, true);
                }
                else // obstructed by bush/tallBush/Tunnel/etc.
                {
                    // failure effect on object which cannot be pushed at end of chain
                    AbilityFailureVFXManager.PlayFailureVFX(adjacentPos);

                    return; // obstruction to rock = NO PUSH/ACTION
                }

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
                    if (lowerObjCheck.ObjData.ObjType == ObjectType.Water && !lowerObjCheck.ObjData.WaterHasRock)
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
                    throw new System.Exception("All log objects MUST have an Mover component");
            }

            // set facing direction
            FaceDirection(player, dir);
            // decrement charges
            player.UseAbilityCharge();
            // action successful (save undo frame)
            UndoHandler.SaveFrame();
            return;
        }
    }

    /// <summary>
    /// Handles ROCK BREAK ability of Ankylosaurus.
    /// Can destroy normal rock (simply disables it) AND can destroy a rock within water, converting it back to a normal water tile.
    /// </summary>
    private static void TryAnkyAbility(PlayerControls player, Mover mover, Vector2Int dir)
    {
        // Check for object at indicated direction of ability (immediate neighbor)
        Vector2Int adjacentPos = mover.GetGlobalGridPos() + dir;
        QuantumState adjacentObj = VisibilityChecks.GetObjectAtPos(mover, adjacentPos.x, adjacentPos.y);

        // REQUIREMENT: adjacent object must be present AND visible.
        if (adjacentObj is null || !VisibilityChecks.IsVisible(player, adjacentPos.x, adjacentPos.y))
        {
            // failure effect at adjacent tile
            AbilityFailureVFXManager.PlayFailureVFX(adjacentPos);

            return;
        }

        // rock
        if (adjacentObj.ObjData.ObjType == ObjectType.Rock)
        {
            // destroy rock
            adjacentObj.ObjData.IsDisabled = true;
        }
        // submerged rock
        else if (adjacentObj.ObjData.ObjType == ObjectType.Water && adjacentObj.ObjData.WaterHasRock)
        {
            // return water to simply water state
            adjacentObj.ObjData.WaterHasRock = false;
        }
        // invalid adjacent object type
        else
        {
            // failure effect at adjacent tile - can only destroy rock
            AbilityFailureVFXManager.PlayFailureVFX(adjacentPos);

            return;
        }

        // always flip player to indicate dino swinging its tail
        player.SetFacingRight(!player.IsFacingRight());

        // decrement charges
        player.UseAbilityCharge();

        // SFX
        AudioManager.Instance.PlayAnkyAbility();

        // action successful (save undo frame)
        UndoHandler.SaveFrame();
        return;
    }

    /// <summary>
    /// Handles SWIMMING ability of the Spinosaurus.
    /// If not in water: checks for entering water.
    /// If in water: checks for exiting water.
    /// </summary>
    private static void TrySpinoAbility(PlayerControls player, Mover mover, Vector2Int dir)
    {
        // Check for object at indicated direction of ability (immediate neighbor)
        Vector2Int adjacentPos = mover.GetGlobalGridPos() + dir;
        QuantumState adjacentObj = VisibilityChecks.GetObjectAtPos(mover, adjacentPos.x, adjacentPos.y);

        // Check for ENTERING water
        if (!player.IsSwimming)
        {
            // REQUIREMENT: adjacent object must be present AND visible.
            // REQUIREMENT: Adjacent object is water AND without any submerged object
            if (adjacentObj is not null && VisibilityChecks.IsVisible(player, adjacentPos.x, adjacentPos.y) &&
                adjacentObj.ObjData.ObjType == ObjectType.Water && !adjacentObj.ObjData.WaterHasLog && !adjacentObj.ObjData.WaterHasRock)
            {
                // visually flip player sprite
                PlayerSpriteSwapper flipper = player.GetComponentInChildren<PlayerSpriteSwapper>();
                if (flipper is null)
                    throw new Exception("Player must have PlayerSpriteSwapper component on one of its children.");
                flipper.RequireFlip();

                // swim!
                player.IsSwimming = true;

                // set facing direction
                FaceDirection(player, dir);

                // enter water SFX
                AudioManager.Instance.PlaySpinoEnter();

                // move player into water
                PlayerMoveChecks.ConfirmPlayerMove(player, mover, dir);
            }
            else
            {
                // failure effect at adjacent tile - there must be viable water adjacent to player
                AbilityFailureVFXManager.PlayFailureVFX(adjacentPos);

                return;
            }
        }
        // Check for EXITING water
        else
        {
            // REQUIREMENT: adjacentPos MUST be visible
            // OPTION 1: moving into open space out of water
            // OPTION 2: moving out of water into pushable logs
            // OPTION 3: moving out of water onto submerged log OR submerged rock
            // OPTION 4: moving out of water into compy
            if (VisibilityChecks.IsVisible(player, adjacentPos.x, adjacentPos.y) &&
                (adjacentObj is null
                || PlayerMoveChecks.PushLogsInSeries(mover, adjacentPos, dir)
                || (adjacentObj is not null && adjacentObj.ObjData.ObjType == ObjectType.Water && (adjacentObj.ObjData.WaterHasLog || adjacentObj.ObjData.WaterHasRock)))
                || adjacentObj is not null && adjacentObj.ObjData.ObjType == ObjectType.Compy)
            {
                // un-swim!
                player.IsSwimming = false;

                // compy check
                if (adjacentObj is not null && adjacentObj.ObjData.ObjType == ObjectType.Compy)
                {
                    // college compy before handling motion
                    player.CollectCompy();
                }

                // visually flip player sprite
                PlayerSpriteSwapper flipper = player.GetComponentInChildren<PlayerSpriteSwapper>();
                if (flipper is null)
                    throw new Exception("Player must have PlayerSpriteSwapper component on one of its children.");
                flipper.RequireFlip();

                // set facing direction
                FaceDirection(player, dir);
                // decrement charges (only on water exit)
                player.UseAbilityCharge();

                // play exit water SFX
                AudioManager.Instance.PlaySpinoExit();

                // move player out of water
                PlayerMoveChecks.ConfirmPlayerMove(player, mover, dir);
            }
            else
            {
                // failure effect at adjacent tile - must be viable non-water tile to exit water
                AbilityFailureVFXManager.PlayFailureVFX(adjacentPos);

                return;
            }
        }
    }

    private const int MAX_PTERA_TILES = 3; // max tiles that the pteranodon can traverse in a single fly action

    /// <summary>
    /// Attempts to relocate player to a different panel at the position next to the player.
    /// Requires there to be no obstructions (no object OR submerged log/rock)
    /// </summary>
    private static void TryPteraAbility(PlayerControls player, Mover mover, Vector2Int dir)
    {
        Vector2Int adjacentPos = mover.GetGlobalGridPos(); // define adjacent posS - to be iterated on later
        QuantumState nextObj;
        PanelStats mainPanel = SortingOrderHandler.GetPanelOfOrder(0); // used for checks of in bounds of main panel

        int tilesToMove = 1;
        while(true)
        {
            adjacentPos += dir;

            // Check for not enough charges
            if (tilesToMove > MAX_PTERA_TILES)
            {
                // ability failure effect at adjacentPos - not able to find landable position
                AbilityFailureVFXManager.PlayFailureVFX(adjacentPos - dir); // subtract one so it shows issue on the FINAL possible traversable tile, not one extra away

                return;
            }

            // Check for flying outside of main panel
            if (!mainPanel.IsPosInBounds(adjacentPos.x, adjacentPos.y))
            {
                // ability failure effect at adjacentPos (or one less dir of it so its not out of bounds?)
                AbilityFailureVFXManager.PlayFailureVFX(adjacentPos);

                return;
            }

            // player has charges and it is within the main panel -> lets check the object

            // Find topmost panel at adjacent pos
            int topMostIndex = -1;
            for (int i = 0; i <= SortingOrderHandler.MaxPanelOrder; i++)
            {
                // Topmost panel at pos found
                if (VisibilityChecks.IsVisible(i, adjacentPos.x, adjacentPos.y))
                {
                    topMostIndex = i;
                    break;
                }
            }
            // To ensure topMOstIndex counts as initialized, but to filter out garbage values
            if (topMostIndex == -1) throw new Exception("How was this reached? topmost panel should always be found here in the Ptera Ability.");

            // retrieve object on topmost panel
            nextObj = VisibilityChecks.GetObjectAtPos(topMostIndex, adjacentPos.x, adjacentPos.y);

            // check for landability
            // OPTION 1: No obstruction
            // OPTION 2: Submerged log/rock
            // OPTION 3: landing on compy pair
            if (nextObj is null ||
                (nextObj.ObjData.ObjType == ObjectType.Water && (nextObj.ObjData.WaterHasLog || nextObj.ObjData.WaterHasRock))
                || nextObj.ObjData.ObjType == ObjectType.Compy)
            {
                // visually flip player sprite
                PlayerSpriteSwapper flipper = player.GetComponentInChildren<PlayerSpriteSwapper>();
                if (flipper is null)
                    throw new Exception("Player must have PlayerSpriteSwapper component on one of its children.");
                flipper.RequireFlip();

                // land on compy - collect compy before handling motion
                if (nextObj is not null && nextObj.ObjData.ObjType == ObjectType.Compy)
                    player.CollectCompy();

                // set facing direction
                FaceDirection(player, dir);
                // decrement charges (one charge per fly regardless of distance)
                player.UseAbilityCharge();
                // move player to adjacent panel
                Transform newParent = SortingOrderHandler.GetPanelOfOrder(topMostIndex).transform.GetChild(1).transform; // 1 = Upper Objects

                // SFX
                AudioManager.Instance.PlayPteraAbility();

                // confirm movement and save
                PlayerMoveChecks.ConfirmPlayerMove(player, mover, adjacentPos, newParent);

                return;
            }

            // leaving this logic here in case this feature may be re-introduced later
            // tree obstruction
            /*if (nextObj.ObjData.ObjType == ObjectType.Tree)
            {
                // TODO: ability failure effect at adjacentPos

                return;
            }*/

            tilesToMove++;
        } 
    }

    /// <summary>
    /// Attempts to move the player through a series of bushes, exiting into the tile after the final bush in series.
    /// Requires no osbtructions to exit the end bush (no object, traversable water, or pushable log).
    /// </summary>
    private static void TryPyroAbility(PlayerControls player, Mover mover, Vector2Int dir)
    {
        // Check for object at indicated direction of ability (immediate neighbor)
        Vector2Int adjacentPos = mover.GetGlobalGridPos() + dir;
        QuantumState adjacentObj = VisibilityChecks.GetObjectAtPos(mover, adjacentPos.x, adjacentPos.y);

        // REQUIREMENT: object must be present AND visible.
        // REQUIREMENT: object MUST be a bush.
        if (adjacentObj is null || !VisibilityChecks.IsVisible(player, adjacentPos.x, adjacentPos.y)
            || adjacentObj.ObjData.ObjType != ObjectType.Bush)
        {
            // failure effect at adjacent tile
            AbilityFailureVFXManager.PlayFailureVFX(adjacentPos);

            return;
        }

        // generate list of all bushes to pass through
        List<QuantumState> bushes = new List<QuantumState>();
        bushes.Add(adjacentObj);

        // Find all bushes in a series
        bool currIsBush = true;
        while (currIsBush)
        {
            adjacentPos += dir;

            // Check for next position's obstruction by higher-ordered panel
            if (!VisibilityChecks.IsVisible(player, adjacentPos.x, adjacentPos.y))
            {
                // failure effect indicating the position with the panel obstruction
                // (may need an additional check to see if it is in bounds of main panel)
                AbilityFailureVFXManager.PlayFailureVFX(adjacentPos);

                return; // panel obstruction -> CANNOT MOVE
            }

            // check for object at next position
            adjacentObj = VisibilityChecks.GetObjectAtPos(mover, adjacentPos.x, adjacentPos.y);
            if (adjacentObj is null) // no object blocking the log
                currIsBush = false;
            else if (adjacentObj.ObjData.ObjType == ObjectType.Bush)
            {
                bushes.Add(adjacentObj);
            }
            else if (adjacentObj.ObjData.ObjType == ObjectType.Water)
            {
                // check for traversable water
                if (adjacentObj.ObjData.WaterHasLog || adjacentObj.ObjData.WaterHasRock)
                    currIsBush = false;
                else // otherwise action fails
                {
                    // failure effect at position of the water (latest adjacentPos)
                    AbilityFailureVFXManager.PlayFailureVFX(adjacentPos);

                    return;
                }
            }
            else if (adjacentObj.ObjData.ObjType == ObjectType.Log) // exit into pushable log
            {
                // check for pushable log
                if (PlayerMoveChecks.PushLogsInSeries(mover, adjacentPos, dir))
                    currIsBush = false;
                else // otherwise action fails
                {
                    // failure effect at position of FIRST adjacent log (latest adjacentPos)
                    AbilityFailureVFXManager.PlayFailureVFX(adjacentPos);

                    return;
                }
            }
            else if (adjacentObj.ObjData.ObjType == ObjectType.Compy)
            {
                currIsBush = false;

                // college compy before handling motion
                player.CollectCompy();
            }
            else // obstructed by non-pushable object
            {
                // failure effect at obstruction (latest adjacentPos)
                AbilityFailureVFXManager.PlayFailureVFX(adjacentPos);

                return;
            }
        }

        // if we got this far, the player CAN move through the bushes

        // visual flip of bush sprites
        foreach(QuantumState bush in bushes)
        {
            ObjectSpriteSwapper bushSwapper = bush.GetComponentInChildren<ObjectSpriteSwapper>();
            if (bushSwapper is null) throw new Exception("All level objects must have ObjectSpriteSwapper component on sprite.");
            bushSwapper.RequireFlip();
        }

        // visual flip of playe rsprite
        PlayerSpriteSwapper playerFlipper = player.GetComponentInChildren<PlayerSpriteSwapper>();
        if (playerFlipper is null) throw new Exception("Player MUST have PlayerSpriteSwapper component on child.");
        playerFlipper.RequireFlip();

        // retrieve pos for player to move to
        if (!bushes[bushes.Count - 1].TryGetComponent(out Mover lastBush))
            throw new Exception("All level objects MUST have Mover component.");
        Vector2Int movePos = lastBush.GetGlobalGridPos() + dir;

        // set facing direction
        FaceDirection(player, dir);
        // decrement charges
        player.UseAbilityCharge();

        // SFX
        AudioManager.Instance.PlayPyroAbility();

        // confirm movement (and save undo frame)
        PlayerMoveChecks.ConfirmPlayerMove(player, mover, movePos, null);

    }

    private static void TryCompyAbility(PlayerControls player, Mover mover, Vector2Int dir)
    {
        // Check for object at indicated direction of ability (immediate neighbor)
        Vector2Int adjacentPos = mover.GetGlobalGridPos() + dir;
        QuantumState adjacentObj = VisibilityChecks.GetObjectAtPos(mover, adjacentPos.x, adjacentPos.y);

        // REQUIREMENT: adjacent tile must be visible.
        // REQUIREMENT: object MUST be EITHER nothing OR water (with rock/log).
        if (!VisibilityChecks.IsVisible(player, adjacentPos.x, adjacentPos.y)
            || !(adjacentObj is null || (adjacentObj.ObjData.ObjType == ObjectType.Water && (adjacentObj.ObjData.WaterHasLog || adjacentObj.ObjData.WaterHasRock))))
        {
            // failure effect at adjacent tile - invalid placement position
            AbilityFailureVFXManager.PlayFailureVFX(adjacentPos);

            return;
        }

        // visually flip player sprite
        PlayerSpriteSwapper flipper = player.GetComponentInChildren<PlayerSpriteSwapper>();
        if (flipper is null)
            throw new Exception("Player must have PlayerSpriteSwapper component on one of its children.");
        flipper.RequireFlip();

        // set facing direction
        FaceDirection(player, dir);

        // if we got this far, compy can be placed. this function handles undo frame saving.
        player.SpawnCompy(dir);

        // SFX
        AudioManager.Instance.PlayCompyAbility();

        // this is the end of a player action, so save
        UndoHandler.SaveFrame();
    }
    #endregion

    #region HELPER FUNCTIONS
    /// <summary>
    /// Attempts to make the player face input direction (no change if up/down direction).
    /// Useful for calling just before the stack frame is saved (if ability causes a facing change).
    /// </summary>
    private static void FaceDirection(PlayerControls player, Vector2Int dir)
    {
        // ensure facing direction is updated, if necessary
        if (dir == Vector2Int.right)
            player.SetFacingRight(true);
        else if (dir == Vector2Int.left)
            player.SetFacingRight(false);
    }
    #endregion
}
