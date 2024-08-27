using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;

/// <summary>
/// Determines ordering of sibling panels relative to each other.
/// SiblingOrder is accessible for use in calculations in SortingOrderHandler.
/// </summary>
public class SiblingOrderHandler : MonoBehaviour
{
    [SerializeField, Tooltip("number order relative to sibling panels. 1 = lowest panel")] public int SiblingOrder = 0;

    private List<SiblingOrderHandler> siblings;

    // Start is called before the first frame update
    void Start()
    {
        // Order MUST be specified
        if (SiblingOrder == 0)
            throw new Exception("order must be specified in SiblingOrderHandler of object: " + gameObject.name);

        // Initialize sibling SiblingOrderHandlers
        siblings = new List<SiblingOrderHandler>();
        foreach(Transform child in transform.parent)
        {
            if (child.TryGetComponent(out SiblingOrderHandler sibling))
                siblings.Add(sibling);
        }
        // MUST have at least 2 siblings
        if (siblings.Count < 2)
            throw new Exception("Only " + siblings.Count + " siblings found with SiblingOrderHandler. Expected 2 or more siblings.");
        // siblings MUST use consecutive integers starting at 1
        bool intFound;
        for(int i = 1; i<siblings.Count+1; i++) // iterate through required order integers
        {
            // reset int found for each required int
            intFound = false;
            for (int j = 0; j < siblings.Count; j++) // iterate through siblings
            {
                // required order integer found
                if (siblings[j].SiblingOrder == i)
                {
                    if (!intFound) intFound = true;
                    else throw new Exception("Sibling sorting orders CANNOT match. Parent panel of issue is " + transform.parent.name);
                }

                // required order integer NOT found
                if (j == siblings.Count - 1 && !intFound)
                    throw new Exception("Sibling sorting orders MUST use consecutive integers starting at 1. Parent panel of issue is " + transform.parent.name);
            }
        }
    }

    /// <summary>
    /// Increase order of current panel; decreasing order of siblings as necessary
    /// Called within PanelDragging.cs which handles clicks
    /// </summary>
    public void Raise()
    {
        // Ensure not already at topmost position
        if(SiblingOrder!=siblings.Count)
        {
            // Find sibling above current panel in ordering - and swap their orders
            for(int i=0; i < siblings.Count; i++)
            {
                if (siblings[i].SiblingOrder==SiblingOrder+1)
                {
                    siblings[i].SiblingOrder--;
                    SiblingOrder++;
                    break;
                }
            }
        }

        // Update panel orders of all panels now that a change has taken place
        if (TryGetComponent(out SortingOrderHandler sortHandler))
            sortHandler.UpdatePanelOrders();
        else
            throw new Exception("ALL panels MUST have a SortingOrderHandler");
    }

    /// <summary>
    /// Decrease order of current panel; increasing order of siblings as necessary
    /// Called within PanelDragging.cs which handles clicks
    /// </summary>
    public void Lower()
    {
        // Ensure not already at bottommost position
        if(SiblingOrder!=1)
        {
            // Find sibling below current panel in ordering - and swap their orders
            for(int i = 0; i < siblings.Count; i++)
            {
                if (siblings[i].SiblingOrder == SiblingOrder - 1)
                {
                    siblings[i].SiblingOrder++;
                    SiblingOrder--;
                    break;
                }
            }
        }

        // Update panel orders of all panels now that a change has taken place
        if (TryGetComponent(out SortingOrderHandler sortHandler))
            sortHandler.UpdatePanelOrders();
        else
            throw new Exception("ALL panels MUST have a SortingOrderHandler");
    }
}
