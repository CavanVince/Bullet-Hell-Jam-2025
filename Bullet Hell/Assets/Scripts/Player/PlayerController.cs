using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;


    private bool dashAvailable = true;

    private bool dashing = false;
   
    private bool canSwing = true;

    private Vector2 moveDir;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    public int playerHealth;
    public float batStartAngle;
    public float batEndAngle;
    public float batLength;
    public float swingTime;
    public float dashMultiplier;
    public float batCooldown;
    public float hitICooldown;
    public bool isHittable;
    public float hitPower;
    


    private enum MoveStates
    {
        Moving,
        Dashing,
    }


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dashMultiplier = 1f;
        playerHealth = 5;
        isHittable = true;
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
        if (Input.GetMouseButtonDown(0))
        {
            if (!canSwing)
            {
                return;
            }
            StartCoroutine(SwingBat());
        }
    }
    private IEnumerator SwingBat()
    {
        canSwing = false;
        dashAvailable = false;

        float duration   = swingTime;
        float elapsed    = 0f;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector3 aimVector = (mousePosition - transform.position).normalized;
        Vector3 centerPerpend = new Vector3(-aimVector.y, aimVector.x, 0);
        float angle = Mathf.Atan2(centerPerpend.y, centerPerpend.x) * Mathf.Rad2Deg;
        

        if (angle < 0)
        {
            angle += 360;
        }
        int directionIndex = Mathf.RoundToInt(angle / 45) % 8;
        Vector3[] directions = new Vector3[8]
        {
            Vector3.left,
            new Vector3(-1,-1,0).normalized,
            Vector3.down,
            new Vector3(1,-1,0).normalized,
            Vector3.right,
            new Vector3(1,1,0).normalized,
            Vector3.up,
            new Vector3(-1,1,0).normalized
        };

        Vector3 adjCenter = directions[directionIndex];  

        Vector3 origin = transform.position;
        Vector3 dir = Quaternion.Euler(0f, 0f, batStartAngle) * Vector3.right;

        List<GameObject> hits = new List<GameObject>();

        while (elapsed < duration)
        {
            origin = transform.position;
            float t = elapsed / duration;
            float currentAngle = Mathf.Lerp(batStartAngle, batEndAngle, t);
            dir = Quaternion.Euler(0f, 0f, currentAngle) * adjCenter;
            Debug.DrawRay(origin, dir * batLength, Color.red);

            RaycastHit2D hit = Physics2D.Raycast(origin, dir.normalized, batLength, 1 << 6 | 1 << 7);
            if (hit && !hits.Contains(hit.transform.gameObject))
            {
                int layer = hit.transform.gameObject.layer;
                Debug.Log(hit.transform.name);
                Vector3 camMousePos3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 camMousePos2D = new Vector2(camMousePos3D.x, camMousePos3D.y);
                
            

                if (layer == 6) // bullet layer
                {
                    Bullet bullet = hit.transform.GetComponent<Bullet>();
                    hit.transform.gameObject.layer = 8; // Player Projectile layer
                    Vector2 reflectDir = (camMousePos2D - new Vector2(hit.transform.position.x, hit.transform.position.y)).normalized;
                    hit.transform.GetComponent<Bullet>().Fire(reflectDir * hitPower);
                    
                }
                if (layer == 7) // enemy layer
                {
                    Vector2 enemyDir = (camMousePos2D - new Vector2(hit.transform.position.x, hit.transform.position.y)).normalized;
                    Debug.Log("enemy hit " + hit.transform.name);
                    hit.transform.GetComponent<BaseEnemy>().Launch(enemyDir * hitPower);
                }
                hits.Add(hit.transform.gameObject);
            }


            elapsed += Time.deltaTime;
            
            yield return null;
        }
        yield return new WaitForSeconds(.5f);
        dashAvailable = true;
        canSwing = true;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        int layer = collision.gameObject.layer;
        if (isHittable && (layer == 6 || layer == 7)) // bullet or enemy layer (layer is 6 or 7) 
        {
            if (layer == 6)
            {
                collision.gameObject.SetActive(false);
            }
            isHittable = false;
            Debug.Log(transform.name + " Hit by: " + collision.name);
            playerHealth--;
            if (playerHealth <= 0)
            {
                Debug.Log("Game Over!!!!!!!!!!!! STOP PLAYING!!!!!!!!STOP MOVING AROUND YOU DUMBASS");
            }
            StartCoroutine(PlayerHit());
        }
    }
    private IEnumerator PlayerHit()
    {
        transform.GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(hitICooldown);
        isHittable = true;
        transform.GetComponent<SpriteRenderer>().color = Color.white;
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
        dashAvailable = false;
        dashing = true;

        dashMultiplier = 2f;


        yield return new WaitForSeconds(batCooldown);
        dashMultiplier = 1f;
        dashing = false;

        yield return new WaitForSeconds(1);
        dashAvailable = true;

    }
}
