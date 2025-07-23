using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class CubeMarching : MonoBehaviour
{
    #region Serialized Fields

    [Header("Grid Settings")] 
    [SerializeField] private int m_Size = 10;
    [SerializeField] private float m_Radius = 10;

    [SerializeField] private float isoLevel = 0f;
    [SerializeField] private bool m_VisualiseScalarField;
    [SerializeField] private bool m_GenerateMesh;

    #endregion

    #region Private Fields

    [SerializeField] private Chunk m_Chunk;

    #endregion

    #region Unity Methods

    private void Start()
    {
        m_Chunk.Position = transform.position;
        m_Chunk.GenerateSmoothScalarField();
        m_Chunk.UpdateMesh();
    }

    private void Update()
    {
      //  m_Chunk.UpdateMesh();
    }

    #endregion

    #region Editor Methods

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (m_GenerateMesh)
        {
            m_GenerateMesh = false;
            m_Chunk.Position = transform.position;
            m_Chunk.GenerateSmoothScalarField();
            m_Chunk.UpdateMesh();
        }

        if (m_VisualiseScalarField && m_Chunk.ScalarField.Length > 0)
        {
            for (int x = 0; x <= m_Chunk.VoxelSize; x++)
            {
                for (int y = 0; y <= m_Chunk.VoxelSize; y++)
                {
                    for (int z = 0; z <= m_Chunk.VoxelSize; z++)
                    {
                        Vector3 position = new Vector3(x, y, z);
                        float value = m_Chunk.ScalarField[x, y, z];
                        Gizmos.color = Color.Lerp(Color.red, Color.green, value);
                        //Gizmos.DrawSphere(position, 0.1f);
                    }
                }
            }
        }
    }
#endif

    #endregion

    #region Mesh Generation

    [ContextMenu("Generate Mesh Sphere")]
    private void GenerateMesh()
    {
        float [,,] m_ScalarField = GenerateSphereScalarField(m_Size, m_Size,m_Size,
            new Vector3(m_Size / 2f, m_Size / 2f, m_Size / 2f), m_Radius);

        List<Vector3> verts = new();
        List<int> tris = new();

        for (int x = 0; x < m_Size; x++)
        {
            for (int y = 0; y < m_Size; y++)
            {
                for (int z = 0; z < m_Size; z++)
                {
                    Vector3[] positions = new Vector3[8];
                    float[] l_CubeCorners = new float[8];

                    for (int i = 0; i < 8; i++)
                    {
                        Vector3Int offset = MarchingTable.Vertices[i];
                        int ix = x + offset.x;
                        int iy = y + offset.y;
                        int iz = z + offset.z;

                        positions[i] = new Vector3(ix, iy, iz); //Position
                        l_CubeCorners[i] = m_ScalarField[ix, iy, iz];
                    }

                    MarchingTable.MarchCube(new GridCell(positions, l_CubeCorners), isoLevel, verts, tris);
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

    #endregion

    #region Scalar Field Generation

    private float[,,] GenerateSphereScalarField(int a_Width, int a_Height, int a_Depth, Vector3 a_Center,
        float a_Radius)
    {
        float[,,] field = new float[a_Width + 1, a_Height + 1, a_Depth + 1];

        for (int x = 0; x <= a_Width; x++)
        {
            for (int y = 0; y <= a_Height; y++)
            {
                for (int z = 0; z <= a_Depth; z++)
                {
                    Vector3 position = new Vector3(x, y, z);
                    float l_Dist = Vector3.Distance(position, a_Center);
                    field[x, y, z] = l_Dist - a_Radius;
                }
            }
        }

        return field;
    }

    #endregion
}