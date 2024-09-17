using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static helper class with easily referenced functions that will handle the complex object/component finding aspects of visibility detection.
/// </summary>
public static class VisibilityCheck
{
    /// <summary>
    /// Indicates if the given player object would be visible if located at the (x, y) GLOBAL position on its current panel
    /// </summary>
    public static bool IsVisible(PlayerControls player, int x, int y)
    {
        // Retrieve player's parent panel's PanelOrder
        int panelOrder;
        if (player.transform.parent is not null && player.transform.parent.parent is not null
            && player.transform.parent.parent.TryGetComponent(out SortingOrderHandler playerPanel))
        {
            panelOrder = playerPanel.PanelOrder;
        }
        else
            throw new Exception("Player object MUST be a child of the 'Objects' object within a panel");

        // Start recursion
        return StartRecursiveCheck(panelOrder, x, y); // Player MUST exclude controls bar since player must treat that bar as an obstruction
    }

    /// <summary>
    /// Indicates if the given object would be visible if located at the (x, y) GLOBAL position on its current panel.
    /// This is the generic object visibility function (not specifically player/panel).
    /// </summary>
    public static bool IsVisible(GameObject obj, int x, int y)
    {
        // Retrieve player's parent panel's PanelOrder
        int panelOrder;
        if (obj.transform.parent is not null && obj.transform.parent.parent is not null
            && obj.transform.parent.parent.TryGetComponent(out SortingOrderHandler playerPanel))
        {
            panelOrder = playerPanel.PanelOrder;
        }
        else
            throw new Exception("Object MUST be a child of the 'Objects' object within a panel. Did you possibly try to call IsVisible on a non-object?");

        // Start recursion
        return StartRecursiveCheck(panelOrder, x, y); // Player MUST exclude controls bar since player must treat that bar as an obstruction
    }

    /// <summary>
    /// Indicates if the panel with the given order is visible at the (x, y) GLOBAL position.
    /// Most generic IsVisible function, useful only when you already know the panel order of the panel you are trying to detect visibility for.
    /// </summary>
    public static bool IsVisible(int panelOrder, int x, int y)
    {
        return StartRecursiveCheck(panelOrder, x, y);
    }

    /// <summary>
    /// Handles retrieving MainPanel object and starting Recursive visibility check.
    /// </summary>
    private static bool StartRecursiveCheck(int panelOrder, int x, int y)
    {
        // Retrieve MainPanel to start recursive visibility check
        GameObject[] mainPanelObjects = GameObject.FindGameObjectsWithTag("MainPanel");
        if (mainPanelObjects.Length == 0)
            throw new Exception("No MainPanel found in scene. Either add one or do not use VisibilityCheck");
        if (mainPanelObjects.Length > 1)
            throw new Exception("Only one instance of MainPanel is permitted in a level scene");

        // Start recursive visibility check from main panel
        if (mainPanelObjects[0].TryGetComponent(out SortingOrderHandler mainSortHandler))
            return mainSortHandler.IsGoalVisibleAndContained(panelOrder, x, y);
        else
            throw new Exception("MainPanel object must have SortingOrderHandler component");
    }

    /// <summary>
    /// Returns the QuantumState component of the object at the specified grid position (within the same panel as the object).
    /// In the case of two objects on the same grid position, returns the topmost (i.e. log on water w/ log/rock)
    /// </summary>
    /// <param name="getLower">true = default, get topmost object; false = get lower object</param>
    public static QuantumState GetObjectAtPos(Mover obj, int x, int y, bool getLower = false)
    {
        // parent must be in UpperObjects, which must be in a Panel
        if (obj.transform.parent is not null && obj.transform.parent.parent is not null)
        {
            // find all sibling objects (objects on the same panel as player)
            // 1 = Upper Objects; 2 = Lower Objects
            QuantumState[] upperSiblingObjects = obj.transform.parent.parent.GetChild(1).GetComponentsInChildren<QuantumState>();
            QuantumState[] lowerSiblingObjects = obj.transform.parent.parent.GetChild(2).GetComponentsInChildren<QuantumState>();
            QuantumState[] siblingObjects = new QuantumState[upperSiblingObjects.Length + lowerSiblingObjects.Length];
            // ordering of siblings as upper, then lower gives priority to higher order objects
            if (getLower)
            {
                lowerSiblingObjects.CopyTo(siblingObjects, 0);
                upperSiblingObjects.CopyTo(siblingObjects, lowerSiblingObjects.Length);
            }
            // ordering of siblings as lower, then upper gives priority to lower order objects
            else
            {
                upperSiblingObjects.CopyTo(siblingObjects, 0);
                lowerSiblingObjects.CopyTo(siblingObjects, upperSiblingObjects.Length);
            }

            // iterate through sibling objects checking for position
            foreach (QuantumState sibling in siblingObjects)
            {
                if (sibling.TryGetComponent(out Mover objMover) && sibling.TryGetComponent(out QuantumState objState))
                {
                    Vector2Int pos = objMover.GetGlobalGridPos();
                    if (pos.x == x && pos.y == y && !objState.ObjData.IsDisabled)
                        return sibling;
                }
                else
                    throw new Exception("All Objects MUST have Mover and QuantumState components.");
            }
        }
        else
            throw new Exception("Object MUST be a child of the 'Objects' object within a panel");

        // return null if no object at position found (on same panel as object)
        return null;
    }
}
