using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

}

// Chunk class. This class will manage how the chunks are stored, drawn and updated
public class Chunk
{
    public static int PATCH_SIZE_X = 1;
    public static int PATCH_SIZE_Y = 1;
    public static int PATCH_SIZE_Z = 1;

    [HideInInspector]
    public static int SIZE_X = 8;
    [HideInInspector]
    public static int SIZE_Y = 24;
    [HideInInspector]
    public static int SIZE_Z = 8;

    #region Chunk Data Attributes
    public Cell[,,] _cells;
    public CELL_TYPE[,,] _cellTypes;
    public long[] _cellFlags;
    private int _x, _y, _z;
    public Vector3 ChunkCoords { get { return new Vector3(_x, _y, _z); } }
    #endregion

    #region Chunk Mesh Attributes
    private GameObject _meshGO;
    private MeshCollider _meshCollider;
    private Mesh _mesh;
    #endregion

    public Chunk(float seed, int x, int y, int z)
    {
        _cells = new Cell[SIZE_X, SIZE_Y, SIZE_Z];
        _cellFlags = new long[SIZE_Y];
        for (int i = 0; i < SIZE_Y; ++i) {
            _cellFlags[i] = 0x00000000;
        }
        _cellTypes = new CELL_TYPE[SIZE_X, SIZE_Y, SIZE_Z];
        _x = x;
        _y = y;
        _z = z;
        Generate(seed, x, z);
        //CreateMesh();
    }

    public Chunk(GameObject go, Cell[,,] cells, long[] cellFlags, CELL_TYPE[,,] cellTypes)
    {
        _cells = cells;
        _cellFlags = cellFlags;
        _cellTypes = cellTypes;
    }

    public void Draw()
    {
        /*for (int i = 0; i < WorldGeneratorOptimized.s.CHUNK_SIZE_X; ++i)
        {
            for (int j = 0; j < WorldGeneratorOptimized.s.CHUNK_SIZE_Y; ++j)
            {
                for (int k = 0; k < WorldGeneratorOptimized.s.CHUNK_SIZE_Z; ++k)
                {
                    bool isThereCell = (_cellFlags[j] & (1 << (k * 8 + i))) != 0;

                    if (isThereCell)
                    {
                        Vector3 cellPos = _go.transform.position + new Vector3(i, j, k);

                        for (int f = 0; f < 6; ++f)
                        {

                            Vector3 neighCellPos = cellPos + WorldGeneratorOptimized.s.GetMeshNormal(f);
                            bool cellInTarget = WorldGeneratorOptimized.s.IsThereCellInPosition(neighCellPos);

                            if (!cellInTarget)
                            {
                                //Cell c = Instantiate(s.cube, cellPos, Quaternion.identity, _go.transform);
                                Cell c = WorldGeneratorOptimized.s._poolDictionary[_cellTypes[i, j, k]].Dequeue();
                                c.transform.parent = _go.transform;
                                c.transform.position = cellPos;
                                c.name = "Block " + i + "_" + j + "_" + k;
                                c.Place();
                                _cells[i, j, k] = c;

                                break;
                            }
                        }
                    }
                }
            }
        }*/
    }

    public void Hide()
    {
        /*for (int i = 0; i < WorldGeneratorOptimized.s.CHUNK_SIZE_X; ++i)
        {
            for (int j = 0; j < WorldGeneratorOptimized.s.CHUNK_SIZE_Y; ++j)
            {
                for (int k = 0; k < WorldGeneratorOptimized.s.CHUNK_SIZE_Z; ++k)
                {
                    bool isThereCell = (_cellFlags[j] & (1 << (k * 8 + i))) != 0;
                    if (isThereCell)
                    {
                        try
                        {
                            Cell c = _cells[i, j, k];
                            _cells[i, j, k] = null;
                            c.Hide();
                            WorldGeneratorOptimized.s._poolDictionary[c.type].Enqueue(c);
                        }
                        catch (Exception e)
                        {
                            // Inner cells of the chunk are not instatiated, even though its _cellFlag is true, so I catch this exception to ensure the chunk is destroyed properly
                        }
                    }
                }
            }
        }*/
    }

    #region Cell Flag Management
    public void SetCellFlag(int x, int y, int z, bool value)
    {
        int bitPosition = z * 8 + x;
        if (value)
        {
            _cellFlags[y] |= ((long) 1 << bitPosition);
        }
        else
        {
            _cellFlags[y] &= ~((long) 1 << bitPosition);
        }
    }

