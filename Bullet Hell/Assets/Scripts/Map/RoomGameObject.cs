using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RoomGameObject : MonoBehaviour
{
    // Flag if the player cleared the room
    private bool isRoomCleared = false;

    // Global event that gets called to all rooms when the player enters it
    public static UnityEvent roomEntered;

    // Global event that gets called to all rooms when the player clear the current one they are in
    public static UnityEvent roomCleared;

    [SerializeField] List<GameObject> doors;

    // The bottom left wall when there isn't a connecting room
    public GameObject BottomLeftWall;

    // The bottom right wall when there isn't a connecting room
    public GameObject BottomRightWall;

    // The top left wall when there isn't a connecting room
    public GameObject TopLeftWall;

    // The top right wall when there isn't a connecting room
    public GameObject TopRightWall;

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
            Debug.Log("Closing");
            door.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isRoomCleared == false)
            roomEntered?.Invoke();
    }
}
