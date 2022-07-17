using System;
using UnityEngine;

public static class Direction
{
    public enum Axis
    {
        XPos,
        XNeg,
        YPos,
        YNeg,
        ZPos,
        ZNeg
    }
    
    public static Vector3Int AxisToVec(Axis axis) => axis switch
    {
        Axis.XPos => new Vector3Int( 1,  0,  0),
        Axis.XNeg => new Vector3Int(-1,  0,  0),
        Axis.YPos => new Vector3Int( 0,  1,  0),
        Axis.YNeg => new Vector3Int( 0, -1,  0),
        Axis.ZPos => new Vector3Int( 0,  0,  1),
        Axis.ZNeg => new Vector3Int( 0,  0, -1),
        _ => throw new ArgumentOutOfRangeException(nameof(axis), $"Cannot convert {axis} to a Vector3!")
    };
}