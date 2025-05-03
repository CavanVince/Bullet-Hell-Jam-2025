using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;

    private float temp;

    private bool dashAvailable = true;

    private bool dashing = false;

    private Vector2 moveDir;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;

    private enum MoveStates
    {
        Moving,
        Dashing,
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    void Update()
    {
        // Gather user input
        if (dashing == false)
        {
        MovePlayer();

        }

        if (Input.GetKeyDown(KeyCode.Space) && dashAvailable)
        {

            StartCoroutine(Dash());
        }
        
    }

    private void FixedUpdate()
    {
        // Move the player
        rb.MovePosition(moveDir * moveSpeed * Time.deltaTime + (Vector2)transform.position);
    }

    void MovePlayer()
    {
        moveDir = Vector2.zero;
        moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        moveDir = moveDir.normalized;
    }

    
    private IEnumerator Dash()
    {
        dashAvailable = false;
        dashing = true;

        temp = moveSpeed;
        yield return null;
        moveSpeed = moveSpeed * 2;

        yield return new WaitForSeconds(.5f);
        moveSpeed = temp;
        dashing = false;

        yield return new WaitForSeconds(1);
        dashAvailable = true;

    }
}
