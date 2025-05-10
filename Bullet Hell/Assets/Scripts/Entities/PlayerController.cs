using System;
using System.Collections;
using UnityEngine;

public class PlayerController : BaseEntity
{
    private Rigidbody2D rb;
    private Vector2 moveDir;
    private Vector2 previousDir; // Used for animation exclusively
    private Animator animator;

    public bool dashAvailable = true;
    private bool dashing = false;
    public float dashMultiplier;
    public float dashLength;
    public float dashCooldown;

    public bool hasBossroomKey;

    private BatSwingController batSwingController;

    public static PlayerController Instance;

    private void Awake()
    {
        // Singleton (yippee!) :D
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected new void Start()
    {
        base.Start();

        batSwingController = GetComponent<BatSwingController>();
        rb = GetComponent<Rigidbody2D>();
        animator = transform.GetChild(0).GetComponent<Animator>();
        dashMultiplier = dashMultiplier > 0 ? dashMultiplier : 1f;
        previousDir = new Vector2(1, -1);
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

        // Bat Controls
        if (Input.GetMouseButtonDown(0))
            batSwingController.StartSwingWindup();
        else if (Input.GetMouseButtonUp(0))
            batSwingController.StopSwingWindup();
    }

    private void FixedUpdate()
    {
        // Move the player
        Vector2 updatedDir = moveDir * moveSpeed * Time.deltaTime * dashMultiplier + (Vector2)transform.position;
        rb.MovePosition(updatedDir);

        // Update animations
        if ((updatedDir - (Vector2)transform.position).magnitude != 0)
        {
            animator.SetFloat("Input X", updatedDir.x - transform.position.x);
            animator.SetFloat("Input Y", updatedDir.y - transform.position.y);
            animator.SetBool("IsMoving", true);
            previousDir = updatedDir - (Vector2)transform.position;
        }
        else
        {
            animator.SetFloat("Input X", previousDir.x);
            animator.SetFloat("Input Y", previousDir.y);
            animator.SetBool("IsMoving", false);
        }
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
