using UnityEngine;


public enum EnemyState
{
    IDLE,
    LAUNCHED,
    DAZED
}
public class BaseEnemy : BaseEntity
{
    public float maxLaunchDistance = .5f;
    public float moveSpeed;
    public bool isLaunchable = true;

    [SerializeField]
    protected float aggroRange;
    protected EnemyState enemyState;
    protected Vector2 moveDir;
    protected bool isLaunched;
    protected PlayerController player;

    private float defaultMoveSpeed;
    private Vector2 launchedFromPos;
    private Vector2 launchDestination;
    private Rigidbody2D rb;

    
    public virtual void Launch(Vector2 direction, float speed, int damage)
    {
        if (!isLaunchable)
        {
            Debug.Log("Enemy not launchable");
            return;
        }
        isLaunched = true;
        Debug.Log("Enemy launched");
        Debug.Log($"prevSpeed:{moveSpeed},speed:{speed},dir{direction}");
        moveDir = direction;
        moveSpeed = speed;

        launchedFromPos = transform.position;
        launchDestination = launchedFromPos * moveDir * maxLaunchDistance;
        healthComponent.TakeDamage(damage);
    }

     protected new void Start()
    {
        defaultMoveSpeed = moveSpeed;
        base.Start();
        enemyState = EnemyState.IDLE;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = 1f;
    }

    void FixedUpdate()
    {
        if (isLaunched)
        {
            bool beyondLaunchTarget = Vector2.Distance(launchDestination, launchedFromPos) <= Vector2.Distance(transform.position, launchedFromPos);
            if (!beyondLaunchTarget)
            {
                rb.MovePosition((Vector2)transform.position + (moveDir * moveSpeed * Time.deltaTime));
            }
            else
            {
                isLaunched = false;
                moveSpeed = defaultMoveSpeed;
            }
        }
    }

    /// <summary>
    /// Launches a bullet at the player if 
    /// </summary>
    protected virtual void ShootPlayer()
    {
        if (Vector2.Distance(player.transform.position, transform.position) < aggroRange)
        {
            BulletManager.instance.FireBullet(transform.position, (player.transform.position - transform.position).normalized);
        }
    }
}
