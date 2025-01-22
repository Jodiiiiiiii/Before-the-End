using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
