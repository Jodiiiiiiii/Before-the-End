using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains data to describe the position and dimensions of a panel.
/// Referenced by other scripts that need position data of other panels.
/// </summary>
public class PanelStats : MonoBehaviour
{
    [Header("Descriptors")]
    [SerializeField, Tooltip("Whether this panel is the immovable main panel")] private bool _isMainPanel = false;

    [Header("Dimensions")]
    [SerializeField, Tooltip("# of tiles of width of the panel")] public int Width;
    [SerializeField, Tooltip("# of tiles of height of the panel (not including 1 unit dragging bar")] public int Height;

    public int OriginX { get; private set; }
    public int OriginY { get; private set; }

    // Update is called once per frame
    void Update()
    {
        // update position variable in case it just changed
        OriginX = (int)transform.position.x;
        OriginY = (int)(transform.position.y - Height); // shift stored origin to bottom left
        
        // shift origin down one if it has a dragging bar
        if (!_isMainPanel) OriginY--;
    }

    /// <summary>
    /// Returns whether position (x,y) is contained within the current panel (including the controls bar).
    /// Useful in visibility detection algorithm.
    /// </summary>
    public bool IsPosInBounds(int x, int y)
    {
        // left/right of panel bounds
        if (x < OriginX || x >= OriginX + Width)
            return false;
        // above/below panel bounds
        if (y < OriginY || (_isMainPanel ? (y >= OriginY + Height) : (y >= OriginY + Height + 1)))
            return false;
        
        // therefore, must be within bounds
        return true;
    }
}
