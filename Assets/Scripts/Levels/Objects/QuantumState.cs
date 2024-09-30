using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the data of object that this script is attached to.
/// Also handles exchanging data between different objects in accordance with the quantum mechanic.
/// </summary>
public class QuantumState : MonoBehaviour
{
    [Header("Object Modification Components")]
    [SerializeField, Tooltip("Used for accessing grid positions for visibility checks")]
    private Mover _objMover;
    [SerializeField, Tooltip("Used for vertically flipping sprites during object type change")]
    private SpriteFlipper _objFlipper;

    [Header("Object Data")]
    [SerializeField, Tooltip("Contains all object data pertaining to all types of objects")]
    public ObjectData ObjData;

    #region QUANTUM MECHANICS
    [Header("Quantum State")]
    [SerializeField, Tooltip("used to call sprite flip when quantum objects are randomized.")]
    private ObjectSpriteSwapper _spriteSwapper;

    private bool _isQuantum = false;
    // Stores objectState, and whether the quantum object is visible
    private static List<QuantumState> _quantumObjects = new List<QuantumState>();

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
    /// either adds or removes current QuantumState from quantum objects list.
    /// Used within ToggleQuantum and SetQuantum functions.
    /// </summary>
    private void UpdateQuantumList()
    {
        // add new quantum object
        if (_isQuantum)
        {
            // ONLY add object if it is not already in the list
            foreach (QuantumState obj in _quantumObjects)
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
            foreach (QuantumState obj in _quantumObjects)
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
    /// Exchanges all object data and parent transforms, AND handles cases of quantum entanglement.
    /// Called by GameTimer.
    /// </summary>
    public static void ShuffleHiddenQuantumObjects()
    {
        // GET ALL HIDDEN QUANTUM OBJECTS
        List<QuantumState> hiddenList = new List<QuantumState>();
        foreach(QuantumState obj in _quantumObjects)
        {
            // object counts as hidden if it is not visible AND it is not disabled (don't randomize objects no longer in play)
            if (!VisibilityChecks.IsVisible(obj.gameObject, obj._objMover.GetGlobalGridPos().x, obj._objMover.GetGlobalGridPos().y)
                && !obj.ObjData.IsDisabled)
            {
                hiddenList.Add(obj);
            }
        }
        
        // TRIM HIDDEN OBJECT LIST -> for quantum entanglement
        for (int i = hiddenList.Count-1; i >= 0; i--)
        {
            // determine lowest object at position
            Vector2Int hiddenObjPos = hiddenList[i]._objMover.GetGlobalGridPos();
            QuantumState lowestObj = VisibilityChecks.GetObjectAtPos(hiddenList[i]._objMover, hiddenObjPos.x, hiddenObjPos.y, true);

            // replace hiddenObj with the object below it for shuffling calculations
            if (lowestObj != hiddenList[i])
            {
                // lower object (if not already present) will replace it
                hiddenList.Remove(hiddenList[i]);

                // ensure not adding a duplicate
                bool dupeFound = false;
                foreach (QuantumState otherHiddenObj in hiddenList)
                {
                    if (otherHiddenObj == lowestObj)
                    {
                        dupeFound = true;
                        break;
                    }
                }
                // add lower object
                if (!dupeFound)
                    hiddenList.Add(lowestObj);
            }
        }

        // SHUFFLE QUANTUM LIST (Fisher-Yates shuffle - O(n) time)
        int n = hiddenList.Count;
        while(n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);

            // QUANTUM ENTANGLEMENT CHECKS
            Vector2Int obj1Pos = hiddenList[k]._objMover.GetGlobalGridPos();
            Vector2Int obj2Pos = hiddenList[n]._objMover.GetGlobalGridPos();
            // entangled object on hiddenList[k]
            QuantumState entangledObj1 = VisibilityChecks.GetObjectAtPos(hiddenList[k]._objMover, obj1Pos.x, obj1Pos.y);
            bool entangledSwap1 = false;
            if (entangledObj1 != hiddenList[k])
                entangledSwap1 = true;
            // entangled object on hiddenList[n]
            QuantumState entangledObj2 = VisibilityChecks.GetObjectAtPos(hiddenList[n]._objMover, obj2Pos.x, obj2Pos.y);
            bool entangledSwap2 = false;
            if (entangledObj2 != hiddenList[n])
                entangledSwap2 = true;
            // MUST handle ifs with bools like this to prevent on effect from impacting the next condition
            if(entangledSwap1)
            {
                entangledObj1._objMover.SetGlobalGoal(obj2Pos.x, obj2Pos.y);
                entangledObj1._spriteSwapper.RequireFlip();
            }
            if(entangledSwap2)
            {
                entangledObj2._objMover.SetGlobalGoal(obj1Pos.x, obj1Pos.y);
                entangledObj2._spriteSwapper.RequireFlip();
            }

            // SWAPPING
            // swap object data
            ObjectData val = hiddenList[k].ObjData;
            hiddenList[k].ObjData = hiddenList[n].ObjData;
            hiddenList[n].ObjData = val;

            // swap quantum states
            bool quantumVal = hiddenList[k].IsQuantum();
            hiddenList[k].SetQuantum(hiddenList[n].IsQuantum());
            hiddenList[n].SetQuantum(quantumVal);

            // Ensure swapped tunnel's other tunnel points to NEW tunnel object
            if (hiddenList[n].ObjData.ObjType == ObjectType.Tunnel)
                hiddenList[n].ObjData.OtherTunnel.ObjData.OtherTunnel = hiddenList[n];
            if (hiddenList[k].ObjData.ObjType == ObjectType.Tunnel)
                hiddenList[k].ObjData.OtherTunnel.ObjData.OtherTunnel = hiddenList[k];

            // swap parent transforms, if necessary
            if (hiddenList[k].transform.parent != hiddenList[n].transform.parent)
            {
                Transform parent = hiddenList[k].transform.parent;
                hiddenList[k].transform.parent = hiddenList[n].transform.parent;
                hiddenList[n].transform.parent = parent;
            }
        }

        // ENSURE VISUAL FLIP (even if no change occurred)
        for (int i = 0; i < hiddenList.Count; i++)
            hiddenList[i]._spriteSwapper.RequireFlip();
    }
    #endregion
}