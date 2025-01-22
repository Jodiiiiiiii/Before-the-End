using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores all data pertinent to a node connection, as used primarily by TravelNode.cs and LevelSelectControls.cs
/// </summary>
[System.Serializable]
public class NodeConnectionData
{
    public enum Direction
    {
        Up, 
        Right, 
        Down, 
        Left
    }

    public TravelNode Node;
    public Direction Dir;
    [HideInInspector]
    public bool Unlocked;
}
