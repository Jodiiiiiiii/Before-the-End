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
    /// Indicates if input panel is visible at input (x, y) position
    /// </summary>
    public static bool IsVisible(PanelControls panel, int x, int y)
    {
        // Retrieve MainPanel to start recursive visibility check
        GameObject[] mainPanelObjects = GameObject.FindGameObjectsWithTag("MainPanel");
        if (mainPanelObjects.Length == 0)
            throw new Exception("No MainPanel found in scene. Either add one or do not use VisibilityCheck");
        if (mainPanelObjects.Length > 1)
            throw new Exception("Only one instance of MainPanel is permitted in a level scene");
        GameObject mainPanel = mainPanelObjects[0];

        // Retrieve panel's PanelOrder
        int panelOrder;
        if (panel.TryGetComponent(out SortingOrderHandler panelSortHandler))
            panelOrder = panelSortHandler.PanelOrder;
        else
            throw new Exception("All panels MUST have a SortingOrderHandler component");

        // Start recursive visibility check from main panel
        if (mainPanel.TryGetComponent(out SortingOrderHandler mainSortHandler))
            return mainSortHandler.IsGoalVisible(panelOrder, x, y);
        else
            throw new Exception("MainPanel object must have SortingOrderHandler component");
    }

    //  Another function can be established here that takes a GameObject as input instead,
    //  automatically detecting its position and panel from its parent(s)
}
