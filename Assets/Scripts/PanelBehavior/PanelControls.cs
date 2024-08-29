using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles inputs related to clicking and dragging panels to move them.
/// Accounts for constraints including clamping wihtin parent panel and rounding to grid units.
/// Also handles inputs when relevant for sibling re-ordering buttons.
/// </summary>
public class PanelControls : MonoBehaviour
{
    // constants
    private const int MOUSE_LEFT = 0; // input constant

    // Panel data
    private PanelStats _parentPanel;
    private PanelStats _currPanel;
    // dragging state
    private bool _isDragging = false;
    private Vector2 _dragOffset = Vector2.zero;
    // sibling ordering
    private SiblingOrderHandler _orderHandler = null; // stays null unless present

    // Start is called before the first frame update
    void Start()
    {
        _parentPanel = transform.parent.gameObject.GetComponent<PanelStats>();
        _currPanel = GetComponent<PanelStats>();

        if (TryGetComponent(out SiblingOrderHandler orderHandler))
            _orderHandler = orderHandler;
    }

    // Update is called once per frame
    void Update()
    {
        // check for initial click
        if(Input.GetMouseButtonDown(MOUSE_LEFT))
        {
            // Mouse position calculations
            Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            mousePos = Camera.main.ScreenToWorldPoint(mousePos); // convert to world space pos
            
            // vars for determining mouse in bounds
            float xDiff = transform.position.x - mousePos.x;
            float yDiff = transform.position.y - mousePos.y;

            if(_orderHandler is null) // no siblings - no ordering button
            {
                // dragging
                if (xDiff <= 0f && xDiff >= -_currPanel.Width && yDiff >= 0f && yDiff <= 1f 
                    && VisibilityCheck.IsVisible(this, Mathf.FloorToInt(mousePos.x), Mathf.FloorToInt(mousePos.y)))
                {
                    _dragOffset = new Vector2(xDiff, yDiff);
                    _isDragging = true;
                }
            }
            else // has siblings - and ordering button
            {
                // start dragging
                if (xDiff <= 0f && xDiff >= -_currPanel.Width + 1 && yDiff >= 0f && yDiff <= 1f
                    && VisibilityCheck.IsVisible(this, Mathf.FloorToInt(mousePos.x), Mathf.FloorToInt(mousePos.y)))
                {
                    _dragOffset = new Vector2(xDiff, yDiff);
                    _isDragging = true;
                }
                // order up relative to siblings
                else if (xDiff <= -_currPanel.Width + 1 && xDiff >= -_currPanel.Width && yDiff >= 0.5f && yDiff <= 1f
                    && VisibilityCheck.IsVisible(this, Mathf.FloorToInt(mousePos.x), Mathf.FloorToInt(mousePos.y)))
                    _orderHandler.Lower();
                // order down relative to siblings
                else if (xDiff <= -_currPanel.Width + 1 && xDiff >= -_currPanel.Width && yDiff >= 0f && yDiff <= 0.5f
                    && VisibilityCheck.IsVisible(this, Mathf.FloorToInt(mousePos.x), Mathf.FloorToInt(mousePos.y)))
                    _orderHandler.Raise();
            }
        }

        // updates while dragging
        if (_isDragging && Input.GetMouseButton(MOUSE_LEFT))
        {
            // Mouse position calculations
            Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            mousePos = Camera.main.ScreenToWorldPoint(mousePos); // convert to world space pos

            // calculate actual position with dragging offset
            Vector2 gridPos = mousePos + _dragOffset;

            // Clamp to bounds of parent panel
            gridPos.x = Mathf.Clamp(gridPos.x, _parentPanel.OriginX, _parentPanel.OriginX + _parentPanel.Width - _currPanel.Width);
            // +1 to account for drag bar
            gridPos.y = Mathf.Clamp(gridPos.y, _parentPanel.OriginY + _currPanel.Height, _parentPanel.OriginY + _parentPanel.Height - 1); 

            // Round to nearest int (nearest grid index)
            gridPos.x = Mathf.Round(gridPos.x);
            gridPos.y = Mathf.Round(gridPos.y);

            // update current panel position
            transform.position = gridPos;
        }
        else
        {
            // lock in place until nav bar is clicked again
            _isDragging = false;

            // call to update stack frames can be called here
        }
    }
}
