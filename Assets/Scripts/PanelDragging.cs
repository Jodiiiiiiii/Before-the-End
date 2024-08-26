using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PanelDragging : MonoBehaviour
{
    // constants
    private const int MOUSE_LEFT = 0; // input constant

    private PanelStats _parentPanel;
    private PanelStats _currPanel;

    private bool _isDragging = false;
    private Vector2 _dragOffset = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        _parentPanel = transform.parent.gameObject.GetComponent<PanelStats>();
        _currPanel = GetComponent<PanelStats>();
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

            // determine if click was in bounds of nav bar
            float xDiff = transform.position.x - mousePos.x;
            float yDiff = transform.position.y - mousePos.y;
            if (xDiff <= 0f && xDiff >= -_currPanel.Width && yDiff >= 0f && yDiff <= 1f)
            {
                _dragOffset = new Vector2(xDiff, yDiff);
                _isDragging = true;
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
            gridPos.y = Mathf.Clamp(gridPos.y, _parentPanel.OriginY + _currPanel.Height + 1, _parentPanel.OriginY + _parentPanel.Height); 

            // Round to nearest int (nearest grid index)
            gridPos.x = Mathf.Round(gridPos.x);
            gridPos.y = Mathf.Round(gridPos.y);

            // update current panel position
            transform.position = gridPos;
        }
        else
        {
            _isDragging = false;
        }
    }
}
