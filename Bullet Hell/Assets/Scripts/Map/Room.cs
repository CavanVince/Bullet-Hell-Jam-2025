using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    SPAWN,
    BOSS,
    SHOP,
    TREASURE,
    EMPTY
}

public enum RoomPrerequisite
{
    NONE,
    KEY,
    SPECIAL_ITEM
}

public class Room
{
    public List<Vector2Int> mapTilePos;
    public RoomPrerequisite prerequisite;
    public RoomType roomType;
    public List<Room> connectedRooms;
    public GameObject prefab;

    public Room(List<Vector2Int> pos, RoomPrerequisite prereq, RoomType type, GameObject prefabRef)
    {
        mapTilePos = pos;
        prerequisite = prereq;
        roomType = type;
        connectedRooms = new List<Room>();
        prefab = prefabRef;
    }
}

public class RoomConfig
{
    public GameObject prefab;
    public List<Vector2Int> tilePositions;
    public RoomType roomType;

    public RoomConfig(GameObject prefab, List<Vector2Int> positions, RoomType type = RoomType.EMPTY)
    {
        this.prefab = prefab;
        this.tilePositions = positions;
        this.roomType = type;
    }
}