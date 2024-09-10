using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the type of object that this script is attached to.
/// It is planned for this script to also handle making calls to visually swap objects accordingly.
/// </summary>
public class ObjectState : MonoBehaviour
{
    [Header("Other Components")]
    [SerializeField, Tooltip("Used for accessing grid positions for visibility checks")]
    private ObjectMover _objMover;
    [SerializeField, Tooltip("Used for vertically flipping sprites during object type change")]
    private ObjectFlipper _objFlipper;

    [Header("Sprites")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite[] _logSprites;
    [SerializeField] private Sprite[] _waterSprites;

    private const int SPRITE_SHRINK = 0;
    private const int SPRITE_NORMAL = 1;

    private ObjectType _spriteType;
    private bool _requiresFlip = false;

    [Header("Object Type")]
    public ObjectType ObjType;
    public enum ObjectType
    {
        Log,
        Water,
        Rock,
        TallRock,
        Bush,
        TallBush,
        Tunnel,
        Pickup
    }
    

    [Header("Quantum State")]
    [SerializeField, Tooltip("game object containing animated particle sprite")]
    private GameObject _quantumParticles;

    private bool _isQuantum = false;
    // Stores objectState, and whether the quantum object is visible
    private static List<ObjectState> _quantumObjects = new List<ObjectState>();

    // Start is called before the first frame update
    void Start()
    {
        // initialize to starting type
        _spriteType = ObjType;
    }

    // Update is called once per frame
    void Update()
    {
        // Update particles to match actual quantum state
        if (_quantumParticles.activeInHierarchy != _isQuantum)
            _quantumParticles.SetActive(_isQuantum);

        // Must handle moving sprite towards matching actual object
        if (_requiresFlip)
        {
            // Ready to restore sprite to normal
            if(_objFlipper.GetCurrentScaleY() == SPRITE_SHRINK)
            {
                // flip back to base scale
                _objFlipper.SetScaleY(SPRITE_NORMAL);
                // ensure sprite update occurs
                _spriteType = ObjType;
                // flip concluded
                _requiresFlip = false;
            }
            else // sprite should be shrinking if not yet at fully shrunk
                _objFlipper.SetScaleY(SPRITE_SHRINK);
        }

        // actually update the sprite
        switch (_spriteType)
        {
            case ObjectState.ObjectType.Log:
                _spriteRenderer.sprite = _logSprites[0];
                break;
            case ObjectState.ObjectType.Water:
                _spriteRenderer.sprite = _waterSprites[0];
                break;
            case ObjectState.ObjectType.Rock:
                break;
            case ObjectState.ObjectType.TallRock:
                break;
            case ObjectState.ObjectType.Bush:
                break;
            case ObjectState.ObjectType.TallBush:
                break;
            case ObjectState.ObjectType.Tunnel:
                break;
            case ObjectState.ObjectType.Pickup:
                break;
        }
    }

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
        List<ObjectType> shuffledTypes = new List<ObjectType>();
        foreach(ObjectState obj in _quantumObjects)
        {
            if (!VisibilityCheck.IsVisible(obj.gameObject, obj._objMover.GetGlobalGridPos().x, obj._objMover.GetGlobalGridPos().y))
            {
                hiddenList.Add(obj);
                shuffledTypes.Add(obj.ObjType);
            }
        }
        
        // shuffle hiddenList (Fisher-Yates shuffle - O(n) time)
        int n = shuffledTypes.Count;
        while(n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            // swap operation
            ObjectType val = shuffledTypes[k];
            shuffledTypes[k] = shuffledTypes[n];
            shuffledTypes[n] = val;
        }

        // swap values of objects to shuffled values (effectively swapping states)
        for(int i = 0; i < hiddenList.Count; i++)
        {
            hiddenList[i].ObjType = shuffledTypes[i];

            // ensure all quantum objects undergo visual flip (even if no change occurred)
            hiddenList[i]._requiresFlip = true;

            // TODO: Update this to also handle other object data values that need to be swapped (i.e. water w or w/o planks state)
            // this will likely require instead a shuffling of indexes and then iteration through one list updating stats accordingly,
            // based on a copy of the original hidden list
        }
    }
}
