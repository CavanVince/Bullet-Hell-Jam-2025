using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class RoomGameObject : MonoBehaviour
{
    // Flag if the player cleared the room
    private bool isRoomCleared = false;

    // Global event that gets called to all rooms when the player enters it
    public static UnityEvent roomEntered;

    // Global event that gets called to all rooms when the player clear the current one they are in
    public static UnityEvent roomCleared;

    // The bottom left wall when there isn't a connecting room
    public GameObject BottomLeftWall;

    // The bottom right wall when there isn't a connecting room
    public GameObject BottomRightWall;

    // The top left wall when there isn't a connecting room
    public GameObject TopLeftWall;

    // The top right wall when there isn't a connecting room
    public GameObject TopRightWall;

    [SerializeField]
    private List<GameObject> doors;

    [SerializeField]
    private Tilemap fogOfWar;

    [SerializeField]
    private Tilemap pathfindingTilemap;

    [SerializeField]
    private List<GameObject> enemyPrefabs;

    private int[,] walkableTiles;

    private void Awake()
    {
        // Populate array with walkable tiles
        pathfindingTilemap.CompressBounds();
        BoundsInt bounds = pathfindingTilemap.cellBounds;
        walkableTiles = new int[bounds.max.x - bounds.min.x, bounds.max.y - bounds.min.y];
        for (int x = bounds.min.x; x < bounds.size.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.size.y; y++)
            {
                TileBase tile = pathfindingTilemap.GetTile(new Vector3Int(x, y, 0));
                if (tile != null)
                {
                    Debug.Log("x: " + x + " y: " + y + " tile: " + tile.name);
                }
            }
        }
    }

    void Start()
    {
        if (roomEntered == null)
            roomEntered = new UnityEvent();

        if (roomCleared == null)
            roomCleared = new UnityEvent();

        roomEntered.AddListener(CloseDoors);
        roomCleared.AddListener(OpenDoors);
    }

    /// <summary>
    /// Logic to perform when the room is succesfully cleared
    /// </summary>
    public void RoomCleared()
    {
        isRoomCleared = true;
        OpenDoors();
    }

    /// <summary>
    /// Open the doors in the room
    /// </summary>
    public void OpenDoors()
    {
        // Open doors
        foreach (GameObject door in doors)
        {
            Animator doorAnim = door.GetComponent<Animator>();
            doorAnim.Play("DoorOpen");
            door.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Close the doors in the room
    /// </summary>
    public void CloseDoors()
    {
        // Close doors
        foreach (GameObject door in doors)
        {
            Animator doorAnim = door.GetComponent<Animator>();
            doorAnim.Play("DoorClose");
            door.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Helper function to start the fog fade coroutine
    /// </summary>
    public void FadeFog()
    {
        StartCoroutine(FadeFogCoroutine());
    }

    private void SpawnEnemies()
    {

    }

    IEnumerator FadeFogCoroutine()
    {
        Color modifiedColor = fogOfWar.color;
        while (modifiedColor.a > 0)
        {
            modifiedColor.a -= Time.deltaTime;
            fogOfWar.color = modifiedColor;
            yield return new WaitForEndOfFrame();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isRoomCleared) return;

        roomEntered?.Invoke();
        FadeFog();
    }
}
