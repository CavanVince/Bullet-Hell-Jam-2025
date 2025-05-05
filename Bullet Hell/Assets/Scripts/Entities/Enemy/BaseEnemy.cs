using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    protected PlayerController player;
    protected float shotTimerTracker = 0;
    protected int enemyHealth;

    [SerializeField]
    protected float shotTimer; // How long between shots

    [SerializeField]
    protected float shotDistance; // How far the enemy can be before it shoots
    public bool isLaunchable;
    private bool isLaunched;
    Rigidbody2D rb;
    private float timeAlive;
    private Vector2 moveDir;
    public float moveSpeed;
    
    public virtual void Launch(Vector2 direction)
    {
        if (!isLaunchable)
        {
            Debug.Log("Enemy not launchable");
            return;
        }
        isLaunched = true;
        Debug.Log("Enemy launched");
        moveDir = direction;
    }

    void Start()
    {
        // TODO: Needs to be made to not suck
        player = FindObjectOfType<PlayerController>();
        enemyHealth = 5;
        isLaunchable = true;
        isLaunched = false;
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = 1f;
    }

    void Update()
    {
        if (shotTimerTracker <= 0)
        {
            ShootPlayer();
            shotTimerTracker = shotTimer;
        }
        else
        {
            shotTimerTracker -= Time.deltaTime;
        }
    }
    void FixedUpdate()
    {
        if (!isLaunched)
        {
            rb.MovePosition((Vector2)transform.position + (moveDir * moveSpeed * Time.deltaTime));
            return;
        }
        rb.MovePosition((Vector2)transform.position + (moveDir * 1 * Time.deltaTime));
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        HealthComponent health = GetComponent<HealthComponent>();
        if (collision.gameObject.layer == 8) // Player Projectile layer
        {
            if (health != null)
            {
                health.TakeDamage();
            }
        }

    }

    /// <summary>
    /// Launches a bullet at the player if 
    /// </summary>
    protected virtual void ShootPlayer()
    {
        if (Vector2.Distance(player.transform.position, transform.position) < shotDistance)
        {
            BulletManager.instance.FireBullet(transform.position, (player.transform.position - transform.position).normalized);
        }
    }
}
