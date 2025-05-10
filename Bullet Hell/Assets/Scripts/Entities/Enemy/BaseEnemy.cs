using UnityEngine;


public enum EnemyState
{
    IDLE,
    LAUNCHED,
    DAZED,

    EXPLODING // only used for BombEnemy
}
public class BaseEnemy : BaseEntity
{
    public float maxLaunchDistance = .5f;
    public float moveSpeed;
    public bool isLaunchable = true;

    private bool isShooting;

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
    private AttackPatterns ap;

    protected override void Start()
    {
        ap = new AttackPatterns(EntityManager.instance);
        defaultMoveSpeed = moveSpeed;
        base.Start();
        enemyState = EnemyState.IDLE;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = 1f;
    }

    private void Update()
    {
        if (isShooting) return;
        // In aggro range
        if (Vector2.Distance(player.transform.position, transform.position) <= aggroRange)
        {
            isShooting = true;
            StartCoroutine(ap.Shoot(() => transform.position, () => player.transform.position, 3, .5f, 2, () => isShooting = false));
            // strafe and shoot at player as appropriate
        }
        else
        {
            // idk maybe just move around randomly or do nothin?
            // or see if nearby friendlies are aggro'd and join in
        }
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

    public virtual void Launch(Vector2 direction, float speed, int damage)
    {
        GetComponent<Animator>().SetInteger("animState", (int) enemyState);
        if (!isLaunchable)
        {
            return;
        }
        isLaunched = true;
        moveDir = direction;
        moveSpeed = speed;

        launchedFromPos = transform.position;
        launchDestination = launchedFromPos * moveDir * maxLaunchDistance;
        healthComponent.TakeDamage(damage);
    }

    protected virtual void OnAggro()
    {

    }

   

    

    /// <summary>
    /// Launches a bullet at the player if 
    /// </summary>
    protected virtual void ShootPlayer()
    {
        if (Vector2.Distance(player.transform.position, transform.position) < aggroRange)
        {
            EntityManager.instance.FireBullet(typeof(StandardBullet), transform.position, (player.transform.position - transform.position).normalized);
        }
    }
}
