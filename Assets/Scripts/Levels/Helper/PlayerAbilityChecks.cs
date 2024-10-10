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
                return; // unimplemented
            case DinoType.Pyro:
                return; // unimplemented
            case DinoType.Compy:
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

        // object present and visible (and not clock)
        if (adjacentObj is not null && adjacentObj.ObjData.ObjType != ObjectType.Clock
            && VisibilityChecks.IsVisible(player, abilityPos.x, abilityPos.y))
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

        // REQUIREMENT: object must be present AND visible.
        // REQUIREMENT: object MUST be EITHER a log OR a rock.
        if (adjacentObj is null || !VisibilityChecks.IsVisible(player, adjacentPos.x, adjacentPos.y)
            || (adjacentObj.ObjData.ObjType != ObjectType.Log && adjacentObj.ObjData.ObjType != ObjectType.Rock))
        {
            // TODO: failure effect at adjacent tile

            return;
        }

        // log = guaranteed success
        if (adjacentObj.ObjData.ObjType == ObjectType.Log)
        {
            // push/crush logs
            PlayerMoveChecks.PushLogsInSeries(mover, adjacentPos, dir, true);

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
                    PlayerMoveChecks.PushLogsInSeries(mover, adjacentPos, dir, true);
                }
                else // obstructed by bush/tallBush/Tunnel/etc.
                {
                    // TODO: failure effect on object which cannot be pushed at end of chain

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
            // TODO: failure effect at adjacent tile

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
            // TODO: failure effect at adjacent tile 
            return;
        }

        // always flip player to indicate dino swinging its tail
        player.SetFacingRight(!player.IsFacingRight());

        // decrement charges
        player.UseAbilityCharge();
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
                // swim!
                player.IsSwimming = true;

                // ensure facing direction is updated, if necessary
                if (dir == Vector2Int.right)
                    player.SetFacingRight(true);
                else if (dir == Vector2Int.left)
                    player.SetFacingRight(false);

                // move player into water
                PlayerMoveChecks.ConfirmPlayerMove(mover, dir);
            }
            else
            {
                // TODO: failure effect at adjacent tile

                return;
            }
        }
        // Check for EXITING water
        else
        {
            // OPTION 1: moving into open space out of water
            // OPTION 2: moving out of water into pushable logs
            // OPTION 3: moving out of water onto submerged log OR submerged rock
            if ((adjacentObj is null && VisibilityChecks.IsVisible(player, adjacentPos.x, adjacentPos.y))
                || PlayerMoveChecks.PushLogsInSeries(mover, adjacentPos, dir)
                || (adjacentObj is not null && adjacentObj.ObjData.ObjType == ObjectType.Water && (adjacentObj.ObjData.WaterHasLog || adjacentObj.ObjData.WaterHasRock)))
            {
                // un-swim!
                player.IsSwimming = false;

                // ensure facing direction is updated, if necessary
                if (dir == Vector2Int.right)
                    player.SetFacingRight(true);
                else if (dir == Vector2Int.left)
                    player.SetFacingRight(false);

                // move player out of water
                PlayerMoveChecks.ConfirmPlayerMove(mover, dir);
            }
            else
            {
                // TODO: failure effect at adjacent tile

                return;
            }
        }

        
    }
}
