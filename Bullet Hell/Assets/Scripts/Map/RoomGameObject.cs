using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
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

    // Array of 1's and 0's to indicate tile walkability
    public NativeArray<int> walkabilityGrid;

    // List of the room door game objects
    [SerializeField]
    private List<GameObject> doors;

    // Fog of war tilemap
    [SerializeField]
    private Tilemap fogOfWar;

    // Pathfinding tilemap
    [SerializeField]
    private Tilemap pathfindingTilemap;

    // Spawn point parent
    [SerializeField]
    private GameObject enemySpawnPoints;

    // List of enemy prefabs
    [SerializeField]
    private List<GameObject> enemyPrefabs;

    private void Awake()
    {
        // Populate array with walkable tiles
        pathfindingTilemap.CompressBounds();
        BoundsInt bounds = pathfindingTilemap.cellBounds;
        walkabilityGrid = new NativeArray<int>(bounds.size.x * bounds.size.y, Allocator.Persistent);

        int horizontalDiff = Mathf.Sign(bounds.min.x) == -1 ? -bounds.min.x : 0;
        int verticalDiff = Mathf.Sign(bounds.min.x) == -1 ? -bounds.min.y : 0;

        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                TileBase tile = pathfindingTilemap.GetTile(new Vector3Int(x, y, 0));
                walkabilityGrid[(x + horizontalDiff) + (y + verticalDiff) * bounds.size.x] = 0;

                if (tile != null)
                {
                    walkabilityGrid[(x + horizontalDiff) + (y + verticalDiff) * bounds.size.x] = 1;
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
        for (int i = 0; i < enemySpawnPoints.transform.childCount; i++)
        {
            EntityManager.instance.SummonEnemy(typeof(BaseEnemy), enemySpawnPoints.transform.GetChild(i).transform.position, Quaternion.identity);
        }
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
        if (collision.CompareTag("Player") != true) return;
        if (isRoomCleared) return;

        roomEntered?.Invoke();
        FadeFog();
        SpawnEnemies();
    }

    private void OnDestroy()
    {
        walkabilityGrid.Dispose();
    }
}