    public bool GetCellFlag(int x, int y, int z)
    {
        int bitPosition = z * 8 + x;
        
        return (_cellFlags[y] & ((long) 1 << bitPosition)) != 0;
    }
    #endregion

    #region Chunk Generation

    public void Generate(float seed, int cx, int cz)
    {
        //GameObject chunkGO = new GameObject("Chunk_" + cx.ToString() + "_" + cz.ToString());
        //chunkGO.transform.position = new Vector3(cx * SIZE_X, 0, cz * SIZE_Z);

        int N_PATCHES_X = SIZE_X / PATCH_SIZE_X;
        int N_PATCHES_Y = SIZE_Y / PATCH_SIZE_Y;
        int N_PATCHES_Z = SIZE_Z / PATCH_SIZE_Z;

        float noiseScale = 0.02f;

        for (int x = 0; x < N_PATCHES_X; ++x)
        {
            for (int y = 0; y < N_PATCHES_Y; ++y)
            {
                for (int z = 0; z < N_PATCHES_Z; ++z)
                {
                    float noiseValue2D = 0.5f + (float)NoiseS3D.NoiseCombinedOctaves(seed + (cx * SIZE_X + x * PATCH_SIZE_X) * noiseScale, seed + (cz * SIZE_Z + z * PATCH_SIZE_Z) * noiseScale);
                    float noiseValue3D = (float)NoiseS3D.NoiseCombinedOctaves(seed + (cx * SIZE_X + x * PATCH_SIZE_X) * noiseScale, seed + (y * PATCH_SIZE_Y) * noiseScale, seed + (cz * SIZE_Z + z * PATCH_SIZE_Z) * noiseScale);
                    // Remap the value to 0 - 1 
                    noiseValue3D = ExtensionMethods.Remap(noiseValue3D, -1, 1, 0, 1);

                    float heightPerc = (float)y / (float)N_PATCHES_Y;
                    float density = WorldGeneratorOptimized.s.heightDensityCurve.Evaluate(heightPerc);

                    noiseValue2D = ExtensionMethods.Remap(noiseValue2D, -1, 1, 0, 1);
                    bool belowY = ((float)y / SIZE_Y) < noiseValue2D;
                    bool surface = Math.Abs(((float)y / SIZE_Y) - noiseValue2D) < 0.05f;
                    if (noiseValue3D < density && belowY)
                    {
                        GeneratePatch(x, y, z, surface);
                    }

                }
            }
        }
    }

    private void GeneratePatch(int px, int py, int pz, bool surface = false)
    {
        //UnityEngine.Debug.Log("Generating patch " + px + "_" + py + "_" + pz);
        //GameObject patchGO = new GameObject("Patch_" + px + "_" + py + "_" + pz);
        //patchGO.transform.parent = chunkGO.transform;
        for (int y = 0; y < PATCH_SIZE_Y; ++y)
        {
            for (int x = 0; x < PATCH_SIZE_X; ++x)
            {
                for (int z = 0; z < PATCH_SIZE_Z; ++z)
                {
                    int nBit = (pz * PATCH_SIZE_Z + z) * 8 + (px * PATCH_SIZE_X + x);

                    SetCellFlag(px * PATCH_SIZE_X + x, py * PATCH_SIZE_Y + y, pz * PATCH_SIZE_Z + z, true);
                    
                }
            }
        }

        for (int y = 0; y < PATCH_SIZE_Y; ++y)
        {
            for (int x = 0; x < PATCH_SIZE_X; ++x)
            {
                for (int z = 0; z < PATCH_SIZE_Z; ++z)
                {
                    int nBit = (pz * PATCH_SIZE_Z + z) * 8 + (px * PATCH_SIZE_X + x);

                    _cellTypes[px * PATCH_SIZE_X + x, py * PATCH_SIZE_Y + y, pz * PATCH_SIZE_Z + z] = AssignCellType(py, y, surface);
                }
            }
        }
    }

    private CELL_TYPE AssignCellType(int patchY, int cellY, bool surface = false)
    {
        int N_PATCHES_Y = SIZE_Y / PATCH_SIZE_Y;
        int globalCellY = patchY * PATCH_SIZE_Y + cellY;

        if ((patchY == N_PATCHES_Y - 1 && cellY == PATCH_SIZE_Y - 1) || surface) return CELL_TYPE.GRASS;
        else if (patchY == 0 && cellY == 0) return CELL_TYPE.BOTTOM;    // Limit cell in the bottom
        else if (globalCellY < 10) return CELL_TYPE.STONE;
        else return CELL_TYPE.GROUND;

    }

