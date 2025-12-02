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
    private CinemachineCamera virtualCamera;

    [Header("Core Prefabs")]
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private GameObject endLadderPrefab;

    [Header("Enemy Prefabs")]
    [SerializeField]
    private GameObject meleeEnemyPrefab;
    [SerializeField]
    private GameObject rangedEnemyPrefab;

    [Header("Chest Spawning")]
    [SerializeField]
    private GameObject[] chestPrefabs;
    [SerializeField, Range(0, 1)]
    private float chestSpawnChance = 0.5f; // Chance for a chest to spawn in a room

    [Header("Other Prop Spawning (Collidable)")]
    [SerializeField]
    private GameObject[] otherPropPrefabs;
    [SerializeField]
    private int minPropsPerRoom = 0;
    [SerializeField]
    private int maxPropsPerRoom = 2;
    [SerializeField]
    private bool allowDuplicatePropsInRoom = true;

    [Header("Foliage Spawning (Non-Collidable)")]
    [SerializeField]
    private GameObject[] foliagePrefabs;
    [SerializeField]
    private int minFoliagePerRoom = 1;
    [SerializeField]
    private int maxFoliagePerRoom = 3;

    [Header("Pickup Spawning")]
    [SerializeField]
    private GameObject smallHealthPotionPrefab;
    [SerializeField, Range(0, 1)]
    private float smallHealthPotionSpawnChance = 0.1f;
    [SerializeField]
    private GameObject bigHealthPotionPrefab;
    [SerializeField, Range(0, 1)]
    private float bigHealthPotionSpawnChance = 0.05f;

    [Header("Enemy Spawning")]
    [SerializeField]
    private int minEnemiesPerRoom = 1;
    [SerializeField]
    private int maxEnemiesPerRoom = 1;
    [SerializeField, Range(0, 1)]
    private float rangedEnemySpawnChance = 0.25f;

    [Header("Torch Spawning")]
    [SerializeField]
    private GameObject torchPrefab;
    [SerializeField, Range(0, 1)]
    private float torchSpawnChance = 0.1f;

    [Header("Common Settings")]
    [SerializeField]
    private Vector2 worldOffset = new Vector2(0.5f, 0.5f);
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
                InstantiateAt(torchPrefab, position, Quaternion.identity);
            }
        }
    }

    private void HandleRoomsWithFloors(IReadOnlyList<RoomFirstDungeonGenerator.RoomFloorData> rooms)
    {
        if (rooms == null || rooms.Count == 0) return;

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

        List<RoomFirstDungeonGenerator.RoomFloorData> enemyRooms = new List<RoomFirstDungeonGenerator.RoomFloorData>();
        foreach (var r in lastRooms)
        {
            if (r.Center == playerRoom.Center || r.Center == endRoom.Center) continue;
            enemyRooms.Add(r);
        }

        SpawnPlayerAtRoom(playerRoom);
        SpawnEndLadderAtRoom(endRoom);

        // Spawn all content types
        SpawnChests(lastRooms);
        SpawnOtherProps(lastRooms);
        SpawnFoliage(lastRooms);
        SpawnPickups(lastRooms);
        SpawnEnemies(enemyRooms);
    }

    private void SpawnChests(List<RoomFirstDungeonGenerator.RoomFloorData> rooms)
    {
        if (chestPrefabs == null || chestPrefabs.Length == 0) return;

        foreach (var room in rooms)
        {
            if (Random.value < chestSpawnChance)
            {
                GameObject prefab = chestPrefabs[Random.Range(0, chestPrefabs.Length)];
                if (prefab != null)
                {
                    SpawnObjectAtRandomTile(prefab, room);
                }
            }
        }
    }

    private void SpawnOtherProps(List<RoomFirstDungeonGenerator.RoomFloorData> rooms)
    {
        if (otherPropPrefabs == null || otherPropPrefabs.Length == 0) return;

        foreach (var room in rooms)
        {
            int count = Random.Range(minPropsPerRoom, maxPropsPerRoom + 1);
            for (int i = 0; i < count; i++)
            {
                GameObject prefab = allowDuplicatePropsInRoom
                    ? otherPropPrefabs[Random.Range(0, otherPropPrefabs.Length)]
                    : otherPropPrefabs[Mathf.Clamp(i, 0, otherPropPrefabs.Length - 1)];

                if (prefab != null)
                {
                    SpawnObjectAtRandomTile(prefab, room);
                }
            }
        }
    }

    private void SpawnFoliage(List<RoomFirstDungeonGenerator.RoomFloorData> rooms)
    {
        if (foliagePrefabs == null || foliagePrefabs.Length == 0) return;

        foreach (var room in rooms)
        {
            int count = Random.Range(minFoliagePerRoom, maxFoliagePerRoom + 1);
            for (int i = 0; i < count; i++)
            {
                GameObject prefab = foliagePrefabs[Random.Range(0, foliagePrefabs.Length)];
                if (prefab != null)
                {
                    SpawnObjectAtRandomTile(prefab, room);
                }
            }
        }
    }

    private void SpawnPickups(List<RoomFirstDungeonGenerator.RoomFloorData> rooms)
    {
        foreach (var room in rooms)
        {
            // Chance to spawn a small potion
            if (smallHealthPotionPrefab != null && Random.value < smallHealthPotionSpawnChance)
            {
                SpawnObjectAtRandomTile(smallHealthPotionPrefab, room);
            }

            // Chance to spawn a big potion
            if (bigHealthPotionPrefab != null && Random.value < bigHealthPotionSpawnChance)
            {
                SpawnObjectAtRandomTile(bigHealthPotionPrefab, room);
            }
        }
    }

    private void SpawnEnemies(List<RoomFirstDungeonGenerator.RoomFloorData> rooms)
    {
        foreach (var room in rooms)
        {
            int count = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);
            for (int i = 0; i < count; i++)
            {
                GameObject enemyPrefabToSpawn = meleeEnemyPrefab;
                if (rangedEnemyPrefab != null && Random.value < rangedEnemySpawnChance)
                {
                    enemyPrefabToSpawn = rangedEnemyPrefab;
                }

                if (enemyPrefabToSpawn != null)
                {
                    var enemyInstance = SpawnObjectAtRandomTile(enemyPrefabToSpawn, room);
                    if (enemyInstance != null && enemyInstance.TryGetComponent<EnemyAI>(out var ai))
                    {
                        ai.SetPlayer(playerTransform);
                    }
                }
            }
        }
    }

    private void SpawnPlayerAtRoom(RoomFirstDungeonGenerator.RoomFloorData room)
    {
        if (playerPrefab == null) return;
        var instance = SpawnObjectAtRandomTile(playerPrefab, room);
        playerTransform = instance.transform;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayer(instance.GetComponent<Health>());
        }
        EnsureCameraFollowsPlayer();
    }

    private void SpawnEndLadderAtRoom(RoomFirstDungeonGenerator.RoomFloorData room)
    {
        if (endLadderPrefab == null) return;
        Vector3 pos = GetTrueRoomCenterPosition(room);
        InstantiateAt(endLadderPrefab, pos, Quaternion.identity);
    }

    private GameObject SpawnObjectAtRandomTile(GameObject prefab, RoomFirstDungeonGenerator.RoomFloorData room)
    {
        if (room.FloorTiles == null || room.FloorTiles.Count == 0) return null;
        Vector2Int tile = room.FloorTiles[Random.Range(0, room.FloorTiles.Count)];
        return InstantiateAt(prefab, tile, Quaternion.identity);
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

    private IReadOnlyList<Vector2Int> GetCenters(List<RoomFirstDungeonGenerator.RoomFloorData> rooms)
    {
        List<Vector2Int> centers = new List<Vector2Int>(rooms.Count);
        foreach (var r in rooms) centers.Add(r.Center);
        return centers;
    }

    private GameObject InstantiateAt(GameObject prefab, Vector2Int gridPos, Quaternion rotation)
    {
        Vector3 worldPos = new Vector3(gridPos.x + worldOffset.x, gridPos.y + worldOffset.y, 0f);
        return InstantiateAt(prefab, worldPos, rotation);
    }

    private GameObject InstantiateAt(GameObject prefab, Vector3 worldPos, Quaternion rotation)
    {
        var parent = contentParent != null ? contentParent : transform;
        var instance = Instantiate(prefab, worldPos, rotation, parent);
        instance.name = $"{prefab.name}_{Mathf.RoundToInt(worldPos.x)}_{Mathf.RoundToInt(worldPos.y)}";
        return instance;
    }

    private void EnsureCameraFollowsPlayer()
    {
        if (playerTransform == null) return;
        if (virtualCamera == null) virtualCamera = FindObjectOfType<CinemachineCamera>();
        if (virtualCamera != null)
        {
            virtualCamera.Follow = playerTransform;
            virtualCamera.LookAt = playerTransform;
        }
    }
}