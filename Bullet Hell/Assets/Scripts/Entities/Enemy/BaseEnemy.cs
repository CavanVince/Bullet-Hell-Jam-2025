using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public enum EnemyState
{
    IDLE,
    MOVING,
    LAUNCHED,
    DAZED,
    SHOOTING,
    EXPLODING // only used for BombEnemy
}
public class BaseEnemy : BaseEntity
{
    public float maxLaunchDistance = .5f;
    public bool isLaunchable = true;
    public RoomGameObject OwningRoom;

    private bool isShooting;

    [SerializeField]
    protected float aggroRange;
    protected EnemyState enemyState;
    protected Vector2 moveDir;
    protected bool isLaunched;
    protected List<Vector2> path;
    [HideInInspector]
    public PlayerController player;

    private Vector2 launchedFromPos;
    private Vector2 launchDestination;
    private Rigidbody2D rb;
    private AttackPatterns ap;

    [HideInInspector]
    public Func<IEnumerator> shootFunc;

    protected override void Start()
    {
        base.Start();
        player = PlayerController.Instance;
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = 1f;
        path = new List<Vector2>();
        enemyState = EnemyState.MOVING;
        ap = new AttackPatterns(EntityManager.instance);

        ResetState();
    }

    IEnumerator Shoot()
    {
        isShooting = true;
        yield return StartCoroutine(shootFunc());
        isShooting = false;
    }

    protected virtual void Update()
    {
        if (isShooting)
        {
            return;
        }
        // In aggro range
        // TODO: Add raycast check to make sure player is visible
        if (Vector2.Distance(player.transform.position, transform.position) <= aggroRange)
        {
            enemyState = EnemyState.SHOOTING;
            isShooting = true;
            path.Clear();
            StartCoroutine(Shoot());
            // strafe and shoot at player as appropriate
        }
        else
        {
            // idk maybe just move around randomly or do nothin?
            // or see if nearby friendlies are aggro'd and join in
            if (enemyState == EnemyState.MOVING && path.Count <= 0)
            {
                PathToPlayer();
            }
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
        else if (enemyState == EnemyState.MOVING && path.Count > 0)
        {
            Vector2 enemyPos = (Vector2)transform.position;
            Vector2 newPos = OwningRoom.GetComponentInParent<Grid>().CellToWorld(new Vector3Int((int)path[0].x, (int)path[0].y, 0));

            //rb.MovePosition(enemyPos + ((enemyPos - newPos).normalized * moveSpeed * Time.deltaTime));

            if (Vector2.Distance(enemyPos, newPos) <= 0.25f)
            {
                path.RemoveAt(0);
            }
        }
    }

    public virtual void Launch(Vector2 direction, float speed, int damage)
    {
        GetComponent<Animator>().SetInteger("animState", (int)enemyState);
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

    /// <summary>
    /// Calculate the shortest path to the player's position
    /// </summary>
    protected virtual void PathToPlayer()
    {
        path.Clear();

        Vector3Int enemyPos = ConvertToRoomSpace(transform.position);
        Vector3Int playerPos = ConvertToRoomSpace(player.transform.position);

        Pathfinding.CalculatePath(
            new int2(enemyPos.x, enemyPos.y),
            new int2(playerPos.x, playerPos.y),
            new int2(OwningRoom.GridBounds.size.x, OwningRoom.GridBounds.size.y),
            OwningRoom.walkabilityGrid,
            this);
    }

    /// <summary>
    /// Helper function to assign the path once it has been calculated by the pathfinding manager
    /// </summary>
    /// <param name="newPath"></param>
    public void SetPath(NativeList<int2> newPath)
    {
        for (int i = 0; i < newPath.Length; i++)
        {
            path.Add(new Vector2(newPath[i].x, newPath[i].y));
        }
        newPath.Dispose();
    }

    /// <summary>
    /// Helper function to convert a position to room space
    /// </summary>
    /// <param name="pos">The position in room space</param>
    /// <returns></returns>
    private Vector3Int ConvertToRoomSpace(Vector2 pos)
    {
        return OwningRoom.GetComponentInParent<Grid>().WorldToCell(pos);
    }

    public override void ResetState()
    {
        base.ResetState();
        enemyState = EnemyState.IDLE;
        isLaunched = false;
        isShooting = false;
        shootFunc = () => ap.Shoot(new ShootParameters(originCalculation:() => transform.position, destinationCalculation:() => player.transform.position));
    }
}
