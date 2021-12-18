using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World
{
    public static World s;

    private static int N_CHUNKS_X = 20;
    private static int N_CHUNKS_Y = 1;
    private static int N_CHUNKS_Z = 20;


    public static Chunk[,,] _map;
    private float _seed;

    public GameObject go { get { return _go;  } }
    private GameObject _go;

    public GameObject[] ChunkLayers { get { return chunkLayers; } }
    private GameObject[] chunkLayers;

    public World() {
        s = this;
        _seed = UnityEngine.Random.Range(0.0f, 1000.0f);
        _go = new GameObject("World");
        chunkLayers = new GameObject[N_CHUNKS_Y];
        for (int i = 0; i < N_CHUNKS_Y; ++i)
            chunkLayers[i] = new GameObject("ChunkLayer"+i);
        Generate();
    }

    public World(Chunk[,,] chunks) 
    {
        _map = chunks;    
    }



    private void Generate() 
    {
        _map = new Chunk[N_CHUNKS_X, N_CHUNKS_Y, N_CHUNKS_Z];
        for (int k = 0; k < N_CHUNKS_Y; ++k)
        {
            for (int i = 0; i < N_CHUNKS_X; ++i)
            {
                for (int j = 0; j < N_CHUNKS_Z; ++j)
                {
                    _map[i, k, j] = new Chunk(_seed, i, k, j);
                    //_map[i, 0, j].CreateMesh();
                }
            }
        }

        for (int k = 0; k < N_CHUNKS_Y; ++k)
        {
            for (int i = 0; i < N_CHUNKS_X; ++i)
            {
                for (int j = 0; j < N_CHUNKS_Z; ++j)
                {
                    //_map[i, 0, j] = new Chunk(_seed, i, j);
                    _map[i, k, j].CreateMesh();
                }
            }
        }
    }

    #region Static Methods
    public static Chunk GetChunkFromPosition(Vector3 p)
    {
        int cx = ((int)p.x) / Chunk.SIZE_X;
        int cy = ((int)p.y) / Chunk.SIZE_Y;
        int cz = ((int)p.z) / Chunk.SIZE_Z;

        return _map[cx, cy, cz];
    }

    public static bool IsThereCellInPosition(Vector3 p)
    {
        if (IsInLimits(p))
        {
            Chunk c = GetChunkFromPosition(p);

            int xInChunk = ((int)p.x) % Chunk.SIZE_X;
            int yInChunk = ((int)p.y) % Chunk.SIZE_Y;
            int zInChunk = ((int)p.z) % Chunk.SIZE_Z;

            try
            {
                return c.GetCellFlag(xInChunk, yInChunk, zInChunk);
            }
            catch (NullReferenceException e)
            {
                UnityEngine.Debug.LogError("Failed to access cell flag");
                return false;
            }
        }
        else return false;
    }

    public static CELL_TYPE GetCELL_TYPEInPosition(Vector3 p)
    {
        if (IsInLimits(p))
        {
            Chunk c = GetChunkFromPosition(p);

            int xInChunk = ((int)p.x) % Chunk.SIZE_X;
            int yInChunk = ((int)p.y) % Chunk.SIZE_Y;
            int zInChunk = ((int)p.z) % Chunk.SIZE_Z;

            try
            {
                return c.GetCellType(xInChunk, yInChunk, zInChunk);
            }
            catch (NullReferenceException e)
            {
                UnityEngine.Debug.LogError("Failed to access cell flag");
                return CELL_TYPE.BOTTOM;
            }
        }
        return CELL_TYPE.BOTTOM;
    }

    public static void DestroyCellInPosition(Vector3 p) {
        Chunk c = GetChunkFromPosition(p);

        int xInChunk = ((int)p.x) % Chunk.SIZE_X;
        int yInChunk = ((int)p.y) % Chunk.SIZE_Y;
        int zInChunk = ((int)p.z) % Chunk.SIZE_Z;

        try
        {
            c.SetCellFlag(xInChunk, yInChunk, zInChunk, false);
            c.RegenerateMesh();
        }
        catch (NullReferenceException e)
        {
            UnityEngine.Debug.LogError("Failed to access cell flag");
        }

        Vector3[] chunksToUpdate = new Vector3[2] { c.ChunkCoords, c.ChunkCoords };
        
        if (xInChunk == 0)
            chunksToUpdate[0] -= Vector3.right;
        else if(xInChunk == Chunk.SIZE_X - 1)
            chunksToUpdate[0] += Vector3.right;
        if (zInChunk == 0)
            chunksToUpdate[1] -= Vector3.forward;
        else if (zInChunk == Chunk.SIZE_Z - 1)
            chunksToUpdate[1] += Vector3.forward;

        try
        {
            for (int i = 0; i < chunksToUpdate.Length; ++i)
            {
                if (chunksToUpdate[i] != c.ChunkCoords)
                    _map[(int)chunksToUpdate[i].x, 0, (int)chunksToUpdate[i].y].RegenerateMesh();
            }
            
        }
        catch (IndexOutOfRangeException e) {
            Debug.LogWarning("Se eliminó una celda al borde del mundo.");
        }
    }

    public static void PutCellInPosition(Vector3 p)
    {
        Chunk c = GetChunkFromPosition(p);
        int xInChunk = ((int)p.x) % Chunk.SIZE_X;
        int yInChunk = ((int)p.y) % Chunk.SIZE_Y;
        int zInChunk = ((int)p.z) % Chunk.SIZE_Z;

        try
        {
            if (Inventory.s.AccessibleObjects[Inventory.s.CurrentObjectIndex].currentStack > 0) {
                c.SetCellFlag(xInChunk, yInChunk, zInChunk, true);
                c.SetCellType(xInChunk, yInChunk, zInChunk, Inventory.s.GetCurrentObjectCellType());
                c.RegenerateMesh();
                Inventory.s.BlockPlaced();
            }
        }
        catch (NullReferenceException e)
        {
            UnityEngine.Debug.LogError("Failed to access cell flag");
        }

    }

    public static bool IsInLimits(Vector3 pos) 
    {
        if (pos.x >= 0 && pos.x < N_CHUNKS_X * Chunk.SIZE_X &&
                pos.y >= 0 && pos.y <= N_CHUNKS_Y * Chunk.SIZE_Y &&
                pos.z >= 0 && pos.z < N_CHUNKS_Z * Chunk.SIZE_Z)
            return true;
        else return false;
    }

    public static bool IsCellActive(int chunkX, int chunkY, int chunkZ, int cellX, int cellY, int cellZ) {
        if (cellX < 0) { chunkX--; cellX = Chunk.SIZE_X-1; }
        if (cellX >= Chunk.SIZE_X) { chunkX++; cellX = 0; }

        if (cellY < 0) { chunkY--; cellY = Chunk.SIZE_Y - 1; }
        if (cellY >= Chunk.SIZE_Y) { chunkY++; cellY = 0; }

        if (cellZ < 0) { chunkZ--; cellZ = Chunk.SIZE_Z - 1; }
        if (cellZ >= Chunk.SIZE_Z) { chunkZ++; cellZ = 0; }

        try {
            return _map[chunkX, chunkY, chunkZ].GetCellFlag(cellX, cellY, cellZ);
        }
        catch (IndexOutOfRangeException e) 
        {
            return false;
        }
        
    }

    #endregion

}
