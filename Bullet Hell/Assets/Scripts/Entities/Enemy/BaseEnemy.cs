using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    protected PlayerController player;
    protected float shotTimerTracker = 0;
    protected int enemyHealth;

    public bool takesFriendlyFireAerialBulletDamage;

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
        Collider2D coll = collision.collider;
        OnTriggerEnter2D(coll);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log($"{transform.name} collided with {collision.name}");
        HealthComponent health = GetComponent<HealthComponent>();
        if (health == null)
        {
            Debug.Log("WARNING: NO HEALTH ATTACHED TO ENEMY.");
            return;
        }

        bool isHostileBullet = collision.transform.gameObject.layer == BulletHellCommon.PLAYER_PROJECTILE_LAYER;
        bool isAerialBullet = collision.GetComponentInParent<BaseBullet>() && collision.GetComponentInParent<BaseBullet>().GetType() == typeof(AerialBullet);
        if (isHostileBullet || (takesFriendlyFireAerialBulletDamage && isAerialBullet))
        {
            health.TakeDamage();
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
