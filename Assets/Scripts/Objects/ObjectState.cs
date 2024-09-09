using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the type of object that this script is attached to.
/// It is planned for this script to also handle making calls to visually swap objects accordingly.
/// </summary>
public class ObjectState : MonoBehaviour
{
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
    public ObjectType ObjType;

    private bool _isQuantum = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsQuantum()
    {
        return _isQuantum;
    }

    /// <summary>
    /// flip stored IsQuantum state
    /// </summary>
    public void ToggleQuantum()
    {
        _isQuantum = !_isQuantum;
    }
}
