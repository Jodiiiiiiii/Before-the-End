using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the data of object that this script is attached to.
/// Also handles exchanging data between different objects in accordance with the quantum mechanic.
/// </summary>
public class ObjectState : MonoBehaviour
{
    [Header("Object Modification Components")]
    [SerializeField, Tooltip("Used for accessing grid positions for visibility checks")]
    private ObjectMover _objMover;
    [SerializeField, Tooltip("Used for vertically flipping sprites during object type change")]
    private SpriteFlipper _objFlipper;

    [Header("Object Data")]
    [SerializeField, Tooltip("Contains all object data pertaining to all types of objects")]
    public ObjectData ObjData;

    #region QUANTUM MECHANICS
    // Update is called once per frame
    void Update()
    {
        // Update particles to match actual quantum state
        if (_quantumParticles.activeInHierarchy != _isQuantum)
            _quantumParticles.SetActive(_isQuantum);
    }

    [Header("Quantum State")]
    [SerializeField, Tooltip("game object containing animated particle sprite")]
    private GameObject _quantumParticles;
    [SerializeField, Tooltip("used to call sprite flip when quantum objects are randomized.")]
    private ObjectSpriteSwapper _spriteSwapper;

    private bool _isQuantum = false;
    // Stores objectState, and whether the quantum object is visible
    private static List<ObjectState> _quantumObjects = new List<ObjectState>();

    public bool IsQuantum()
    {
        return _isQuantum;
    }

    /// <summary>
    /// flip stored IsQuantum state.
    /// Updates static list of quantum objects.
    /// </summary>
    public void ToggleQuantum()
    {
        _isQuantum = !_isQuantum;

        UpdateQuantumList();
    }

    /// <summary>
    /// For directly setting, instead of just toggling, quantum state.
    /// Useful in UndoObject in particular.
    /// Updates static list of quantum objects.
    /// </summary>
    public void SetQuantum(bool newState)
    {
        _isQuantum = newState;

        UpdateQuantumList();
    }

    /// <summary>
    /// either adds or removes current ObjectState from quantum objects list.
    /// Used within ToggleQuantum and SetQuantum functions.
    /// </summary>
    private void UpdateQuantumList()
    {
        // add new quantum object
        if (_isQuantum)
        {
            // ONLY add object if it is not already in the list
            foreach (ObjectState obj in _quantumObjects)
            {
                if (obj.Equals(this))
                {
                    return;
                }
            }
            _quantumObjects.Add(this);
        }

        else // removing quantum state
        {
            // remove quantum object from list (if possible)
            foreach (ObjectState obj in _quantumObjects)
            {
                if (obj.Equals(this))
                {
                    _quantumObjects.Remove(obj);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Shuffles hidden quantum objects amongst themselves. OR does nothing if no such objects exist.
    /// Called by GameTimer.
    /// </summary>
    public static void ShuffleHiddenQuantumObjects()
    {
        // make new lists of only the hidden objects
        List<ObjectState> hiddenList = new List<ObjectState>();
        foreach(ObjectState obj in _quantumObjects)
        {
            if (!VisibilityCheck.IsVisible(obj.gameObject, obj._objMover.GetGlobalGridPos().x, obj._objMover.GetGlobalGridPos().y))
                hiddenList.Add(obj);
        }

        // shuffle object data (Fisher-Yates shuffle - O(n) time)
        int n = hiddenList.Count;
        while(n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            // swap operation
            ObjectData val = hiddenList[k].ObjData;
            hiddenList[k].ObjData = hiddenList[n].ObjData;
            hiddenList[n].ObjData = val;
        }

        // ensure all quantum objects undergo visual flip (even if no change occurred)
        for (int i = 0; i < hiddenList.Count; i++)
            hiddenList[i]._spriteSwapper.RequireFlip();
    }
    #endregion
}