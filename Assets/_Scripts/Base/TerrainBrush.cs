using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBrush : MonoBehaviour
{
    public Vector3Int brushSize = new Vector3Int(4, 4, 4); // Width, Height, Depth
    public float raiseSpeed = 1f; // Speed of scalar increase
    public float maxRaiseValue = 1f; // Maximum scalar increase
    public Chunk targetChunk; // Assign the chunk here

    private bool isBrushing = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            StartCoroutine(ApplyBrush());
    }

    IEnumerator ApplyBrush()
    {
        isBrushing = true;
        Bounds bounds = new Bounds(transform.position, new Vector3(brushSize.x, brushSize.y, brushSize.z));
        List<Vector3> selectedPositions = new List<Vector3>();
        foreach (Vector3 l_Pos in targetChunk.VertexPositions)
        {
            if(bounds.Contains(l_Pos))
            {
                selectedPositions.Add(l_Pos);
                //Debug.Log("Contains: " + l_Pos);
            }
        }
        //  Vector3 mouseWorldPos = GetMouseWorldPosition();
        //Vector3Int chunkLocalPos = targetChunk.WorldToLocalVoxelPos(mouseWorldPos);
        //Vector3Int chunkLocalPos =(Vector3Int) mouseWorldPos;
        float elapsed = 0f;
        float duration = 1f; // seconds to complete extrusion

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float raiseAmount = Mathf.Lerp(0, maxRaiseValue, t);

            ApplyRaise(selectedPositions, raiseAmount);
            targetChunk.UpdateMesh();

            elapsed += Time.deltaTime;
            yield return null;
        }

        isBrushing = false;
    }

    void ApplyRaise(List<Vector3> positions, float amount)
    {
        foreach (Vector3 l_Pos in positions)
        {
            Vector3Int chunkLocalPos = targetChunk.WorldToLocalVoxelPos(l_Pos);
            targetChunk.ScalarField[chunkLocalPos.x, chunkLocalPos.y, chunkLocalPos.z] = amount;
            targetChunk.ScalarLookup[chunkLocalPos] = targetChunk.ScalarField[chunkLocalPos.x, chunkLocalPos.y, chunkLocalPos.z];
        }
                    
    }


    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }

        return Vector3.zero;
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        /*
        if (!Application.isPlaying) return;

        if (targetChunk == null) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3Int chunkLocalPos = targetChunk.WorldToLocalVoxelPos(mouseWorldPos);

        Vector3 worldBrushCenter = mouseWorldPos;
        */

        /*Vector3 brushWorldSize = new Vector3(
            brushSize.x * targetChunk.VoxelSize,
            brushSize.y * targetChunk.VoxelSize,
            brushSize.z * targetChunk.VoxelSize
        );*/
        Vector3 brushWorldSize = new Vector3(
            brushSize.x,
            brushSize.y,
            brushSize.z
        );
        Bounds bounds = new Bounds(transform.position, new Vector3(brushSize.x, brushSize.y, brushSize.z));
        List<Vector3> selectedPositions = new List<Vector3>();
        foreach (Vector3 l_Pos in targetChunk.VertexPositions)
        {
            if(bounds.Contains(l_Pos))
            {
                selectedPositions.Add(l_Pos);
                Vector3Int chunkLocalPos = targetChunk.WorldToLocalVoxelPos(l_Pos);
                Gizmos.color = Color.Lerp(Color.red, Color.green, targetChunk.ScalarLookup[chunkLocalPos]);
                Gizmos.DrawSphere(l_Pos, 0.1f);
                //Debug.Log("Contains: " + l_Pos);
            }
        }
        ApplyRaise(selectedPositions, maxRaiseValue);
        targetChunk.UpdateMesh();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, brushWorldSize);
    }
#endif
}