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

    // Quantum state variables
    private bool _isQuantum = false;
    [SerializeField, Tooltip("game object containing animated particle sprite")]
    private GameObject _quantumParticles;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Update particles to match actual quantum state
        if (_quantumParticles.activeInHierarchy != _isQuantum)
            _quantumParticles.SetActive(_isQuantum);
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

    /// <summary>
    /// For directly setting, instead of just toggling, quantum state.
    /// Useful in UndoObject in particular
    /// </summary>
    public void SetQuantum(bool newState)
    {
        _isQuantum = newState;
    }
}
