using System;
using System.Collections;
using UnityEngine;

public class PlayerController : BaseEntity
{
    private Rigidbody2D rb;
    private Vector2 moveDir;
    
    [SerializeField]
    private float moveSpeed;

    public bool dashAvailable = true;
    private bool dashing = false;
    public float dashMultiplier;
    public float dashLength;
    public float dashCooldown;

    public bool hasBossroomKey;

    private BatSwingController batSwingController;

    protected new void Start()
    {
        base.Start();

        batSwingController = GetComponent<BatSwingController>();
        rb = GetComponent<Rigidbody2D>();
        dashMultiplier = dashMultiplier > 0 ? dashMultiplier : 1f;
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

        // Bat Controls //
        if (Input.GetMouseButtonDown(0))
            batSwingController.StartSwingWindup();
        else if (Input.GetMouseButtonUp(0))
            batSwingController.StopSwingWindup();

        // Debug Control, TODO: Remove
        if (Input.GetKeyDown(KeyCode.F))
        {
            BulletManager.instance.FireAerialBullet(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    private void FixedUpdate()
    {
        // Move the player
        rb.MovePosition(moveDir * moveSpeed * Time.deltaTime * dashMultiplier + (Vector2)transform.position);
    }

    void MovePlayer()
    {
        moveDir = Vector2.zero;
        moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        moveDir = moveDir.normalized;
    }
    
    private IEnumerator Dash()
    
    {
        dashing = true;
        dashAvailable = false;
        dashMultiplier = 2f;

        yield return new WaitForSeconds(dashLength);

        dashMultiplier = 1f;
        dashing = false;

        yield return new WaitForSeconds(dashCooldown);
        dashAvailable = true;

    }
}
