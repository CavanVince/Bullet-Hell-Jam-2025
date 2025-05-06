using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool requiresKey;
    public bool open;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (!requiresKey || collision.gameObject.GetComponent<PlayerController>().hasBossroomKey)
            {
                open = true;
            }
        }
    }
}