    public CELL_TYPE GetCellType(int x, int y, int z) { return _cellTypes[x, y, z]; }

    public void SetCellType(int x, int y, int z, CELL_TYPE type) { _cellTypes[x, y, z] = type; }
    #endregion

    #region Chunk Mesh
    public void CreateMesh()
    {
        _meshGO = new GameObject("Terrain_" + _x + "_" + _y + "_" + _z);
        _meshGO.transform.parent = World.s.ChunkLayers[_y].transform;
        _meshGO.layer = 9;
        MeshRenderer meshRenderer = _meshGO.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Resources.Load<Material>("Materials/M_Bioma1");
        MeshFilter meshFilter = _meshGO.AddComponent<MeshFilter>();

        //vertices = new List<Vector3>();
        _mesh = new Mesh();

        // Assign vertices
        _mesh.vertices = GenerateVertices();
        //vertices.AddRange(mesh.vertices);   // Gizmos

        // Assign triangles
        _mesh.triangles = GenerateTriangles();

        // Assign UV
        _mesh.uv = GenerateUV();


        meshFilter.mesh = _mesh;
        _meshCollider = _meshGO.AddComponent<MeshCollider>();
        _meshGO.transform.position = new Vector3(_x * SIZE_X, _y * SIZE_Y, _z * SIZE_Z);
    }

    public void RegenerateMesh() 
    {
        _mesh.triangles = GenerateTriangles();
        _mesh.uv = GenerateUV();
        UnityEngine.Object.DestroyImmediate(_meshCollider, true);
        _meshCollider = _meshGO.AddComponent<MeshCollider>();
    }

    public void UpdateMesh() 
    {
        _mesh.triangles = GenerateTriangles();
        _mesh.uv = GenerateUV();
    }

    private Vector3[] GenerateVertices()
    {
        List<Vector3> vertices = new List<Vector3>();

        for (int k = 0; k < SIZE_Y; ++k)
        {
            for (int i = 0; i < SIZE_X; ++i)
            {
                for (int j = 0; j < SIZE_Z; ++j)
                {
                    // Down quad
                    vertices.Add(new Vector3(i, k, j)); // 1
                    vertices.Add(new Vector3(i, k, j + 1)); // 2
                    vertices.Add(new Vector3(i + 1, k, j)); // 3
                    vertices.Add(new Vector3(i + 1, k, j + 1)); // 4

                    // Up quad
                    vertices.Add(new Vector3(i, k + 1, j + 1)); // 6
                    vertices.Add(new Vector3(i, k + 1, j)); // 5    
                    vertices.Add(new Vector3(i + 1, k + 1, j + 1)); // 8
                    vertices.Add(new Vector3(i + 1, k + 1, j)); // 7

                    // Left Quad
                    vertices.Add(new Vector3(i, k, j)); // 1
                    vertices.Add(new Vector3(i + 1, k, j)); // 3
                    vertices.Add(new Vector3(i, k + 1, j)); // 5    
                    vertices.Add(new Vector3(i + 1, k + 1, j)); // 7
                    
                    // Right Quad
                    vertices.Add(new Vector3(i + 1, k, j + 1)); // 4
                    vertices.Add(new Vector3(i, k, j + 1)); // 2
                    vertices.Add(new Vector3(i + 1, k + 1, j + 1)); // 8
                    vertices.Add(new Vector3(i, k + 1, j + 1)); // 6
                    
                    // Front Quad
                    vertices.Add(new Vector3(i, k, j + 1)); // 2
                    vertices.Add(new Vector3(i, k, j)); // 1
                    vertices.Add(new Vector3(i, k + 1, j + 1)); // 6
                    vertices.Add(new Vector3(i, k + 1, j)); // 5    
                    
                    // Back Quad
                    vertices.Add(new Vector3(i + 1, k, j)); // 3
                    vertices.Add(new Vector3(i + 1, k, j + 1)); // 4
                    vertices.Add(new Vector3(i + 1, k + 1, j)); // 7
                    vertices.Add(new Vector3(i + 1, k + 1, j + 1)); // 8
                }
            }
        }

        return vertices.ToArray();
    }

