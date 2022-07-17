using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class World : MonoBehaviour
{
    public const int WidthInBlocks = WidthInChunks * Chunk.Size;
    public const int HeightInBlocks = HeightInChunks * Chunk.Size;
    
    private const int WidthInChunks = 8;
    private const int HeightInChunks = 4;

    private const float GroundLevel = Chunk.Size * HeightInChunks * 0.5f - 16;

    public BuffData BuffData;
    public bool HasSpawningStarted { get; private set; }
    
    private static readonly Vector3 HalfBlockPhysicalExtents = new(0.49f, 0.49f, 0.49f);
    private static readonly Vector3Int spawnPos = new Vector3Int(128, 50, 128);
    
    [SerializeField] private GameObject chunkPrefab;

    private readonly Dictionary<Vector3Int, Chunk> chunks = new();
    private readonly ConcurrentQueue<Chunk> chunksToUpdate = new();

    private readonly VoxelTextureArray textureArray = new();
    
    private void Start()
    {
        textureArray.Initialize();
        
        InitializeChunks();

        const int tileSize = 5;

        List<Vector3Int> reservedStructurePositions = new()
        {
            spawnPos, new Vector3Int(240, 50, 128),
            new Vector3Int(16, 50, 128),
            new Vector3Int(128, 50, 240),
            new Vector3Int(128, 50, 16)
        };
        ConcurrentBag<Vector3Int> structurePositions = new();
        
        foreach (Vector3Int reservedPos in reservedStructurePositions)
        {
            structurePositions.Add(reservedPos);
        }

        GenerateTerrain((pos, random) =>
        {
            const int maxHeight = (int)GroundLevel;

            if (pos.y >= maxHeight)
            {
                if (pos.y != maxHeight || random.Next(0, 800) != 0) return;
                
                bool alreadyOccupied = false;

                foreach (Vector3Int structurePosition in structurePositions)
                {
                    if (!(Vector3Int.Distance(structurePosition, pos) < 15)) continue;
                    
                    alreadyOccupied = true;
                    break;
                }
                    
                if (!alreadyOccupied) structurePositions.Add(pos);

                return;
            }
            
            Block.Id blockId = Block.Id.WhiteTile;
            
            if (pos.x / tileSize % 2 == pos.z / tileSize % 2) blockId = Block.Id.BlackTile;
            
            SetBlock(blockId, pos);
        });

        System.Random random = SharedRandom.Default;

        foreach (Vector3Int structurePosition in structurePositions)
        {
            if (reservedStructurePositions.Contains(structurePosition)) continue;
            
            Block.Id blockId = Block.Id.BlackTile;

            if (random.Next(2) == 0) blockId = Block.Id.WhiteTile;
            if (random.Next(10) == 0) blockId = Block.Id.Die;

            Structures.Structure structure = Structures.StructureArray[random.Next(Structures.StructureArray.Length)];
            structure.Spawn(this, structurePosition, blockId);
        }

        Thread meshGenThread = new(MeshGenThreadProc);
        meshGenThread.Start();
    }

    public void StartSpawning()
    {
        HasSpawningStarted = true;
    }

    private void GenerateTerrain(Action<Vector3Int, System.Random> generator)
    {
        Parallel.ForEach(chunks, pair => { pair.Value.data.Generate(generator); });
    }

    private void InitializeChunks()
    {
        const int sizeInChunks = WidthInChunks * WidthInChunks * HeightInChunks;
        for (int i = 0; i < sizeInChunks; i++)
        {
            (int x, int y, int z) = i.To3D(WidthInChunks, HeightInChunks);
            InstantiateChunk(new Vector3Int(x, y, z));
        }
    }

    private void Update()
    {
        while (chunksToUpdate.TryDequeue(out Chunk chunk)) chunk.renderer.UpdateMesh(chunk.data);
    }

    private void MeshGenThreadProc()
    {
        List<Chunk> dirtyChunks = new();
        
        while (true)
        {
            foreach ((Vector3Int _, Chunk chunk) in chunks)
            {
                if (chunk.data.IsDirty) dirtyChunks.Add(chunk);
            }

            Parallel.ForEach(dirtyChunks, chunk =>
            {
                chunk.data.GenerateMesh();
                chunksToUpdate.Enqueue(chunk);
            });
            
            dirtyChunks.Clear();
        }
    }

    public bool IsBlockPhysicallyOccupied(Vector3Int blockPos)
    {
        Vector3 checkPos = blockPos + new Vector3(0.5f, 0.5f, 0.5f);
        return GetBlock(blockPos) != Block.Id.Air || Physics.CheckBox(checkPos, HalfBlockPhysicalExtents);
    }
    
    public bool IsUnsnappedBlockPhysicallyOccupied(Vector3 blockPos, LayerMask mask, float padding = 0f)
    {
        Vector3 checkPos = blockPos + new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 extents = HalfBlockPhysicalExtents + new Vector3(padding, padding, padding);
        return Physics.CheckBox(checkPos, extents, Quaternion.identity, mask);
    }
    
    public Block.Id GetBlock(Vector3Int pos) => GetBlock(pos.x, pos.y, pos.z);
    
    public Block.Id GetBlock(int x, int y, int z)
    {
        Vector3Int chunkPos = GetChunkPos(x, y, z);
        if (!chunks.ContainsKey(chunkPos)) return Block.Id.Air;

        Vector3Int localPos = GetLocalPos(x, y, z);
        return chunks[chunkPos].data.GetBlock(localPos);
    }

    public void SetBlock(Block.Id blockId, Vector3Int pos) => SetBlock(blockId, pos.x, pos.y, pos.z);
    
    public void SetBlock(Block.Id blockId, int x, int y, int z)
    {
        Vector3Int chunkPos = GetChunkPos(x, y, z);
        if (!chunks.ContainsKey(chunkPos)) return;

        Vector3Int localPos = GetLocalPos(x, y, z);
        Chunk chunk = chunks[chunkPos];
        chunk.data.SetBlock(blockId, localPos);
        
        if (localPos.x == 0)              MarkDirty(chunkPos + Vector3Int.left);
        if (localPos.x == Chunk.Size - 1) MarkDirty(chunkPos + Vector3Int.right);
        if (localPos.y == 0)              MarkDirty(chunkPos + Vector3Int.down);
        if (localPos.y == Chunk.Size - 1) MarkDirty(chunkPos + Vector3Int.up);
        if (localPos.z == 0)              MarkDirty(chunkPos + Vector3Int.back);
        if (localPos.z == Chunk.Size - 1) MarkDirty(chunkPos + Vector3Int.forward);
    }

    // Get the position of the chunk containing this block.
    private static Vector3Int GetChunkPos(int x, int y, int z) => new()
    {
        x = x / Chunk.Size,
        y = y / Chunk.Size,
        z = z / Chunk.Size
    };
    
    // Get the location of this block inside the chunk containing it.
    private static Vector3Int GetLocalPos(int x, int y, int z) => new()
    {
        x = x % Chunk.Size,
        y = y % Chunk.Size,
        z = z % Chunk.Size
    };

    private void MarkDirty(Vector3Int chunkPos)
    {
        if (chunks.ContainsKey(chunkPos)) chunks[chunkPos].data.MarkDirty();
    }
    
    private void InstantiateChunk(Vector3Int position)
    {
        Vector3 worldPos = position * Chunk.Size;

        GameObject chunkObj = Instantiate(chunkPrefab, worldPos, Quaternion.identity);
        ChunkRenderer chunkRenderer = chunkObj.GetComponent<ChunkRenderer>();
        chunkRenderer.material = textureArray.Material;
        ChunkData chunkData = new(this, position);
        
        chunks.Add(position, new Chunk(chunkData, chunkRenderer));
    }
}
