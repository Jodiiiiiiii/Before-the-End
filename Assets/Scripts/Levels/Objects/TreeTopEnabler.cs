using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles toggling of tree top sprite visibility based on whether the object is a tree or not.
/// </summary>
public class TreeTopEnabler : MonoBehaviour
{
    [SerializeField, Tooltip("Reference to access whether this object is a tree or not")]
    private QuantumState _quantumState;
    [SerializeField, Tooltip("Used to enable or disable the sprite")]
    private SpriteRenderer _treeTopRenderer;

    // Update is called once per frame
    void Update()
    {
        _treeTopRenderer.enabled = _quantumState.ObjData.ObjType == ObjectType.Tree;
    }
}
