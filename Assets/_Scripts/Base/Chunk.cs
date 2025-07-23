using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[System.Serializable]
public class Chunk : MonoBehaviour
{
    public const int Size = 16;
    public Vector3 Position;
    public float[,,] ScalarField = new float[Size + 1, Size + 1, Size + 1];
    public Dictionary<Vector3Int, float> ScalarLookup = new();
    public Vector3[,,] VertexPositions = new Vector3[Size + 1, Size + 1, Size + 1];

    public float scale = 0.05f; // Controls frequency of hills
    public float heightScale = 8f; // Controls height of hills
    public float baseHeight = 8f; // Keeps terrain nearly flat
    public float SurfaceLevel = 0.5f; // Surface level for marching cubes
    public int VoxelSize => Size;

    public Chunk(Vector3 worldPosition)
    {
        Position = worldPosition;
        //InitializeFields();
        GenerateSmoothScalarField();
    }

    void InitializeFields()
    {
        for (int x = 0; x <= Size; x++)
        {
            for (int y = 0; y <= Size; y++)
            {
                for (int z = 0; z <= Size; z++)
                {
                    Vector3Int local = new(x, y, z);
                    Vector3 worldPos = Position + new Vector3(x, y, z);
                    float noise = Mathf.PerlinNoise(worldPos.x * 0.1f, worldPos.z * 0.1f) - worldPos.y * 0.01f;

                    ScalarField[x, y, z] = noise;
                    VertexPositions[x, y, z] = worldPos;
                    ScalarLookup[local] = noise;
                }
            }
        }
    }

    public void GenerateSmoothScalarField()
    {
        VertexPositions = new Vector3[Size + 1, Size + 1, Size + 1];
        ScalarLookup = new();
        ScalarField = new float[Size + 1, Size + 1, Size + 1];
        for (int x = 0; x <= Size; x++)
        {
            for (int y = 0; y <= Size; y++)
            {
                for (int z = 0; z <= Size; z++)
                {
                    Vector3Int local = new(x, y, z);
                    Vector3 worldPos = Position + new Vector3(x, y, z);

                    float noiseValue = Perlin3D(worldPos.x, worldPos.y, worldPos.z, scale);
                    float terrainHeight = baseHeight + noiseValue * heightScale;

                    float scalar = terrainHeight - worldPos.y; // Iso-surface comparison
                    ScalarField[x, y, z] = scalar;
                    VertexPositions[x, y, z] = worldPos;
                    ScalarLookup[local] = scalar;
                }
            }
        }
    }

    float Perlin3D(float x, float y, float z, float scale)
    {
        float xy = Mathf.PerlinNoise(x * scale, y * scale);
        float yz = Mathf.PerlinNoise(y * scale, z * scale);
        float xz = Mathf.PerlinNoise(x * scale, z * scale);
        float yx = Mathf.PerlinNoise(y * scale, x * scale);
        float zy = Mathf.PerlinNoise(z * scale, y * scale);
        float zx = Mathf.PerlinNoise(z * scale, x * scale);
        return (xy + yz + xz + yx + zy + zx) / 6f;
    }

    public bool IsValidIndex(Vector3 pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.z >= 0 &&
               pos.x < Size && pos.y < Size && pos.z < Size;
    }

    public Vector3Int WorldToLocalVoxelPos(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - transform.position; // World to local space
        int x = Mathf.FloorToInt(localPos.x / Size);
        int y = Mathf.FloorToInt(localPos.y / Size);
        int z = Mathf.FloorToInt(localPos.z / Size);

        return new Vector3Int(x, y, z);
    }

    public void UpdateMesh()
    {
        List<Vector3> verts = new();
        List<int> tris = new();

        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                for (int z = 0; z < Size; z++)
                {
                    Vector3[] positions = new Vector3[8];
                    float[] l_CubeCorners = new float[8];

                    for (int i = 0; i < 8; i++)
                    {
                        Vector3Int offset = MarchingTable.Vertices[i];
                        int ix = x + offset.x;
                        int iy = y + offset.y;
                        int iz = z + offset.z;

                        positions[i] = VertexPositions[ix, iy, iz]; //Position
                        l_CubeCorners[i] = ScalarLookup.GetValueOrDefault(new Vector3Int(ix, iy, iz), 0f);
                    }

                    MarchingTable.MarchCube(new GridCell(positions, l_CubeCorners), SurfaceLevel, verts, tris);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32; // supports large meshes
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }
}