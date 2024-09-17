using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ObjectState;

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

[System.Serializable]
public struct ObjectData
{
    [Header("All objects")]
    [Tooltip("Which major object type the object is")]
    public ObjectType ObjType;
    [Tooltip("whether the object is interactable OR simple gone (disabled, destroyed, etc.).")]
    public bool IsDisabled;

    [Header("Water")]
    [Tooltip("Whether water contains a log object (traversable)")]
    public bool WaterHasLog;
    [Tooltip("Whether water contains a rock object (traversable)")]
    public bool WaterHasRock;
}


