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
    Clock,
    Compy,
    Fire,
    Void
}

[System.Serializable]
public struct ObjectData
{
    [Header("All objects")]
    [Tooltip("Which major object type the object is")]
    public ObjectType ObjType;
    [Tooltip("whether the object is interactable OR simple gone (disabled, destroyed, etc.).")]
    public bool IsDisabled;

    [Header("Fire (log/bush)")]
    [Tooltip("Whether this log/bush is currently on fire")]
    public bool IsOnFire;

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
    public bool DataEquals(ObjectData other)
    {
        // most time/data efficient way to compare (avoids byte comparisons of default Object.Equals(Object).)
        return ObjType == other.ObjType
            && IsDisabled == other.IsDisabled
            && IsOnFire == other.IsOnFire
            && WaterHasLog == other.WaterHasLog
            && WaterHasRock == other.WaterHasRock
            && OtherTunnel == other.OtherTunnel
            && TunnelIndex == other.TunnelIndex;
    }

    /// <summary>
    /// Identical to DataEquals, except ignores the OtherTUnnel QuantumState reference.
    /// This is useful for visual sprite swapping since this is the only data value that should NOT cause a visual flip.
    /// </summary>
    public bool DataEqualsExceptTunnelRef(ObjectData other)
    {
        return ObjType == other.ObjType
            && IsDisabled == other.IsDisabled
            && IsOnFire == other.IsOnFire
            && WaterHasLog == other.WaterHasLog
            && WaterHasRock == other.WaterHasRock
            && TunnelIndex == other.TunnelIndex;
    }

    /// <summary>
    /// Basically a copy constructor to copy by value and not reference.
    /// </summary>
    public ObjectData CopyOf()
    {
        ObjectData newCopy = new ObjectData();
        newCopy.ObjType = ObjType;
        newCopy.IsDisabled = IsDisabled;
        newCopy.IsOnFire = IsOnFire;
        newCopy.WaterHasLog = WaterHasLog;
        newCopy.WaterHasRock = WaterHasRock;
        newCopy.OtherTunnel = OtherTunnel;
        newCopy.TunnelIndex = TunnelIndex;
        return newCopy;
    }
}