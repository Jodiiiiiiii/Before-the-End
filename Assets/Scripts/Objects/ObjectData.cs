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
    // TODO: add isDisabled variable here to disable all functionality/sprite (i.e. destruction, falling into water, etc.)

    [Header("Water")]
    [Tooltip("Whether water contains a log object (traversable)")]
    public bool WaterHasLog;
    [Tooltip("Whether water contains a rock object (traversable)")]
    public bool WaterHasRock;

    public ObjectType GetObjectType()
    {
        return ObjType;
    }

    public void SetObjectType(ObjectType newType)
    {
        ObjType = newType;
    }

    public bool IsWaterHasLog()
    {
        return WaterHasLog;
    }

    public bool IsWaterHasRock()
    {
        return WaterHasRock;
    }

    public void SetWaterHasLog(bool newState)
    {
        WaterHasLog = newState;
    }

    public void SetWaterHasRock(bool newState)
    {
        WaterHasRock = newState;
    }
}


