using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class RoomFirstDungeonGenerator : RandomWalkDungeonGenerator
{
    [SerializeField]
    private int minRoomWidth = 4, minRoomHeight = 4;
    [SerializeField]
    private int dungeonWidth = 20, dungeonHeight = 20;
    [SerializeField, Range(0, 10)]
    private int offset = 1;
    [SerializeField]
    private bool randomWalkRooms = false;

    // Exposed immutable list of final room centers
    public IReadOnlyList<Vector2Int> LastRoomCenters => lastRoomCenters;
    private readonly List<Vector2Int> lastRoomCenters = new List<Vector2Int>();

    // Per-room floors data provided after generation
    public struct RoomFloorData
    {
        public Vector2Int Center;
        public List<Vector2Int> FloorTiles;
    }

    // Events for external spawning logic
    public event Action<IReadOnlyList<Vector2Int>> OnRoomCentersFinalized;
    public event Action<IReadOnlyList<RoomFloorData>> OnRoomFloorsFinalized;
    public event Action<IReadOnlyList<Vector2Int>> OnTorchPositionsFinalized;

    public override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    private void CreateRooms()
    {
        var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(
            new BoundsInt((Vector3Int)startPosition,
                new Vector3Int(dungeonWidth, dungeonHeight, 0)),
            minRoomWidth,
            minRoomHeight);

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        List<RoomFloorData> perRoomFloors = new List<RoomFloorData>();

        if (randomWalkRooms)
        {
            for (int i = 0; i < roomsList.Count; i++)
            {
                var roomBounds = roomsList[i];
                var roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
                var roomFloorWalk = RunRandomWalk(randomWalkParameters, roomCenter);

                List<Vector2Int> roomTiles = new List<Vector2Int>();
                foreach (var position in roomFloorWalk)
                {
                    if (position.x >= (roomBounds.xMin + offset) &&
                        position.x <= (roomBounds.xMax - offset) &&
                        position.y >= (roomBounds.yMin - offset) &&
                        position.y <= (roomBounds.yMax - offset))
                    {
                        floor.Add(position);
                        roomTiles.Add(position);
                    }
                }

                perRoomFloors.Add(new RoomFloorData
                {
                    Center = roomCenter,
                    FloorTiles = roomTiles
                });
            }
        }
        else
        {
            foreach (var room in roomsList)
            {
                List<Vector2Int> roomTiles = new List<Vector2Int>();
                for (int col = offset; col < room.size.x - offset; col++)
                {
                    for (int row = offset; row < room.size.y - offset; row++)
                    {
                        Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                        floor.Add(position);
                        roomTiles.Add(position);
                    }
                }

                perRoomFloors.Add(new RoomFloorData
                {
                    Center = (Vector2Int)Vector3Int.RoundToInt(room.center),
                    FloorTiles = roomTiles
                });
            }
        }

        // Centers snapshot
        lastRoomCenters.Clear();
        foreach (var room in roomsList)
        {
            lastRoomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));
        }

        // Corridors
        var centersForCorridors = new List<Vector2Int>(lastRoomCenters);
        HashSet<Vector2Int> corridors = ConnectRooms(centersForCorridors);
        floor.UnionWith(corridors);

        tilemapVisualizer.PaintFloorTiles(floor);
        var torchPositions = WallGenerator.CreateWalls(floor, tilemapVisualizer);

        // Notify listeners
        OnRoomCentersFinalized?.Invoke(LastRoomCenters);
        OnRoomFloorsFinalized?.Invoke(perRoomFloors);
        OnTorchPositionsFinalized?.Invoke(torchPositions.ToList());
    }

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        if (roomCenters.Count == 0) return corridors;

        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest;
            corridors.UnionWith(newCorridor);
        }

        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();

        var position = currentRoomCenter;
        corridor.Add(position);
        while (position.y != destination.y)
        {
            position += destination.y > position.y ? Vector2Int.up : Vector2Int.down;
            corridor.Add(position);
        }
        while (position.x != destination.x)
        {
            position += destination.x > position.x ? Vector2Int.right : Vector2Int.left;
            corridor.Add(position);
        }

        return corridor;
    }

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float length = float.MaxValue;

        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);
            if (currentDistance < length)
            {
                length = currentDistance;
                closest = position;
            }
        }

        return closest;
    }

    public Vector2Int FindFarthestPointTo(Vector2Int from, IReadOnlyList<Vector2Int> roomCenters)
    {
        Vector2Int farthest = Vector2Int.zero;
        float length = float.MinValue;
        foreach (var position in roomCenters)
        {
            if (position == from) continue;
            float currentDistance = Vector2.Distance(position, from);
            if (currentDistance > length)
            {
                length = currentDistance;
                farthest = position;
            }
        }
        return farthest;
    }

    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        for (int i = 0; i < roomsList.Count; i++)
        {
            var roomBounds = roomsList[i];
            var roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);
            foreach (var position in roomFloor)
            {
                if (position.x >= (roomBounds.xMin + offset) &&
                    position.x <= (roomBounds.xMax - offset) &&
                    position.y >= (roomBounds.yMin - offset) &&
                    position.y <= (roomBounds.yMax - offset))
                {
                    floor.Add(position);
                }
            }
        }

        return floor;
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomsList)
        {
            for (int col = offset; col < room.size.x - offset; col++)
            {
                for (int row = offset; row < room.size.y - offset; row++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    floor.Add(position);
                }
            }
        }

        return floor;
    }
}
