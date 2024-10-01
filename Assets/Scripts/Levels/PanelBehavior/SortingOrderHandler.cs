using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

/// <summary>
/// Includes function for recursive depth-first traversal of panel hierarchy, which determines ordering of panels.
/// Sorting orders of tilemaps and panel objects are updated accordingly based on determined panel order.
/// Also handles recursive checks for visibility, based on grid position and panel order.
/// </summary>
public class SortingOrderHandler : MonoBehaviour
{
    public static int MaxPanelOrder = 0; // default 0 if only main panel

    [HideInInspector] public int PanelOrder = 0;

    private TilemapRenderer _groundTilemap = null;
    private TilemapRenderer _borderTilemap = null;
    private SortingGroup _upperObjectsSortingGroup;
    private SortingGroup _lowerObjectsSortingGroup;

    private List<SortingOrderHandler> _subPanels = new List<SortingOrderHandler>();

    private bool _instantiated = false;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize tilemap renderers and object sorting group
        foreach (Transform child in transform)
        {
            // initialize tilemap renderers
            if (child.TryGetComponent(out Grid grid))
            {
                TilemapRenderer[] tilemaps = child.transform.GetComponentsInChildren<TilemapRenderer>();

                //incorrect number of child tilemaps
                if (tilemaps.Length != 2)
                    throw new Exception("" + gameObject.name + " object has " + tilemaps.Length + " child tilemaps. Expected 2 (ground and border).");

                for (int i = 0; i < 2; i++)
                {
                    if (tilemaps[i].CompareTag("GroundTilemap"))
                        _groundTilemap = tilemaps[i];
                    else if (tilemaps[i].CompareTag("BorderTilemap"))
                        _borderTilemap = tilemaps[i];
                    else // Missing identifier tags
                        throw new Exception("tilemap " + tilemaps[i].name + ", child of " + gameObject.name + " object does not have GroundTilemap or BorderTilemap tag");
                }
            }

            // initialize object sorting groups
            if (child.CompareTag("PanelObjects"))
            {
                if (child.TryGetComponent(out SortingGroup sortGroup))
                {
                    if (_upperObjectsSortingGroup is null)
                        _upperObjectsSortingGroup = sortGroup;
                    else
                        _lowerObjectsSortingGroup = sortGroup;
                }
                else
                    throw new Exception("Object tagged as PanelObjects must contain SortingGroup component");
            }

            // initialize subpanel SortingOrderHandlers
            if (child.TryGetComponent(out SortingOrderHandler sortHandler))
                _subPanels.Add(sortHandler);
        }

        // Missing grid object?
        if (_groundTilemap is null || _borderTilemap is null)
            throw new Exception("GroundTilemap and/or BorderTilemap improperly initialized. Are you missing a Grid altogether?");

