using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class MapGenerator : MonoBehaviour
{

    public GameObject bossRoomKey;

    [SerializeField]
    private int WIDTH = 30, HEIGHT = 30;

    [SerializeField]
    private float room_size = 30f;

    [Range(0, 50)]
    public int MINROOMS = 15;
    public int MAXROOMS = 20;

    [SerializeField, Range(0f, 1f)]
    private float roomPlacementChance = 0.5f;

    private Room[,] board;

    [SerializeField]
    private List<GameObject> rooms;

    [SerializeField]
    private GameObject bossPrefab;

    [SerializeField]
    private GameObject shopPrefab;

    [SerializeField]
    private GameObject treasurePrefab;

    private List<GameObject> mapTiles = new List<GameObject>();
    private List<Room> allRooms = new List<Room>();
    private Vector3 spawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        Vector2Int startPos = new Vector2Int(WIDTH / 2, HEIGHT / 2);
        do
        {
            ClearMap();
            board = new Room[WIDTH, HEIGHT];
            CreateMap(startPos, board);

        } while (allRooms.Count() < MINROOMS);

        BuildTileMap();
        AssignSpecialRooms();
        MovePlayer();
    }

    void ClearMap()
    {
        foreach (GameObject tile in mapTiles)
        {
            Destroy(tile);
        }
        mapTiles.Clear();
        allRooms.Clear();
    }

    // This method checks if a room configuration can fit at the given position
    bool CanPlaceRoomConfiguration(List<Vector2Int> tilePositions)
    {
        foreach (Vector2Int pos in tilePositions)
        {
            // Check if position is within map bounds
            if (pos.x < 0 || pos.x >= WIDTH || pos.y < 0 || pos.y >= HEIGHT)
                return false;

            // Check if position is already occupied
            if (board[pos.x, pos.y] != null)
                return false;
        }
        return true;
    }

    // Gets all valid room configurations that can be placed at a given position
    Dictionary<GameObject, List<Vector2Int>> GetAllRoomConfigurationsForCandidate(Vector2Int pos)
    {
        Dictionary<GameObject, List<Vector2Int>> validConfigurations = new Dictionary<GameObject, List<Vector2Int>>();

        // 1x1 Room (basic room)
        List<Vector2Int> singleTile = new List<Vector2Int> { pos };
        if (CanPlaceRoomConfiguration(singleTile))
        {
            validConfigurations.Add(rooms[Random.Range(0, rooms.Count)], singleTile);
        }

        return validConfigurations;

    }


    void PopulatePositionsForRoom(Room room)
    {
        foreach (Vector2Int pos in room.mapTilePos)
        {
            board[pos.x, pos.y] = room;
        }
    }

    // Find doorway positions between two rooms
    List<Vector2Int> FindDoorPositions(Room roomA, Room roomB)
    {
        List<Vector2Int> doorPositions = new List<Vector2Int>();

        foreach (Vector2Int posA in roomA.mapTilePos)
        {
            foreach (Vector2Int posB in roomB.mapTilePos)
            {
                // Check if the tiles are adjacent (not diagonal)
                if ((Mathf.Abs(posA.x - posB.x) == 1 && posA.y == posB.y) ||
                    (Mathf.Abs(posA.y - posB.y) == 1 && posA.x == posB.x))
                {
                    doorPositions.Add(new Vector2Int((posA.x + posB.x) / 2, (posA.y + posB.y) / 2));
                }
            }
        }

        return doorPositions;
    }

    void CreateMap(Vector2Int startPos, Room[,] board)
    {
        // Create spawn room (1x1)
        Room spawnRoom = new Room(new List<Vector2Int> { startPos }, RoomPrerequisite.NONE, RoomType.SPAWN, rooms[0]);
        allRooms.Add(spawnRoom);
        PopulatePositionsForRoom(spawnRoom);

        Queue<Room> roomQueue = new Queue<Room>();
        roomQueue.Enqueue(spawnRoom);

        int filledRooms = 1;

        while (roomQueue.Count > 0 && filledRooms < MAXROOMS)
        {
            Room currentRoom = roomQueue.Dequeue();

            // Get all non-occupied positions around the current room
            List<Vector2Int> potentialPositions = GetNeighborCoords(currentRoom.mapTilePos).Where(p => board[p.x, p.y] == null).ToList();

            // Shuffle for randomness
            potentialPositions = ShuffleList(potentialPositions);

            foreach (Vector2Int pos in potentialPositions)
            {
                // Chance to skip this position
                if (Random.value > roomPlacementChance)
                    continue;

                // Get all valid room configurations for this position
                Dictionary<GameObject, List<Vector2Int>> possibleConfigs = GetAllRoomConfigurationsForCandidate(pos);

                if (possibleConfigs.Count == 0)
                    continue;

                // Choose a random configuration
                int randomConfigIndex = Random.Range(0, possibleConfigs.Count);
                KeyValuePair<GameObject, List<Vector2Int>> chosenConfig = possibleConfigs.ElementAt(randomConfigIndex);

                // Create new room with the chosen configuration
                Room newRoom = new Room(
                    chosenConfig.Value,
                    RoomPrerequisite.NONE,
                    RoomType.EMPTY,
                    chosenConfig.Key
                );

                // Add room to map
                PopulatePositionsForRoom(newRoom);
                ConnectRooms(currentRoom, newRoom);
                roomQueue.Enqueue(newRoom);
                allRooms.Add(newRoom);

                filledRooms++;

                // Stop if we've reached the maximum room count
                if (filledRooms >= MAXROOMS)
                    break;
            }
        }
    }

    // Assign special rooms (boss, shop, treasure)
    void AssignSpecialRooms()
    {
        // Skip if not enough rooms
        if (allRooms.Count <= 1)
            return;

        // Find the room furthest from spawn to be the boss room
        Room furthestRoom = FindRoomFurthestFromSpawn();
        if (furthestRoom != null)
        {
            furthestRoom.roomType = RoomType.BOSS;
            furthestRoom.prefab = bossPrefab;
        }

        // Pick a random room for shop and treasure
        List<Room> availableRooms = allRooms.Where(r => r.roomType == RoomType.EMPTY).ToList();

        if (availableRooms.Count > 0)
        {
            Room shopRoom = availableRooms[Random.Range(0, availableRooms.Count)];
            shopRoom.roomType = RoomType.SHOP;
            shopRoom.prefab = shopPrefab;

            availableRooms = availableRooms.Where(r => r != shopRoom).ToList();
        }

        if (availableRooms.Count > 0)
        {
            Room treasureRoom = availableRooms[Random.Range(0, availableRooms.Count)];
            treasureRoom.roomType = RoomType.TREASURE;
            treasureRoom.prefab = treasurePrefab;
        }
    }

    Room FindRoomFurthestFromSpawn()
    {
        Room spawnRoom = allRooms.FirstOrDefault(r => r.roomType == RoomType.SPAWN);
        if (spawnRoom == null)
            return null;

        Dictionary<Room, int> distances = new Dictionary<Room, int>();
        Dictionary<Room, bool> visited = new Dictionary<Room, bool>();

        foreach (Room room in allRooms)
        {
            distances[room] = int.MaxValue;
            visited[room] = false;
        }

        distances[spawnRoom] = 0;
        Queue<Room> queue = new Queue<Room>();
        queue.Enqueue(spawnRoom);
        visited[spawnRoom] = true;

        while (queue.Count > 0)
        {
            Room current = queue.Dequeue();

            foreach (Room neighbor in current.connectedRooms)
            {
                if (!visited[neighbor])
                {
                    visited[neighbor] = true;
                    distances[neighbor] = distances[current] + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Find the room with the highest distance
        Room furthest = null;
        int maxDistance = -1;

        foreach (var kvp in distances)
        {
            if (kvp.Value > maxDistance && kvp.Value != int.MaxValue)
            {
                maxDistance = kvp.Value;
                furthest = kvp.Key;
            }
        }

        return furthest;
    }

    private void BuildTileMap()
    {
        // Clear previous tiles first
        foreach (GameObject tile in mapTiles)
        {
            Destroy(tile);
        }
        mapTiles.Clear();

        List<Room> seenRooms = new List<Room>();

        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                Room currentRoom = board[x, y];
                if (currentRoom == null) continue;
                if (seenRooms.Contains(currentRoom)) continue;

                seenRooms.Add(currentRoom);

                Vector3 roomPos = new Vector3(x * -room_size + y * room_size, y * -(room_size / 2) - x * (room_size / 2), 0);
                GameObject roomInstance = Instantiate(
                    currentRoom.prefab,
                    roomPos,
                    transform.rotation
                );

                RoomGameObject roomGo = roomInstance.transform.Find("Grid")?.GetComponent<RoomGameObject>();
                roomGo?.OpenDoors();

                if (currentRoom.roomType == RoomType.SPAWN)
                {
                    //roomInstance.GetComponentInChildren<Tilemap>().color = Color.yellow;
                    spawnPoint = roomPos;
                    roomGo?.RoomCleared();
                    roomGo?.FadeFog();
                }
                else if (currentRoom.roomType == RoomType.BOSS)
                {
                    //roomInstance.GetComponentInChildren<Tilemap>().color = Color.red;
                }

                // Determine if a wall needs to be enabled due to a lack of a connecting room
                if (y + 1 < board.GetLength(1) && board[x, y + 1] == null)
                {
                    // No bottom right neighbour
                    roomGo?.BottomRightWall?.SetActive(true);
                }
                if (x + 1 < board.GetLength(0) && board[x + 1, y] == null)
                {
                    // No bottom left neighbour
                    roomGo?.BottomLeftWall?.SetActive(true);
                }
                if (y - 1 > 0 && board[x, y - 1] == null)
                {
                    // No top left neighbour
                    roomGo?.TopLeftWall?.SetActive(true);
                }
                if (x - 1 > 0 && board[x - 1, y] == null)
                {
                    // No top right neighbour
                    roomGo?.TopRightWall?.SetActive(true);
                }

                mapTiles.Add(roomInstance);
            }
        }


        mapTiles[mapTiles.Count - 1].transform.Find("Grid")?.GetComponent<RoomGameObject>().AddDrop(bossRoomKey);
    }

    void ConnectRooms(Room a, Room b)
    {
        a.connectedRooms.Add(b);
        b.connectedRooms.Add(a);
    }

    List<Vector2Int> GetNeighborCoords(List<Vector2Int> roomBounds)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        foreach (Vector2Int current in roomBounds)
        {
            // Check all four directions
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, -1), // Left
                new Vector2Int(0, 1),  // Right
                new Vector2Int(1, 0), // Down
                new Vector2Int(-1, 0)   // Up
            };

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = current + dir;

                // Check if within bounds
                if (neighbor.x >= 0 && neighbor.x < WIDTH && neighbor.y >= 0 && neighbor.y < HEIGHT)
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        // Remove duplicates and positions that are already part of the room
        neighbors = neighbors.Distinct().Where(n => !roomBounds.Contains(n)).ToList();

        return neighbors;
    }

    // Helper to shuffle a list
    private List<T> ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }

    // Debugging visualization
    private void OnDrawGizmos()
    {
        if (board == null) return;

        // Colors for different room types
        Color spawnColor = Color.yellow;
        Color bossColor = Color.red;
        Color shopColor = Color.blue;
        Color treasureColor = Color.green;
        Color emptyColor = Color.gray;
        Color connectionColor = Color.cyan;

        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                Room room = board[x, y];
                if (room != null)
                {
                    // Draw room
                    Vector3 position = new Vector3(x, y, 0);

                    // Set color based on room type
                    switch (room.roomType)
                    {
                        case RoomType.SPAWN:
                            Gizmos.color = spawnColor;
                            break;
                        case RoomType.BOSS:
                            Gizmos.color = bossColor;
                            break;
                        case RoomType.SHOP:
                            Gizmos.color = shopColor;
                            break;
                        case RoomType.TREASURE:
                            Gizmos.color = treasureColor;
                            break;
                        default:
                            Gizmos.color = emptyColor;
                            break;
                    }

                    Gizmos.DrawCube(position, new Vector3(0.9f, 0.9f, 0.1f));
                }
            }
        }

        // Draw connections between rooms
        Gizmos.color = connectionColor;
        HashSet<(Room, Room)> drawnConnections = new HashSet<(Room, Room)>();

        foreach (Room room in allRooms)
        {
            foreach (Room connectedRoom in room.connectedRooms)
            {
                // Check if this connection has already been drawn
                if (!drawnConnections.Contains((room, connectedRoom)) &&
                    !drawnConnections.Contains((connectedRoom, room)))
                {
                    // Calculate center positions
                    Vector3 startPos = CalculateRoomCenter(room.mapTilePos);
                    Vector3 endPos = CalculateRoomCenter(connectedRoom.mapTilePos);

                    // Draw line
                    Gizmos.DrawLine(startPos, endPos);

                    // Mark connection as drawn
                    drawnConnections.Add((room, connectedRoom));
                }
            }
        }
    }

    Vector2 CalculateRoomCenter(List<Vector2Int> tilePositions)
    {
        if (tilePositions.Count == 0)
            return Vector2.zero;

        float sumX = 0, sumY = 0;
        foreach (Vector2Int pos in tilePositions)
        {
            sumX += pos.x;
            sumY += pos.y;
        }

        // Calculate the average position (center point)
        return new Vector2(sumX / tilePositions.Count, sumY / tilePositions.Count);
    }

    /// <summary>
    /// Move the player to the spawn room
    /// </summary>
    private void MovePlayer()
    {
        Transform player = FindObjectOfType<PlayerController>().transform;
        player.position = spawnPoint;
    }
}