    private int[] GenerateTriangles()
    {
        List<int> triangles = new List<int>();

        for (int k = 0; k < SIZE_Y; ++k)
        {
            for (int i = 0; i < SIZE_X; ++i)
            {
                for (int j = 0; j < SIZE_Z; ++j)
                {
                    Vector3 cellPosition = new Vector3(i + _x * SIZE_X + .5f, k + _y * SIZE_Y + .5f, j + _z * SIZE_Z + .5f);

                    int firstCellVertex = 24 * (j + 8 * i + 64 * k);


                    if (World.IsThereCellInPosition(cellPosition)) { 
                        for (int f = 0; f < 6; ++f) {
                            if (MustPaintFace(f, i, k, j)){
                                int bottomLeft = (4 * f) + firstCellVertex;   // 1
                                int bottomRight = bottomLeft + 1; // 2
                                int upperLeft = bottomRight + 1;    // 3
                                int upperRight = upperLeft + 1;   // 4

                                // 1, 3, 2
                                triangles.Add(bottomLeft);
                                triangles.Add(upperLeft);
                                triangles.Add(bottomRight);

                                // 3, 4, 2
                                triangles.Add(upperLeft);
                                triangles.Add(upperRight);
                                triangles.Add(bottomRight);
                            }
                        }
                    }
                }
            }
        }

        return triangles.ToArray();
    }

    private bool MustPaintFace(int face, int x, int y, int z) {
        bool result = true;
        
        switch (face)
        {
            case 0: result = !World.IsCellActive(_x, _y, _z, x, y - 1, z); break;   // Down
            case 1: result = !World.IsCellActive(_x, _y, _z, x, y + 1, z); break;   // Up
            case 2: result = !World.IsCellActive(_x, _y, _z, x, y, z - 1); break;   // Left
            case 3: result = !World.IsCellActive(_x, _y, _z, x, y, z + 1); break;   // Right
            case 4: result = !World.IsCellActive(_x, _y, _z, x - 1, y, z); break;   // Front
            case 5: result = !World.IsCellActive(_x, _y, _z, x + 1, y, z); break;   // Back
            default: break;
        }
        
        return result;
    }

    private Vector2[] GenerateUV()
    {
        List<Vector2> uvs = new List<Vector2>();
        int N_CELLTYPES = 4;
        for (int k = 0; k < SIZE_Y; ++k)
        {
            for (int i = 0; i < SIZE_X; ++i)
            {
                for (int j = 0; j < SIZE_Z; ++j)
                {
                    float rowBottom = (float)_cellTypes[i, k, j] / N_CELLTYPES;
                    float step = 1f / N_CELLTYPES;

                    uvs.Add(new Vector2(1 / 3f, rowBottom));
                    uvs.Add(new Vector2(2 / 3f, rowBottom));
                    uvs.Add(new Vector2(1 / 3f, rowBottom + step));
                    uvs.Add(new Vector2(2 / 3f, rowBottom + step));

                    uvs.Add(new Vector2(0, rowBottom));
                    uvs.Add(new Vector2(1 / 3f, rowBottom));
                    uvs.Add(new Vector2(0, rowBottom + step));
                    uvs.Add(new Vector2(1 / 3f, rowBottom + step));

                    uvs.Add(new Vector2(2 / 3f, rowBottom));
                    uvs.Add(new Vector2(1, rowBottom));
                    uvs.Add(new Vector2(2 / 3f, rowBottom + step));
                    uvs.Add(new Vector2(1, rowBottom + step));

                    uvs.Add(new Vector2(2 / 3f, rowBottom));
                    uvs.Add(new Vector2(1, rowBottom));
                    uvs.Add(new Vector2(2 / 3f, rowBottom + step));
                    uvs.Add(new Vector2(1, rowBottom + step));

                    uvs.Add(new Vector2(2 / 3f, rowBottom));
                    uvs.Add(new Vector2(1, rowBottom));
                    uvs.Add(new Vector2(2 / 3f, rowBottom + step));
                    uvs.Add(new Vector2(1, rowBottom + step));

                    uvs.Add(new Vector2(2 / 3f, rowBottom));
                    uvs.Add(new Vector2(1, rowBottom));
                    uvs.Add(new Vector2(2 / 3f, rowBottom + step));
                    uvs.Add(new Vector2(1, rowBottom + step));
                }
            }
        }


        //Debug.Log("Vertices: " + _mesh.vertices.Length + " || UVs: " + uvs.Count);

        return uvs.ToArray();
    }
    #endregion


}
