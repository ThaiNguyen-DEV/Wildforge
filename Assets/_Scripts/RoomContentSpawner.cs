using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Cinemachine;

public class RoomContentSpawner : MonoBehaviour
{
    [SerializeField]
    private RoomFirstDungeonGenerator dungeonGenerator;

    [Header("Camera")]
    [SerializeField]
    private CinemachineCamera virtualCamera; // assign in inspector

    [Header("Prefabs")]
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private GameObject meleeEnemyPrefab; // Renamed for clarity
    [SerializeField]
    private GameObject rangedEnemyPrefab; // Added ranged enemy
    [SerializeField]
    private GameObject endLadderPrefab;
    [SerializeField]
    private GameObject torchPrefab;

    [Header("Static room content")]
    [SerializeField]
    private GameObject[] staticPrefabs;
    [SerializeField]
    private int minStaticPerRoom = 0;
    [SerializeField]
    private int maxStaticPerRoom = 2;
    [SerializeField]
    private bool allowDuplicateStaticInRoom = true;

    [Header("Enemies")]
    [SerializeField]
    private int minEnemiesPerRoom = 1;
    [SerializeField]
    private int maxEnemiesPerRoom = 1;
    [SerializeField, Range(0, 1)]
    private float rangedEnemySpawnChance = 0.25f; // Chance to spawn a ranged enemy

    [Header("Torches")]
    [SerializeField, Range(0, 1)]
    private float torchSpawnChance = 0.1f;

    [Header("Placement")]
    [SerializeField]
    private Vector2 worldOffset = new Vector2(0.5f, 0.5f);
    [SerializeField]
    private bool useRoomTilesForPlacement = true;

    [SerializeField]
    private Transform contentParent;
    [SerializeField]
    private bool clearPreviousOnRespawn = true;

