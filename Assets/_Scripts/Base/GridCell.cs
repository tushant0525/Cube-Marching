using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grid Cell is Cube  with positions and scalar values of each corner
/// </summary>
public struct GridCell
{
    public Vector3[] Positions;
    public float[] Values;
    private Dictionary<Vector3,float> ValueMap;

    public GridCell(Vector3[] a_Positions, float[] a_Values)
    {
        Positions = a_Positions;
        Values = a_Values;
        ValueMap = new Dictionary<Vector3, float>();
        for (int i = 0; i < a_Positions.Length; i++)
        {
            ValueMap.Add(a_Positions[i], a_Values[i]);
        }
    }

    public float GetValue(Vector3 a_Pos)
    {
        return ValueMap.GetValueOrDefault(a_Pos, -1f);
    }
}
