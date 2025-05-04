using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform player;
    private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField]
    private float smoothSpeed = 0.125f;
    private Vector3 velocity = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, player.position + offset, ref velocity, smoothSpeed);
    }
}
