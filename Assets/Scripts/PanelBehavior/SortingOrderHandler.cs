using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

/// <summary>
/// Includes function for recursive depth-first traversal of panel hierarchy, which determines ordering of panels.
/// Sorting orders of tilemaps and panel objects are updated accordingly based on determined panel order.
/// </summary>
public class SortingOrderHandler : MonoBehaviour
{
    [HideInInspector] public int PanelOrder = 0;

    private TilemapRenderer _groundTilemap = null;
    private TilemapRenderer _borderTilemap = null;
    private SortingGroup _objectsSortingGroup;

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

            // initialize object sorting group
            if (child.CompareTag("PanelObjects"))
            {
                if (child.TryGetComponent(out SortingGroup sortGroup))
                    _objectsSortingGroup = sortGroup;
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
        if (_objectsSortingGroup is null)
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
                SetPanelOrder(0); // starts recursive setPanelOrder call through subpanel hierarchy
            _instantiated = true;
        }
       
        // update actual sorting orders based on calculated PanelOrders
        // also ensures proper ground->border->objects layering within each panel
        _groundTilemap.sortingOrder = 3 * PanelOrder;
        _borderTilemap.sortingOrder = 3 * PanelOrder + 1;
        _objectsSortingGroup.sortingOrder = 3 * PanelOrder + 2;
    }

    /// <summary>
    /// Visits the current panel, setting the appropriate PanelOrder.
    /// Returns the next appropriate panel order index based on depth-first tree traversal of panel hierarchy.
    /// Called whenever panel ordering changes (i.e. sibling re-ordering button press)
    /// </summary>
    private int SetPanelOrder(int panelOrder)
    {
        PanelOrder = panelOrder;

        if(_subPanels.Count == 0) // leaf node (no sub panels)
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
    /// Starts a recursive call upwards to the root in order to start a depth-first traversal to update panel orders.
    /// Can be called from any node in the panel tree.
    /// </summary>
    public void UpdatePanelOrders()
    {
        if (transform.parent is null)
            SetPanelOrder(0);
        else
        {
            if (transform.parent.TryGetComponent(out SortingOrderHandler sortHandler))
                sortHandler.UpdatePanelOrders();
            else
                throw new Exception("ALL panels MUST have a SortingOrderHandler");
        }
    }
}
