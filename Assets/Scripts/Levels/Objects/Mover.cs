using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles updating of local positions through functions for getting/setting/incrementing local/global grid (integer) positions.
/// Supports functionality for objects that can NEVER move (used then just for storing exact grid positions).
/// </summary>
public class Mover : MonoBehaviour
{
    [Header("Movement Behavior")]
    [SerializeField, Tooltip("Distance from goal position when object will snap to exact goal position")] private float _snappingThreshold = 0.01f;
    [SerializeField, Tooltip("'Snappiness' of object seeking goal position")] private float _movingSharpness = 30f;

    //  local/world-space grid positions of object (always exact integers)
    [Header("Positions (DO NOT CHANGE)")]
    [SerializeField, Tooltip("DO NOT CHANGE. Local position of current object. Useful to see in Inspector")] private Vector2Int _localGridPos;
    [SerializeField, Tooltip("DO NOT CHANGE. Global position of current object. Useful to see in Inspector")] private Vector2Int _globalGridPos;

    // useful when the position needs to be visually updated but algorithms using the actual level position are unaffected.
    private bool _isLocked = false;
    private Vector2Int _lockedLocalPos;
    private Vector2Int _lockedGlobalPos;

    // Start is called before the first frame update
    void Start()
    {
        // ensure it starts at even integers
        _localGridPos = new Vector2Int(Mathf.FloorToInt(transform.localPosition.x), Mathf.FloorToInt(transform.localPosition.y));
        _globalGridPos = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
    }

    // Update is called once per frame
    void Update()
    {
        // GLOBAL CHANGES (PANEL DRAGGING)

        // TODO: May have unusual edge cases when still lerping (not stationary) and panels are moved
        // Determine if there is a discongruence between global/actual global positions (panel dragging)
        Vector2Int actualGlobalPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        if(IsStationary() && _globalGridPos != actualGlobalPos)
        {
            // determine changes
            int xDiff = actualGlobalPos.x - _globalGridPos.x;
            int yDiff = actualGlobalPos.y - _globalGridPos.y;
            // apply changes to stored global positions
            _globalGridPos.x += xDiff;
            _globalGridPos.y += yDiff;
        }

        // LOCAL POSITION LERPING (MOVEMENTS)

        // Determine current local position
        Vector2 currLocalPos = new Vector2(transform.localPosition.x, transform.localPosition.y);

        // Snap to goal if close enough
        if (Vector2.Distance(currLocalPos, _localGridPos) < _snappingThreshold)
        {
            Vector3 snapPos = transform.localPosition;
            snapPos.x = _localGridPos.x;
            snapPos.y = _localGridPos.y;
            transform.localPosition = snapPos;
        }
        else // smoothly lerp towards goal
        {
            Vector2 lerpVec2 = Vector2.Lerp(currLocalPos, _localGridPos, 1f - Mathf.Exp(-_movingSharpness * Time.deltaTime));
            Vector3 lerpPos = transform.localPosition;
            lerpPos.x = lerpVec2.x;
            lerpPos.y = lerpVec2.y;
            transform.localPosition = lerpPos;
        }
    }

    /// <summary>
    /// returns global grid position
    /// </summary>
    public Vector2Int GetGlobalGridPos()
    {
        // return locked pos if locked
        if (_isLocked)
            return _lockedGlobalPos;

        return _globalGridPos;
    }

    /// <summary>
    /// returns local grid position
    /// </summary>
    public Vector2Int GetLocalGridPos()
    {
        // return locked pos if locked
        if (_isLocked)
            return _lockedLocalPos;

        return _localGridPos;
    }

    /// <summary>
    /// Input global goal coordinates and the script will handle converting it to local coordinates.
    /// </summary>
    public void SetGlobalGoal(int x, int y)
    {
        // set global pos
        _globalGridPos = new Vector2Int(x, y);

        // set local pos
        Vector3 globalPos = new Vector3(x, y, transform.position.z);
        Vector3 localPos = transform.parent.InverseTransformPoint(globalPos);

        // must round because it should already be near an int from the inputs, but InverseTransformPoint can make floating-point variability
        _localGridPos = new Vector2Int(Mathf.RoundToInt(localPos.x), Mathf.RoundToInt(localPos.y));
    }

    public void SetLocalGoal(int x, int y)
    {
        // set local pos
        _localGridPos = new Vector2Int(x, y);

        // set global pos
        Vector3 localPos = new Vector3(x, y, transform.localPosition.z);
        Vector3 globalPos = transform.TransformPoint(localPos);

        // must round to ensure no floating point results from TransformPoint
        _globalGridPos = new Vector2Int(Mathf.RoundToInt(globalPos.x), Mathf.RoundToInt(globalPos.y));
    }

    /// <summary>
    /// Increments local/global grid position by moveDir, which must be a vector with only one non-zero value of either -1 or 1
    /// </summary>
    public void Increment(Vector2Int moveDir)
    {
        // Ensure target position is validly only one unit away
        if (moveDir.magnitude != 1 || (moveDir.x != 1 && moveDir.x != -1 && moveDir.x != 0) || (moveDir.y != 1 && moveDir.y != -1 && moveDir.y != 0))
            throw new Exception("Input of CanMove function MUST have only one non-zero value and it must be eiether -1 or 1.");

        // Increment local/global positions
        _localGridPos += moveDir;
        _globalGridPos += moveDir;
    }

    /// <summary>
    /// Indicates if object is exactly at goal position.
    /// Useful to restrict movement actions until the previous action has complete
    /// </summary>
    public bool IsStationary()
    {
        Vector2 currLocalPos = new Vector2(transform.localPosition.x, transform.localPosition.y);
        return currLocalPos == _localGridPos;
    }

    /// <summary>
    /// Make it so that movement behavior is set the same now, BUT returned position behavior will NOT change until unlocked again.
    /// If already locked, then nothing will change (original locked positions are preserved).
    /// </summary>
    public void LockResults()
    {
        if(!_isLocked)
        {
            _isLocked = true;
            _lockedGlobalPos = _globalGridPos;
            _lockedLocalPos = _localGridPos;
        }
    }

    /// <summary>
    /// Return movement behavior to default. Returns where it visually is.
    /// </summary>
    public void UnlockResults()
    {
        _isLocked = false;
    }
}