        // Missing PanelObjects container
        if (_upperObjectsSortingGroup is null)
            throw new Exception("Panel must contain PanelObjects container object. Are you maybe missing the PanelObjects tag");
    }
    // Update is called once per frame
    void Update()
    {
        // Only called during first frame of Update
        // Required to ensure that panel tree hierarchy is only traversed once all panels have gone through the Start method
        if(!_instantiated)
        {
            if (transform.parent is null) // main panel (root) only
            {
                // ensure MaxPanelOrder is re-calculated in each level
                MaxPanelOrder = 0;
                // starts recursive setPanelOrder call through subpanel hierarchy
                SetPanelOrder(0);
            }
            _instantiated = true;
        }
       
        // update actual sorting orders based on calculated PanelOrders
        // also ensures proper ground/border->lowerObjects->upperObjects layering within each panel
        _groundTilemap.sortingOrder = 3 * PanelOrder;
        _borderTilemap.sortingOrder = 3 * PanelOrder;
        _lowerObjectsSortingGroup.sortingOrder = 3 * PanelOrder + 1;
        _upperObjectsSortingGroup.sortingOrder = 3 * PanelOrder + 2;
    }

    /// <summary>
    /// Visits the current panel, setting the appropriate PanelOrder.
    /// Returns the next appropriate panel order index based on depth-first tree traversal of panel hierarchy.
    /// Called whenever panel ordering changes (i.e. start, or sibling re-ordering)
    /// </summary>
    private int SetPanelOrder(int panelOrder)
    {
        PanelOrder = panelOrder;

        // update static max panel order if new highest leaf node is found
        if (PanelOrder > MaxPanelOrder)
            MaxPanelOrder = PanelOrder;

        if (_subPanels.Count == 0) // leaf node (no sub panels)
            return PanelOrder + 1;
        if (_subPanels.Count == 1) // one sub-panel
            return _subPanels[0].SetPanelOrder(PanelOrder + 1);
        else // multiple sub-panels
        {
            // Retrieve all sibling handlers
            List<SiblingOrderHandler> siblingHandlers = new List<SiblingOrderHandler>();
            foreach (SortingOrderHandler subPanel in _subPanels)
            {
                if (subPanel.TryGetComponent(out SiblingOrderHandler siblingHandler))
                    siblingHandlers.Add(siblingHandler);
                else
                    throw new Exception("Any panel with a sibling MUST have a sibling handler.");
            }

            // Find ordered SiblingOrder values
            int nextPanelOrder = PanelOrder + 1;
            for (int i = 1; i < siblingHandlers.Count+1; i++) // iterates through goal siblingOrders
            {
                for (int j = 0; j < siblingHandlers.Count; j++) // iterates through subpanels (siblingHandler components)
                {
                    if (siblingHandlers[j].SiblingOrder == i)
                    {
                        // recursive call to subpanels from this sibling node
                        nextPanelOrder = _subPanels[j].SetPanelOrder(nextPanelOrder);
                        break;
                    }
                }
            }
            return nextPanelOrder;
        }
    }

    /// <summary>
    /// Starts a depth-first traversal to update panel orders.
    /// Can be called from any node in the panel tree.
    /// </summary>
    public void UpdatePanelOrders()
    {
        // Retrieve MainPanel to start recursive panel order update
        GameObject[] mainPanel = GameObject.FindGameObjectsWithTag("MainPanel");
        if (mainPanel.Length == 0)
            throw new Exception("No MainPanel found in scene. Either add one or do not use VisibilityCheck");
        if (mainPanel.Length > 1)
            throw new Exception("Only one instance of MainPanel is permitted in a level scene");

        // Start recursive panel order update
        if (mainPanel[0].TryGetComponent(out SortingOrderHandler sortHandler))
            sortHandler.SetPanelOrder(0);
        else
            throw new Exception("Main panels MUST have a SortingOrderHandler");
    }

    /// <summary>
    /// Given a grid position, and an index corresponding to a particular panel's PanelOrder, 
    /// recursively determines whether that panel is visible (not obstructed by another panel) at that grid position.
    /// Also ensures that goal is actually contained in bounds of goal panel.
    /// </summary>
    /// <param name="excludeControlsBar">
    /// If true, makes CONTAINMENT checks ignore controls bar. 
    /// Obstruction checks will still ALWAYS include controls bar
    /// </param>
    public bool IsGoalVisibleAndContained(int goalPanelOrder, int x, int y)
    {
        // Current panel IS goal panel
        if(goalPanelOrder == PanelOrder)
        {
            // Ensure that the goal panel actually contains the grid position (x, y)
            if (TryGetComponent(out PanelStats panelStats))
            {
                if (!panelStats.IsPosInBounds(x, y))
                    return false;
            }
        }

        // all remaining cases involve a panel with an order layered in FRONT of the goal panel (must check positions)
        if (_subPanels.Count == 0) // leaf node (no sub panels) - only place where true can be returned
        {
            // panel behind goal panel (or the goal is this leaf node panel)
            if (goalPanelOrder >= PanelOrder)
                return true;
            else // panel layered in front of goal panel
            {
                if (TryGetComponent(out PanelStats panelStats))
                    // goal not visible if this subpanel includes the pos
                    return !panelStats.IsPosInBounds(x, y);
                else
                    throw new Exception("All panels MUST have a PanelStats component");
            }
        }
        if (_subPanels.Count == 1) // one sub-panel
        {
            // goal panel is layered behind current panel
            if(goalPanelOrder < PanelOrder)
            {
                if (TryGetComponent(out PanelStats panelStats))
                {
                    if (panelStats.IsPosInBounds(x, y))
                        return false; // goal is obstructed so return
                } 
                else
                    throw new Exception("All panels MUST have a PanelStats component");
            }

            // Visit sub-panel
            return _subPanels[0].IsGoalVisibleAndContained(goalPanelOrder, x, y);
        }
        else // multiple sub-panels
        {
            // goal panel is layered behind current panel
            if (goalPanelOrder < PanelOrder)
            {
                if (TryGetComponent(out PanelStats panelStats))
                {
                    if (panelStats.IsPosInBounds(x, y))
                        return false; // goal is obstructed so return
                }
                else
                    throw new Exception("All panels MUST have a PanelStats component");
            }

            // visit all sub-panels
            for (int i = 0; i < _subPanels.Count; i++)
            {
                if (!_subPanels[i].IsGoalVisibleAndContained(goalPanelOrder, x, y))
                    return false;
            }

            // If this was reached, then every subpanel already returned true from a leaf node
            return true;
        }
    }

    /// <summary>
    /// Starts a depth-first traversal to find a panel of a specific panel order.
    /// Returns null if goal panel is NOT found.
    /// </summary>
    public static PanelStats GetPanelOfOrder(int goalPanelOrder)
    {
        // Retrieve MainPanel to start recursive panel order update
        GameObject[] mainPanel = GameObject.FindGameObjectsWithTag("MainPanel");
        if (mainPanel.Length == 0)
            throw new Exception("No MainPanel found in scene. Either add one or do not use VisibilityCheck");
        if (mainPanel.Length > 1)
            throw new Exception("Only one instance of MainPanel is permitted in a level scene");

        // Start recursive panel order check
        SortingOrderHandler foundHandler;
        if (mainPanel[0].TryGetComponent(out SortingOrderHandler sortHandler))
            foundHandler = sortHandler.CheckForPanelOrder(goalPanelOrder);
        else
            throw new Exception("Main panels MUST have a SortingOrderHandler");

        // indicate if it wasn't found
        if (foundHandler is null)
            return null;

        // Now get and return the PanelStats component
        if (foundHandler.TryGetComponent(out PanelStats panel))
            return panel;
        else
            throw new Exception("All panels MUST contain a PanelStats component");
    }

    /// <summary>
    /// Visits the current panel, checking if it has the goalPanelOrder, otherwise check in children recursively.
    /// </summary>
    private SortingOrderHandler CheckForPanelOrder(int goalPanelOrder)
    {
        // FOUND
        if (PanelOrder == goalPanelOrder)
            return this;

        if (_subPanels.Count == 0) // leaf node (no sub panels)
        {
            // not found
            return null;
        }
        if (_subPanels.Count == 1) // one sub-panel
        {
            // check sub-panel
            return _subPanels[0].CheckForPanelOrder(goalPanelOrder);
        }
        else // multiple sub-panels
        {
            // Recursive call to subpanels until panel is found
            for (int i = 0; i < _subPanels.Count; i++) // iterates through goal siblingOrders
            {
                // Check each child panel
                SortingOrderHandler childResult = _subPanels[i].CheckForPanelOrder(goalPanelOrder);
                if (childResult is not null)
                    return childResult;
            }
            // panel NOT found
            return null;
        }
    }
}
