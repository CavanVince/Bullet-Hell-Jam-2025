using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;


    private bool dashAvailable = true;

    private bool dashing = false;
   
    private bool swinging = false;
    private bool canSwing = true;

    private Vector2 moveDir;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    public float batStartAngle;
    public float batLength;
    public float swingTime;
    public float dashMultiplier;
    
    


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
        swinging = true;
        canSwing = false;
        dashAvailable = false;

        float duration   = swingTime;
        float elapsed    = 0f;
        float startAngle = -batStartAngle;  // bottom
        float endAngle   =  batStartAngle;  // top
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector3 batCenterVector = (mousePosition - transform.position).normalized;
        float angle = Mathf.Atan2(batCenterVector.y, batCenterVector.x) * Mathf.Rad2Deg;
        

        if (angle < 0)
        {
            angle += 360;
        }
        int directionIndex = Mathf.RoundToInt(angle / 45) % 8;
        Vector3[] directions = new Vector3[8]
        {
            Vector3.right,
            new Vector3(1,1,0).normalized,
            Vector3.up,
            new Vector3(-1,1,0).normalized,
            Vector3.left,
            new Vector3(-1,-1,0).normalized,
            Vector3.down,
            new Vector3(1,-1,0).normalized
        };

        Vector3 adjCenter = directions[directionIndex];  

        Vector3 origin = transform.position;
        Vector3 dir = Quaternion.Euler(0f, 0f, startAngle) * Vector3.right;

        List<GameObject> hits = new List<GameObject>();

        while (elapsed < duration)
        {
            origin = transform.position;
            float t = elapsed / duration;
            float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
            dir = Quaternion.Euler(0f, 0f, currentAngle) * adjCenter;
            Debug.DrawRay(origin, dir * batLength, Color.red);

            RaycastHit2D hit = Physics2D.Raycast(origin, dir.normalized, batLength, 1 << 6);
            if (hit && !hits.Contains(hit.transform.gameObject))
            {
                int layer = hit.transform.gameObject.layer;
                Debug.Log(hit.transform.name);
                Debug.Log(layer);
                if (layer == 6) // bullet layer
                {
                    Vector2 reflectDir = -hit.transform.GetComponent<Bullet>().moveDir;
                    hit.transform.GetComponent<Bullet>().Fire(reflectDir * 5f);
                }

                hits.Add(hit.transform.gameObject);
            }


            elapsed += Time.deltaTime;
            
            yield return null;
        }
        yield return new WaitForSeconds(duration);
        dashAvailable = true;
        swinging = false;
        canSwing = true;
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


        yield return new WaitForSeconds(.5f);
        dashMultiplier = 1f;
        dashing = false;

        yield return new WaitForSeconds(1);
        dashAvailable = true;

    }
}
