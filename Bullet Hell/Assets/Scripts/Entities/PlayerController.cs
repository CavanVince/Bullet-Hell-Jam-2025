using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private Vector2 moveDir;
    
    [SerializeField]
    private float moveSpeed;

    private bool dashAvailable = true;
    private bool dashing = false;
    public float dashMultiplier;
    public float dashLength;
    public float dashCooldown;
    
    private bool canSwing = true;
    public float batStartAngle;
    public float batEndAngle;
    public float batLength;
    public float swingTime;
    public float batCooldown;
    public float hitPower;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dashMultiplier = dashMultiplier > 0 ? dashMultiplier : 1f;
        transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = false; // hide Charge Bar
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
            StartCoroutine(ChargeSwing());
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            BulletManager.instance.FireAerialBullet(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    private IEnumerator ChargeSwing()
    {
        GameObject chargeBar = transform.GetChild(1).gameObject;
        ChargeBarController controller = chargeBar.GetComponent<ChargeBarController>();
        chargeBar.SetActive(true);
        controller.StartChargeBarLevel();
        yield return SwingBat();
        chargeBar.SetActive(false);
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

        Vector3 origin = transform.GetChild(0).position;
        Vector3 dir = Quaternion.Euler(0f, 0f, batStartAngle) * Vector3.right;

        List<GameObject> hits = new List<GameObject>();

        while (elapsed < duration)
        {
            origin = transform.GetChild(0).position;
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
                Vector2 reflectDir = (camMousePos2D - new Vector2(hit.transform.position.x, hit.transform.position.y)).normalized;
                if (reflectDir.magnitude < batLength)
                {
                    reflectDir = (camMousePos2D - new Vector2(transform.GetChild(0).position.x, transform.GetChild(0).position.y)).normalized;
                }
                
                if (layer == BulletHellCommon.BULLET_LAYER && hit.transform.GetComponent<BaseBullet>().GetType() == typeof(StandardBullet))
                {
                    Debug.Log($"Hit Bullet {hit.transform.name} with bat");
                    StandardBullet bullet = hit.transform.GetComponent<StandardBullet>();
                    hit.transform.gameObject.layer = BulletHellCommon.PLAYER_PROJECTILE_LAYER; // Player Projectile layer
                    
                    hit.transform.GetComponent<StandardBullet>().Fire(reflectDir * hitPower);
                }
                if (layer == BulletHellCommon.ENEMY_LAYER)
                {
                    Debug.Log($"Hit enemy {hit.transform.name} with bat");
                    hit.transform.GetComponent<BaseEnemy>().Launch(reflectDir * hitPower);
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D coll = collision.collider;
        OnTriggerEnter2D(coll);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int layer = collision.gameObject.layer;
        Debug.Log($"{transform.name} collided with {collision.name}");
        HealthComponent health = GetComponent<HealthComponent>();
        if (layer == BulletHellCommon.BULLET_LAYER || layer == BulletHellCommon.ENEMY_LAYER)
        {
            if (health != null)
                health.TakeDamage();
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
