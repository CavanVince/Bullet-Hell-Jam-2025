using System;
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

    // Flag if the player entered the room
    private bool isRoomEntered = false;

    // The enemy count of the room
    private int enemyCount = 0;

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

    // Bounds of the pathfinding tilemap
    public BoundsInt GridBounds;

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

    private AudioSource audioSource;

    [SerializeField]
    private AudioClip doorOpenClip;
    [SerializeField]
    private AudioClip doorCloseClip;

    private void Awake()
    {
        // Populate array with walkable tiles
        pathfindingTilemap.CompressBounds();
        GridBounds = pathfindingTilemap.cellBounds;
        walkabilityGrid = new NativeArray<int>(GridBounds.size.x * GridBounds.size.y, Allocator.Persistent);

        int horizontalDiff = Mathf.Sign(GridBounds.min.x) == -1 ? -GridBounds.min.x : 0;
        int verticalDiff = Mathf.Sign(GridBounds.min.x) == -1 ? -GridBounds.min.y : 0;

        for (int x = GridBounds.min.x; x < GridBounds.max.x; x++)
        {
            for (int y = GridBounds.min.y; y < GridBounds.max.y; y++)
            {
                TileBase tile = pathfindingTilemap.GetTile(new Vector3Int(x, y, 0));
                walkabilityGrid[(x + horizontalDiff) + (y + verticalDiff) * GridBounds.size.x] = 0;

                if (tile != null)
                {
                    walkabilityGrid[(x + horizontalDiff) + (y + verticalDiff) * GridBounds.size.x] = 1;
                }
            }
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
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
        roomCleared?.Invoke();
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
        audioSource.clip = doorOpenClip;
        audioSource.Play();
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
        audioSource.clip = doorCloseClip;
        audioSource.Play();
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
            GameObject enemy = EntityManager.instance.SummonEnemy(GetRandomEnemyType(), enemySpawnPoints.transform.GetChild(i).transform.position, Quaternion.identity);
            enemy.GetComponent<BaseEnemy>().OwningRoom = this;
            enemyCount++;
        }
    }

    /// <summary>
    /// How the room should process the event of an enemy dying
    /// </summary>
    public void EnemyDied()
    {
        enemyCount--;
        if (enemyCount <= 0)
        {
            RoomCleared();
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

    /// <summary>
    /// Helper function to return a random enemy type
    /// </summary>
    /// <returns></returns>
    private Type GetRandomEnemyType()
    {
        int val = UnityEngine.Random.Range(0, 6);
        switch (val)
        {
            case 0:
                return typeof(BaseEnemy);
            case 1:
                return typeof(ShotgunEnemy);
            case 2:
                return typeof(AerialShooterEnemy);
            case 3:
                return typeof(OctopusShooterEnemy);
            case 4:
                return typeof(RadialShooterEnemy);
                case 5:
                return typeof(BombEnemy);
        }
        return typeof(BaseEnemy);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") != true) return;

        if (!isRoomEntered) isRoomEntered = true;
        else return;

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