    private bool subscribed;
    private Transform playerTransform;
    private List<RoomFirstDungeonGenerator.RoomFloorData> lastRooms;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (dungeonGenerator != null)
        {
            dungeonGenerator.OnRoomCentersFinalized -= HandleCenters;
            dungeonGenerator.OnRoomFloorsFinalized -= HandleRoomsWithFloors;
            dungeonGenerator.OnTorchPositionsFinalized -= HandleTorches;
            subscribed = false;
        }
    }

    private void TrySubscribe()
    {
        if (subscribed) return;
        if (dungeonGenerator == null)
        {
            Debug.LogWarning("[RoomContentSpawner] DungeonGenerator reference not set.");
            return;
        }
        dungeonGenerator.OnRoomCentersFinalized += HandleCenters;
        dungeonGenerator.OnRoomFloorsFinalized += HandleRoomsWithFloors;
        dungeonGenerator.OnTorchPositionsFinalized += HandleTorches;
        subscribed = true;
    }

    private void HandleTorches(IReadOnlyList<Vector2Int> torchPositions)
    {
        if (torchPrefab == null) return;

        foreach (var position in torchPositions)
        {
            if (Random.value < torchSpawnChance)
            {
                Vector3 worldPos = new Vector3(position.x + worldOffset.x, position.y + worldOffset.y, 0f);
                InstantiateAt(torchPrefab, worldPos, Quaternion.identity);
            }
        }
    }

    private void HandleCenters(IReadOnlyList<Vector2Int> centers) { }

    private void HandleRoomsWithFloors(IReadOnlyList<RoomFirstDungeonGenerator.RoomFloorData> rooms)
    {
        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogWarning("[RoomContentSpawner] No room floor data available.");
            return;
        }

        lastRooms = new List<RoomFirstDungeonGenerator.RoomFloorData>(rooms);

        if (clearPreviousOnRespawn && contentParent != null)
        {
            for (int i = contentParent.childCount - 1; i >= 0; i--)
            {
                Destroy(contentParent.GetChild(i).gameObject);
            }
            playerTransform = null;
        }

        int playerIndex = Random.Range(0, lastRooms.Count);
        var playerRoom = lastRooms[playerIndex];
        Vector2Int endCenter = dungeonGenerator.FindFarthestPointTo(playerRoom.Center, GetCenters(lastRooms));
        var endRoom = lastRooms.Find(r => r.Center == endCenter);

        List<RoomFirstDungeonGenerator.RoomFloorData> otherRooms = new List<RoomFirstDungeonGenerator.RoomFloorData>();
        foreach (var r in lastRooms)
        {
            if (r.Center == playerRoom.Center || r.Center == endRoom.Center) continue;
            otherRooms.Add(r);
        }

        SpawnPlayerAtRoom(playerRoom);
        SpawnEndLadderAtRoom(endRoom);

        foreach (var r in lastRooms)
        {
            SpawnStaticInRoomTiles(r);
        }

        SpawnEnemiesInRoomTiles(otherRooms);
    }

    private IReadOnlyList<Vector2Int> GetCenters(List<RoomFirstDungeonGenerator.RoomFloorData> rooms)
    {
        List<Vector2Int> centers = new List<Vector2Int>(rooms.Count);
        foreach (var r in rooms) centers.Add(r.Center);
        return centers;
    }

    private void SpawnPlayerAtRoom(RoomFirstDungeonGenerator.RoomFloorData room)
    {
        if (playerPrefab == null) return;

        Vector3 pos = RoomTileOrCenter(room);
        var instance = InstantiateAt(playerPrefab, pos, Quaternion.identity);
        playerTransform = instance.transform;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayer(instance.GetComponent<Health>());
        }

        EnsureCameraFollowsPlayer();
    }

    private void EnsureCameraFollowsPlayer()
    {
        if (playerTransform == null) return;

        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineCamera>();
        }

        if (virtualCamera != null)
        {
            virtualCamera.Follow = playerTransform;
            virtualCamera.LookAt = playerTransform;
        }
        else
        {
            Debug.LogWarning("[RoomContentSpawner] CinemachineVirtualCamera not found/assigned.");
        }
    }

    private void SpawnEndLadderAtRoom(RoomFirstDungeonGenerator.RoomFloorData room)
    {
        if (endLadderPrefab == null) return;

        Vector3 pos = GetTrueRoomCenterPosition(room);
        InstantiateAt(endLadderPrefab, pos, Quaternion.identity);
    }

    private Vector3 GetTrueRoomCenterPosition(RoomFirstDungeonGenerator.RoomFloorData room)
    {
        if (room.FloorTiles != null && room.FloorTiles.Count > 0)
        {
            Vector2 sum = Vector2.zero;
            foreach (var t in room.FloorTiles) sum += t;
            Vector2 avg = sum / room.FloorTiles.Count;

            Vector2Int closest = room.FloorTiles[0];
            float bestDist = float.MaxValue;
            foreach (var t in room.FloorTiles)
            {
                float d = (avg - (Vector2)t).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    closest = t;
                }
            }
            return new Vector3(closest.x + worldOffset.x, closest.y + worldOffset.y, 0f);
        }

        return new Vector3(room.Center.x + worldOffset.x, room.Center.y + worldOffset.y, 0f);
    }

    private void SpawnStaticInRoomTiles(RoomFirstDungeonGenerator.RoomFloorData room)
    {
        if (staticPrefabs == null || staticPrefabs.Length == 0) return;

        int minCount = Mathf.Max(0, minStaticPerRoom);
        int maxCount = Mathf.Max(minCount, maxStaticPerRoom);
        int count = Random.Range(minCount, maxCount + 1);

        List<Vector2Int> tiles = room.FloorTiles;
        if (tiles == null || tiles.Count == 0) return;

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = allowDuplicateStaticInRoom
                ? staticPrefabs[Random.Range(0, staticPrefabs.Length)]
                : staticPrefabs[Mathf.Clamp(i, 0, staticPrefabs.Length - 1)];

            if (prefab == null) continue;

            Vector2Int tile = tiles[Random.Range(0, tiles.Count)];
            Vector3 worldPos = new Vector3(tile.x + worldOffset.x, tile.y + worldOffset.y, 0f);
            InstantiateAt(prefab, worldPos, Quaternion.identity);
        }
    }

    private void SpawnEnemiesInRoomTiles(List<RoomFirstDungeonGenerator.RoomFloorData> rooms)
    {
        foreach (var room in rooms)
        {
            int minCount = Mathf.Max(0, minEnemiesPerRoom);
            int maxCount = Mathf.Max(minCount, maxEnemiesPerRoom);
            int count = Random.Range(minCount, maxCount + 1);

            var tiles = room.FloorTiles;
            if (tiles == null || tiles.Count == 0) continue;

            for (int i = 0; i < count; i++)
            {
                // Decide which enemy type to spawn
                GameObject enemyPrefabToSpawn = meleeEnemyPrefab;
                if (rangedEnemyPrefab != null && Random.value < rangedEnemySpawnChance)
                {
                    enemyPrefabToSpawn = rangedEnemyPrefab;
                }

                if (enemyPrefabToSpawn == null) continue;

                Vector2Int tile = tiles[Random.Range(0, tiles.Count)];
                Vector3 worldPos = new Vector3(tile.x + worldOffset.x, tile.y + worldOffset.y, 0f);
                var enemy = InstantiateAt(enemyPrefabToSpawn, worldPos, Quaternion.identity);

                var ai = enemy.GetComponent<EnemyAI>();
                if (ai != null)
                {
                    if (playerTransform == null)
                    {
                        var found = GameObject.FindWithTag("Player");
                        playerTransform = found != null ? found.transform : null;
                    }
                    if (playerTransform != null)
                    {
                        ai.SetPlayer(playerTransform);
                    }
                }
            }
        }
    }

    private Vector3 RoomTileOrCenter(RoomFirstDungeonGenerator.RoomFloorData room)
    {
        if (useRoomTilesForPlacement && room.FloorTiles != null && room.FloorTiles.Count > 0)
        {
            Vector2Int tile = room.FloorTiles[Random.Range(0, room.FloorTiles.Count)];
            return new Vector3(tile.x + worldOffset.x, tile.y + worldOffset.y, 0f);
        }
        return new Vector3(room.Center.x + worldOffset.x, room.Center.y + worldOffset.y, 0f);
    }

    private GameObject InstantiateAt(GameObject prefab, Vector3 worldPos, Quaternion rotation)
    {
        var parent = contentParent != null ? contentParent : transform;
        var instance = Instantiate(prefab, worldPos, rotation, parent);
        instance.name = $"{prefab.name}_{Mathf.RoundToInt(worldPos.x)}_{Mathf.RoundToInt(worldPos.y)}";
        return instance;
    }
}