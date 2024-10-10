using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectType
{
    Log,
    Water,
    Rock,
    Bush,
    Tunnel,
    Tree,
    Clock
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

    [Header("Tunnel")]
    [Tooltip("Corresponding tunnel reference.")]
    public QuantumState OtherTunnel;
    [Tooltip("Pairing number of the tunnel; used for sprite swapping properly.")]
    public int TunnelIndex;

    /// <summary>
    /// Returns true if ALL component data elements are equal between the two ObjectData instances.
    /// </summary>
    public bool Equals(ObjectData other)
    {
        // most time/data efficient way to compare (avoids byte comparisons of default Object.Equals(Object).)
        return ObjType == other.ObjType
            && IsDisabled == other.IsDisabled
            && WaterHasLog == other.WaterHasLog
            && WaterHasRock == other.WaterHasRock
            && OtherTunnel == other.OtherTunnel
            && TunnelIndex == other.TunnelIndex;
    }
}


