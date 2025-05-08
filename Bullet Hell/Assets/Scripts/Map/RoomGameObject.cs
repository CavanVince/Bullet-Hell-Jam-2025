using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RoomGameObject : MonoBehaviour
{
    // Flag if the player cleared the room
    private bool roomCleared = false;

    public UnityEvent roomEntered;

    void Start()
    {

    }

    /// <summary>
    /// Logic to perform when the room is succesfully cleared
    /// </summary>
    public void RoomCleared()
    {
        roomCleared = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collider");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger");
    }
}
