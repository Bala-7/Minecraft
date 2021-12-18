using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    // Chunk class. This class will manage how the chunks are stored, drawn and updated
    public class Chunk
    {
        private GameObject _go;
        public Cell[,,] _cells;
        public byte[,,] _cellTypes;
        public long[] _cellFlags;

        public Chunk(GameObject go, Cell[,,] cells, long[] cellFlags, byte[,,] cellTypes)
        {
            _go = go;
            _cells = cells;
            _cellFlags = cellFlags;
            _cellTypes = cellTypes;
        }

        public void Draw()
        {
            for (int i = 0; i < s.CHUNK_SIZE_X; ++i)
            {
                for (int j = 0; j < s.CHUNK_SIZE_Y; ++j)
                {
                    for (int k = 0; k < s.CHUNK_SIZE_Z; ++k)
                    {
                        bool isThereCell = (_cellFlags[j] & (1 << (k * 8 + i))) != 0;

                        if (isThereCell)
                        {
                            Vector3 cellPos = _go.transform.position + new Vector3(i, j, k);

                            for (int f = 0; f < 6; ++f)
                            {

                                Vector3 neighCellPos = cellPos + s.GetMeshNormal(f);
                                bool cellInTarget = s.IsThereCellInPosition(neighCellPos);

                                if (!cellInTarget)
                                {
                                    //Cell c = Instantiate(s.cube, cellPos, Quaternion.identity, _go.transform);
                                    Cell c = s._poolDictionary[_cellTypes[i, j, k]].Dequeue();
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
            }
        }

        public void Hide()
        {
            for (int i = 0; i < s.CHUNK_SIZE_X; ++i)
            {
                for (int j = 0; j < s.CHUNK_SIZE_Y; ++j)
                {
                    for (int k = 0; k < s.CHUNK_SIZE_Z; ++k)
                    {
                        bool isThereCell = (_cellFlags[j] & (1 << (k * 8 + i))) != 0;
                        if (isThereCell)
                        {
                            try
                            {
                                Cell c = _cells[i, j, k];
                                _cells[i, j, k] = null;
                                c.Hide();
                                s._poolDictionary[c.type].Enqueue(c);
                            }
                            catch (Exception e)
                            {
                                // Inner cells of the chunk are not instatiated, even though its _cellFlag is true, so I catch this exception to ensure the chunk is destroyed properly
                            }
                        }
                    }
                }
            }
        }

        public void SetCellFlag(int x, int y, int z, bool value)
        {
            int bitPosition = z * 8 + x;
            if (value)
            {
                _cellFlags[y] |= (1 << bitPosition);
            }
            else
            {
                _cellFlags[y] &= ~(1 << bitPosition);
            }
        }

        public bool GetCellFlag(int x, int y, int z)
        {
            int bitPosition = z * 8 + x;

            return (_cellFlags[y] & (1 << bitPosition)) != 0;
        }
    }

    public List<Cell> cubes;    // Available cubes
    public static WorldGenerator s; // Singleton
    private List<Vector3> _meshNormals; // List of vectors pointing at the six directions of cubes faces

    private Dictionary<int, Queue<Cell>> _poolDictionary;   // Pools of cubes. For each cube type there will be a pool to take cube instances from


    #region World generation parameters
    [Range(1, 10)]
    public int PATCH_SIZE_X;
    [Range(1, 10)]
    public int PATCH_SIZE_Y;
    [Range(1, 10)]
    public int PATCH_SIZE_Z;


    [Range(1, 64)]
    public int CHUNK_SIZE_X;
    [Range(1, 64)]
    public int CHUNK_SIZE_Y;
    [Range(1, 64)]
    public int CHUNK_SIZE_Z;

    [Range(1, 20)]
    public int N_CHUNKS_X;

    private int N_CHUNKS_Y = 1;
    [Range(1, 20)]
    public int N_CHUNKS_Z;
    [Range(1, 5)]
    public int DRAW_CHUNK_DISTANCE;

    public AnimationCurve heightDensityCurve;

    private Chunk[,,] _map;
    private float _seed;

    [Range(0.0f, 1.0f)]
    private float chunkDensity;
    private float noiseScale = 0.1f;
    #endregion

    #region World drawing
    private int _xDrawLimitLow, _xDrawLimitHigh;
    private int _zDrawLimitLow, _zDrawLimitHigh;

    private int _prevPlayerChunkX;
    private int _prevPlayerChunkZ;
    #endregion

    public long SetBitTo1(long value, int position)
    {
        // Set a bit at position to 1.
        return value |= (1 << position);
    }

    public long SetBitTo0(long value, int position)
    {
        // Set a bit at position to 0.
        return value & ~(1 << position);
    }

    public bool IsBitSetTo1(long value, int position)
    {
        // Return whether bit at position is set to 1.
        return (value & (1 << position)) != 0;
    }

    private void Awake()
    {
        s = this;
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 30;

        _map = new Chunk[N_CHUNKS_X, 1, N_CHUNKS_Z];
        _seed = UnityEngine.Random.Range(1, 100000);

        #region Mesh Normals init
        _meshNormals = new List<Vector3>();
        _meshNormals.Add(-Vector3.forward);
        _meshNormals.Add(Vector3.forward);
        _meshNormals.Add(-Vector3.right);
        _meshNormals.Add(Vector3.right);
        _meshNormals.Add(Vector3.up);
        _meshNormals.Add(-Vector3.up);
        #endregion
    }

    public Vector3 GetMeshNormal(int index) { return _meshNormals[index]; }

    // Start is called before the first frame update
    void Start()
    {
        GenerateCellPool();
        GenerateWorld();
        SpawnPlayer();
    }

    void SpawnPlayer() {
        float centerX = (CHUNK_SIZE_X * N_CHUNKS_X) / 2.0f;
        float centerZ = (CHUNK_SIZE_Z * N_CHUNKS_Z) / 2.0f;

        FPS_Player.s.MoveToPosition(new Vector3(centerX, CHUNK_SIZE_Y + 4, centerZ));

        int playerChunkX = N_CHUNKS_X / 2;
        int playerChunkZ = N_CHUNKS_Z / 2;

        
        _xDrawLimitLow = Mathf.Max(0, playerChunkX - DRAW_CHUNK_DISTANCE);
        _xDrawLimitHigh = Mathf.Min(N_CHUNKS_X, playerChunkX + DRAW_CHUNK_DISTANCE);

        _zDrawLimitLow = Mathf.Max(0, playerChunkZ - DRAW_CHUNK_DISTANCE);
        _zDrawLimitHigh = Mathf.Min(N_CHUNKS_Z, playerChunkZ + DRAW_CHUNK_DISTANCE);


        for (int i = _xDrawLimitLow; i <= _xDrawLimitHigh; ++i) {
            for (int j = _zDrawLimitLow; j <= _zDrawLimitHigh; ++j)
            {
                //UnityEngine.Debug.Log("Drawing chunck " + i + "_" + j);
                
                _map[i, 0, j].Draw();
            }
        }

        _prevPlayerChunkX = playerChunkX;
        _prevPlayerChunkZ = playerChunkZ;

        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pp = FPS_Player.s.GetWorldPosition();
        int currentChunkX = (int)(pp.x) / CHUNK_SIZE_X;
        int currentChunkZ = (int)(pp.z) / CHUNK_SIZE_Z;


        int movedX = currentChunkX - _prevPlayerChunkX;
        int movedZ = currentChunkZ - _prevPlayerChunkZ;
        if (movedX != 0)
        {
            /*UnityEngine.Debug.Log("Player in chunk " + currentChunkX + " - " + currentChunkZ);
            UnityEngine.Debug.Log("Draw limits " + _xDrawLimitLow + " - " + _xDrawLimitHigh + " | " + _zDrawLimitLow + " - " + _zDrawLimitHigh);
            UnityEngine.Debug.Log(movedX);
            UnityEngine.Debug.Log("---------------------------");
            */
            int zLoopLimitA = Mathf.Max(0, currentChunkZ - DRAW_CHUNK_DISTANCE);
            int zLoopLimitB = Mathf.Min(N_CHUNKS_Z - 1, currentChunkZ + DRAW_CHUNK_DISTANCE);

            if (movedX < 0)
            {
                
                _xDrawLimitLow = Mathf.Max(0, _xDrawLimitLow - 1);
                for (int f = zLoopLimitA; f <= zLoopLimitB; ++f) {
                    _map[_xDrawLimitLow, 0, f].Draw();

                    if (_xDrawLimitHigh > 2 * DRAW_CHUNK_DISTANCE + 1) _map[_xDrawLimitHigh, 0, f].Hide();
                }
                if(_xDrawLimitHigh > 2 * DRAW_CHUNK_DISTANCE + 1) _xDrawLimitHigh--;
                //_xDrawLimitHigh = Mathf.Max(_xDrawLimitLow + 2*DRAW_CHUNK_DISTANCE + 1, _xDrawLimitHigh - 1);
            }
            else
            {
                _xDrawLimitHigh = Mathf.Min(N_CHUNKS_X-1, _xDrawLimitHigh + 1);
                for (int f = zLoopLimitA; f <= zLoopLimitB; ++f)
                {
                    _map[_xDrawLimitHigh, 0, f].Draw();

                    if (_xDrawLimitLow < N_CHUNKS_X - 2 * DRAW_CHUNK_DISTANCE + 1) _map[_xDrawLimitLow, 0, f].Hide();
                }
                if (_xDrawLimitLow < N_CHUNKS_X - 2 * DRAW_CHUNK_DISTANCE + 1) _xDrawLimitLow++;
            }
            
        }
        _prevPlayerChunkX = currentChunkX;

        if (movedZ != 0) {
            //UnityEngine.Debug.Log("Player in chunk " + currentChunkX + " - " + currentChunkZ);
            //UnityEngine.Debug.Log("Draw limits " + _xDrawLimitLow + " - " + _xDrawLimitHigh + " | " + _zDrawLimitLow + " - " + _zDrawLimitHigh);
            //UnityEngine.Debug.Log("---------------------------");

            int xLoopLimitA = Mathf.Max(0, currentChunkX - DRAW_CHUNK_DISTANCE);
            int xLoopLimitB = Mathf.Min(N_CHUNKS_X - 1, currentChunkX + DRAW_CHUNK_DISTANCE);

            if (movedZ < 0)
            {

                _zDrawLimitLow = Mathf.Max(0, _zDrawLimitLow - 1);
                for (int f = xLoopLimitA; f <= xLoopLimitB; ++f)
                {
                    _map[f , 0 ,_zDrawLimitLow].Draw();

                    if (_zDrawLimitHigh > 2 * DRAW_CHUNK_DISTANCE + 1) _map[f, 0, _zDrawLimitHigh].Hide();
                }
                if (_zDrawLimitHigh > 2 * DRAW_CHUNK_DISTANCE + 1) _zDrawLimitHigh--;
            }
            else
            {
                _zDrawLimitHigh = Mathf.Min(N_CHUNKS_Z - 1, _zDrawLimitHigh + 1);
                for (int f = xLoopLimitA; f <= xLoopLimitB; ++f)
                {
                    _map[f,0,_zDrawLimitHigh].Draw();

                    if (_zDrawLimitLow < (N_CHUNKS_Z-1) - 2 * DRAW_CHUNK_DISTANCE) _map[f,0,_zDrawLimitLow].Hide();
                }
                if (_zDrawLimitLow < (N_CHUNKS_Z-1) - 2 * DRAW_CHUNK_DISTANCE) _zDrawLimitLow++;
            }
        }
        _prevPlayerChunkZ = currentChunkZ;
    }


    private void GenerateWorld() {
        _seed = UnityEngine.Random.Range(0.0f, 1000.0f);
        for (int i = 0; i < N_CHUNKS_X; ++i) {
            for (int j = 0; j < N_CHUNKS_Z; ++j) {
                GenerateChunk(i, j);
            }
        }
    }

    private void GenerateChunk(int cx, int cz) {
        GameObject chunkGO = new GameObject("Chunk_" + cx.ToString() + "_" + cz.ToString());
        chunkGO.transform.position = new Vector3(cx * CHUNK_SIZE_X, 0, cz * CHUNK_SIZE_Z);

        //Chunk chunk = new Chunk(chunkGO, new Cell[CHUNK_SIZE_X, CHUNK_SIZE_Y, CHUNK_SIZE_Z], new bool[CHUNK_SIZE_X, CHUNK_SIZE_Y, CHUNK_SIZE_Z], new int[CHUNK_SIZE_X, CHUNK_SIZE_Y, CHUNK_SIZE_Z]);
        Chunk chunk = new Chunk(chunkGO, new Cell[CHUNK_SIZE_X, CHUNK_SIZE_Y, CHUNK_SIZE_Z], new long[CHUNK_SIZE_Y], new byte[CHUNK_SIZE_X, CHUNK_SIZE_Y, CHUNK_SIZE_Z]);
        int N_PATCHES_X = CHUNK_SIZE_X / PATCH_SIZE_X;
        int N_PATCHES_Y = CHUNK_SIZE_Y / PATCH_SIZE_Y;
        int N_PATCHES_Z = CHUNK_SIZE_Z / PATCH_SIZE_Z;


        for (int x = 0; x < N_PATCHES_X; ++x)
        {
            for (int y = 0; y < N_PATCHES_Y; ++y)
            {
                for (int z = 0; z < N_PATCHES_Z; ++z)
                {
                    float noiseValue = (float)NoiseS3D.NoiseCombinedOctaves(_seed + (cx * CHUNK_SIZE_X + x * PATCH_SIZE_X) * noiseScale, _seed + (y * PATCH_SIZE_Y) * noiseScale, _seed + (cz * CHUNK_SIZE_Z + z * PATCH_SIZE_Z) * noiseScale);
                    // Remap the value to 0 - 1 
                    noiseValue = (noiseValue + 1) * 0.5f;
                    
                    float heightPerc = (float)y / (float)N_PATCHES_Y;
                    float density = heightDensityCurve.Evaluate(heightPerc);
                    if (noiseValue < density) {
                        chunk = GeneratePatch(chunkGO, chunk, x, y, z);
                    }

                }
            }
        }



        //chunk.Hide();

        _map[cx, 0, cz] = chunk;
    }

    private Chunk GeneratePatch(GameObject chunkGO, Chunk chunk, int px, int py, int pz) {
        //UnityEngine.Debug.Log("Generating patch " + px + "_" + py + "_" + pz);
        GameObject patchGO = new GameObject("Patch_" + px + "_" + py + "_" + pz);
        patchGO.transform.parent = chunkGO.transform;

        for (int x = 0; x < PATCH_SIZE_X; ++x)
        {
            for (int y = 0; y < PATCH_SIZE_Y; ++y)
            {
                for (int z = 0; z < PATCH_SIZE_Z; ++z)
                {
                    int nBit = (pz * PATCH_SIZE_Z + z) * 8 + (px * PATCH_SIZE_X + x);
                    
                    chunk.SetCellFlag(px * PATCH_SIZE_X + x, py * PATCH_SIZE_Y + y, pz * PATCH_SIZE_Z + z, true);
                    chunk._cellTypes[px * PATCH_SIZE_X + x, py * PATCH_SIZE_Y + y, pz * PATCH_SIZE_Z + z] = AssignCELL_TYPE(py, y);
                }
            }
        }

        return chunk;
    }

    private byte AssignCELL_TYPE(int patchY, int cellY) {
        int N_PATCHES_Y = CHUNK_SIZE_Y / PATCH_SIZE_Y;

        if (patchY == N_PATCHES_Y - 1 && cellY == PATCH_SIZE_Y - 1) return 0;
        else if (patchY == 0 && cellY == 0) return 2;    // Limit cell in the bottom
        else return 1;
    
    }

    public Chunk GetChunkFromPosition(Vector3 p) {
        int cx = ((int)p.x) / CHUNK_SIZE_X;
        int cz = ((int)p.z) / CHUNK_SIZE_Z;

        return _map[cx, 0, cz];
    }

    public bool IsThereCellInPosition(Vector3 p) {

        if (IsInLimits(p))
        {
            Chunk c = GetChunkFromPosition(p);

            int xInChunk = ((int)p.x) % CHUNK_SIZE_X;
            int yInChunk = ((int)p.y) % CHUNK_SIZE_Y;
            int zInChunk = ((int)p.z) % CHUNK_SIZE_Z;



            try
            {
                return c.GetCellFlag(xInChunk, yInChunk, zInChunk);
            }
            catch (NullReferenceException e) {
                UnityEngine.Debug.LogError("Failed to access cell flag");
                return false;
            }
        }
        else return false;
    }

    public Cell GetCellInPosition(Vector3 p) {
        if (IsInLimits(p))
        {
            Chunk c = GetChunkFromPosition(p);

            int xInChunk = ((int)p.x) % CHUNK_SIZE_X;
            int yInChunk = ((int)p.y) % CHUNK_SIZE_Y;
            int zInChunk = ((int)p.z) % CHUNK_SIZE_Z;



            try
            {
                if (c._cells[xInChunk, yInChunk, zInChunk] == null) {
                    if (_poolDictionary[1].Count > 0)
                    {
                        Cell cellToPlace = _poolDictionary[1].Dequeue();
                        cellToPlace.transform.position = p;
                        c._cells[xInChunk, yInChunk, zInChunk] = cellToPlace;
                    }
                    else UnityEngine.Debug.Log("EMPTY POOL.");
                }
                return c._cells[xInChunk, yInChunk, zInChunk];
            }
            catch (NullReferenceException e)
            {
                UnityEngine.Debug.LogError("Failed to access cell flag");
                return null;
            }
        }
        else return null;
    }

    public void DestroyCellInPosition(Vector3 p) {
        if (IsInLimits(p))
        {
            Chunk c = GetChunkFromPosition(p);

            int xInChunk = ((int)p.x) % CHUNK_SIZE_X;
            int yInChunk = ((int)p.y) % CHUNK_SIZE_Y;
            int zInChunk = ((int)p.z) % CHUNK_SIZE_Z;



            try
            {
                c.SetCellFlag(xInChunk, yInChunk, zInChunk, false);
                
            }
            catch (NullReferenceException e)
            {
                UnityEngine.Debug.LogError("Failed to destroy cell");
                
            }
        }
    }

    public bool IsInLimits(Vector3 pos) {


        if (pos.x >= 0 && pos.x < N_CHUNKS_X * CHUNK_SIZE_X &&
            pos.y >= 0 && pos.y < N_CHUNKS_Y * CHUNK_SIZE_Y &&
            pos.z >= 0 && pos.z < N_CHUNKS_Z * CHUNK_SIZE_Z)
            return true;
        else return false;

    }

    private void GenerateCellPool() {

        GameObject cellPoolGO = new GameObject("CellPool");
        Queue<Cell> _grassPool = new Queue<Cell>();
        Queue<Cell> _groundPool = new Queue<Cell>();
        Queue<Cell> _limitPool = new Queue<Cell>();

        // Generate grass cubes pool
        int howManyGrass = (int)Mathf.Pow(2 * DRAW_CHUNK_DISTANCE + 1, 2) * CHUNK_SIZE_X * 2 * CHUNK_SIZE_Z;
        for (int i = 0; i < howManyGrass; ++i)
        {
            Cell c = Instantiate(cubes[0], Vector3.zero, Quaternion.identity, cellPoolGO.transform);
            c.GetComponent<BoxCollider>().enabled = false;

            _grassPool.Enqueue(c);
        }

        // Generates ground cubes pool
        int howManyGround = (int) Mathf.Pow(2*DRAW_CHUNK_DISTANCE + 1, 2) * CHUNK_SIZE_X * CHUNK_SIZE_Y * CHUNK_SIZE_Z;
        for (int i = 0; i < howManyGround; ++i) {
            Cell c = Instantiate(cubes[1], Vector3.zero, Quaternion.identity, cellPoolGO.transform);
            c.GetComponent<BoxCollider>().enabled = false;

            _groundPool.Enqueue(c);
        }


        int howManyLimit = (int)Mathf.Pow(2 * DRAW_CHUNK_DISTANCE + 1, 2) * CHUNK_SIZE_X * 2 * CHUNK_SIZE_Z;
        for (int i = 0; i < howManyLimit; ++i)
        {
            Cell c = Instantiate(cubes[2], Vector3.zero, Quaternion.identity, cellPoolGO.transform);
            c.GetComponent<BoxCollider>().enabled = false;

            _limitPool.Enqueue(c);
        }
        
        _poolDictionary = new Dictionary<int, Queue<Cell>>();
        _poolDictionary.Add(0, _grassPool);
        _poolDictionary.Add(1, _groundPool);
        _poolDictionary.Add(2, _limitPool);


    }

    public void EnqueueCell(Cell c) { 
        c.Hide();
        c.transform.position = Vector3.zero;
        _poolDictionary[c.type].Enqueue(c);
    }

}
