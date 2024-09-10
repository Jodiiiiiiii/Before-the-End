using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Handles inputs related to clicking and dragging panels to move them.
/// Accounts for constraints including clamping wihtin parent panel and rounding to grid units.
/// Also handles inputs when relevant for sibling re-ordering buttons.
/// </summary>
public class PanelControls : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Tooltip("Used for calling position updates")] private ObjectMover _objMover;

    [Header("Alpha Reduction")]
    [SerializeField, Tooltip("Alpha value (0 to 1) for object and all child objects when panel dragging is occurring")]
    private float _draggingAlpha = 0.8f;
    [SerializeField, Tooltip("ground tilemap so that alpha can be reduced")]
    private Tilemap _groundTilemap;
    [SerializeField, Tooltip("border tilemap so that alpha can be reduced")]
    private Tilemap _borderTilemap;

    // constants
    private const int MOUSE_LEFT = 0; // input constant

    // Panel data
    private PanelStats _parentPanel;
    private PanelStats _currPanel;
    // dragging state
    private bool _isDragging = false;
    private Vector2 _dragOffset = Vector2.zero;
    private bool _hasPosChanged = false;
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
                    && VisibilityCheck.IsVisible(this, Mathf.FloorToInt(mousePos.x), Mathf.CeilToInt(mousePos.y))) // upper-left corner
                {
                    _dragOffset = new Vector2(xDiff, yDiff);
                    _isDragging = true;
                    _hasPosChanged = false;
                    PlayerControls.IsPlayerLocked = true; // player cannot move while panel dragging
                }
            }
            else // has siblings - and ordering button
            {
                // start dragging
                if (xDiff <= 0f && xDiff >= -_currPanel.Width + 1 && yDiff >= 0f && yDiff <= 1f
                    && VisibilityCheck.IsVisible(this, Mathf.FloorToInt(mousePos.x), Mathf.CeilToInt(mousePos.y))) // upper-left corner
                {
                    _dragOffset = new Vector2(xDiff, yDiff);
                    _isDragging = true;
                    _hasPosChanged = false;
                    PlayerControls.IsPlayerLocked = true; // player cannot move while panel dragging
                }
                // order up relative to siblings
                else if (xDiff <= -_currPanel.Width + 1 && xDiff >= -_currPanel.Width && yDiff >= 0.5f && yDiff <= 1f
                    && VisibilityCheck.IsVisible(this, Mathf.FloorToInt(mousePos.x), Mathf.CeilToInt(mousePos.y))) // upper-left corner
                {
                    if(_orderHandler.Lower()) // re-ordering algorithm
                        UndoHandler.SaveFrame(); // update UndoStack (if a change happened)
                }
                // order down relative to siblings
                else if (xDiff <= -_currPanel.Width + 1 && xDiff >= -_currPanel.Width && yDiff >= 0f && yDiff <= 0.5f
                    && VisibilityCheck.IsVisible(this, Mathf.FloorToInt(mousePos.x), Mathf.CeilToInt(mousePos.y))) // upper-left corner
                {
                    if(_orderHandler.Raise()) // re-ordering algorithm
                        UndoHandler.SaveFrame(); // update UndoStack (if a change happened)
                }
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

            // Initiate panel movement IF a change has occurred (+ reduced alpha and locking)
            Vector2Int gridPosInt = new Vector2Int(Mathf.RoundToInt(gridPos.x), Mathf.RoundToInt(gridPos.y));
            if (!gridPosInt.Equals(_objMover.GetGlobalGridPos()))
            {
                // reduce alpha while dragging
                SetAlphaOfChildren(_draggingAlpha);

                // prevent changes from affecting visibility checks until released
                _objMover.LockResults();

                _hasPosChanged = true;
            }

            // now that we know a change has occured, update position every frame
            // we cannot get position from object mover anymore since it is locked
            if(_hasPosChanged)
            {
                // Round to nearest int AND update current panel position (through ObjectMover)
                _objMover.SetGlobalGoal(Mathf.RoundToInt(gridPos.x), Mathf.RoundToInt(gridPos.y));
            }
        }
        else if (_isDragging) // END DRAGGING: Mouse Button no longer pressed
        {
            // lock in place until nav bar is clicked again
            _isDragging = false;

            // restore alpha to full
            SetAlphaOfChildren(1);

            // unlock object mover so it not behaves as normal again
            _objMover.UnlockResults();

            // frame dragging has completed, call the update to the Undo Stack (if pos changed)
            if (_hasPosChanged)
                UndoHandler.SaveFrame();

            PlayerControls.IsPlayerLocked = false; // unlock player controls now that panel dragging is complete
        }
    }

    /// <summary>
    /// Update alpha values of ground and border tilemaps
    /// </summary>
    private void SetAlphaOfChildren(float alpha)
    {
        Color newCol = _groundTilemap.color;
        newCol.a = alpha;
        _groundTilemap.color = newCol;
        newCol = _borderTilemap.color;
        newCol.a = alpha;
        _borderTilemap.color = newCol;
    }
}
