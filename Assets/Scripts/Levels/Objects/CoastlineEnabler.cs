using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles enabling visual coastline flourish on water tiles as necessary based on adjacent connections to other water tiles.
/// </summary>
public class CoastlineEnabler : MonoBehaviour
{
    [SerializeField, Tooltip("Used to tell which object type the current object is (only water has coastline).")]
    private QuantumState _state;

    [Header("Coastline Visual Objects")]
    [SerializeField, Tooltip("Used to enable top coastline side.")]
    private GameObject _topCoastline;
    [SerializeField, Tooltip("Used to enable right coastline side.")]
    private GameObject _rightCoastline;
    [SerializeField, Tooltip("Used to enable bottom coastline side.")]
    private GameObject _bottomCoastline;
    [SerializeField, Tooltip("Used to enable left coastline side.")]
    private GameObject _leftCoastline;

    // stored references to reduce times that object at position must be retrieved
    private QuantumState _topObject = null;
    private QuantumState _rightObject = null;
    private QuantumState _bottomObject = null;
    private QuantumState _leftObject = null;

    private void Start()
    {
        // Precondition checking for object configuration
        if (_state.transform.parent is null || _state.transform.parent.parent is null)
            throw new System.Exception("ALL Objects MUST be a child of a child of a panel.");

        // ensure proper state at start of scene
        //UpdateLogic();
    }

    private void OnEnable()
    {
        UndoHandler.ActionOccur += CheckAtEndOfFrame;
        UndoHandler.UndoOccur += CheckAtEndOfFrame;
    }

    private void OnDisable()
    {
        UndoHandler.ActionOccur -= CheckAtEndOfFrame;
        UndoHandler.UndoOccur -= CheckAtEndOfFrame;
    }

    private void CheckAtEndOfFrame()
    {
        StartCoroutine(WaitCheckAtEndOfFrame());
    }

    private IEnumerator WaitCheckAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        UpdateLogic();
    }

    /// <summary>
    /// Updates visuals of coastlines based on whether there are adjacent water tiles next to the current tile.
    /// </summary>
    private void UpdateLogic()
    {
        // skip checks and disable if object is not water
        if (_state.ObjData.ObjType != ObjectType.Water)
        {
            if (_topCoastline.activeSelf) _topCoastline.SetActive(false);
            if (_rightCoastline.activeSelf) _rightCoastline.SetActive(false);
            if (_bottomCoastline.activeSelf) _bottomCoastline.SetActive(false);
            if (_leftCoastline.activeSelf) _leftCoastline.SetActive(false);

            return;
        }

        // Update top coastline
        UpdateCoastline(ref _topObject, ref _topCoastline, Vector2Int.up);

        // right coastline update
        UpdateCoastline(ref _rightObject, ref _rightCoastline, Vector2Int.right);

        // bottom coastline update
        UpdateCoastline(ref _bottomObject, ref _bottomCoastline, Vector2Int.down);

        // left coastline update
        UpdateCoastline(ref _leftObject, ref _leftCoastline, Vector2Int.left);
    }

    /// <summary>
    /// Checks for object, and enables coastline in direction, if necessary.
    /// Only retrieves new object is current reference has changed in some way or is null.
    /// </summary>
    private void UpdateCoastline(ref QuantumState currRef, ref GameObject coastlineRef, Vector2Int dir)
    {
        // Precondition: proper object configuration
        if (currRef is not null && (currRef.transform.parent is null || currRef.transform.parent.parent is null))
            throw new System.Exception("ALL Objects MUST be a child of a child of a panel.");

        // Precondition: Proper panel configuration
        if (!_state.transform.parent.parent.TryGetComponent(out PanelStats currPanel))
            throw new System.Exception("ALL Panels MUST have a PanelStats.");

        Vector2Int checkPos = _state.ObjMover.GetGlobalGridPos() + dir;

        // retrieve new object if
        // (1) we have no reference
        // (2) the parent's no longer match
        // (3) or the positions no longer match
        if (currRef is null ||
            (currRef.transform.parent.parent != _state.transform.parent.parent) ||
            currRef.ObjMover.GetGlobalGridPos() - dir != _state.ObjMover.GetGlobalGridPos())
        {
            currRef = VisibilityChecks.GetObjectAtPos(_state.ObjMover, checkPos.x, checkPos.y, true);
        }

        // enable if there is no adjacent water, otherwise disable
        // does NOT enable if adjacent position is outside of current panel
        // (based on bounds, not visibility to avoid weird behaviors for water blocked by another panel)
        if ((currRef is null || currRef.ObjData.ObjType != ObjectType.Water)
            && currPanel.IsPosInBounds(checkPos.x, checkPos.y))
        {
            coastlineRef.SetActive(true);
        }
        else
        {
            coastlineRef.SetActive(false);
        }
    }
}
