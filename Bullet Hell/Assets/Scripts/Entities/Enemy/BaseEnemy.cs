using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using System;
using System.Collections;

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

    private float dazeDurationS = 1f;

    [SerializeField]
    protected float aggroRange;
    [SerializeField]
    protected float shootRange;

    protected EnemyState enemyState;
    protected Vector2 moveDir;
    protected List<Vector2> path;
    [HideInInspector]
    public PlayerController player;

    private Vector2 launchedFromPos;
    private Vector2 launchDestination;
    private Rigidbody2D rb;
    private AttackPatterns ap;

    private float pathCalcTime = 1;
    private float currentPathCalcTime;

    [HideInInspector]
    public Func<IEnumerator> shootFunc;

    private Coroutine shootRoutine, shootFuncRoutine;

    protected override void Start()
    {
        base.Start();
        player = PlayerController.Instance;
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = 1f;
        path = new List<Vector2>();
        ap = new AttackPatterns(EntityManager.instance);

        ResetState();
    }

    IEnumerator Shoot()
    {
        enemyState = EnemyState.SHOOTING;
        shootFuncRoutine = StartCoroutine(shootFunc());
        yield return shootFuncRoutine;
        enemyState = EnemyState.IDLE;
        shootRoutine = null;
    }

    void StopShooting()
    {
        if (shootFuncRoutine != null)
        {
            StopCoroutine(shootFuncRoutine);
        }
        if (shootRoutine != null)
        {
            StopCoroutine(shootRoutine);
        }
    }

    protected virtual void Update()
    {
        // was shooting but now out of range
        if (enemyState == EnemyState.SHOOTING && Vector2.Distance(player.transform.position, transform.position) > shootRange)
        {
            if (shootFuncRoutine != null)
            {
                // is still finishing its shot pattern, keep shooting until complete
                return;
            }
            enemyState = EnemyState.IDLE;
            return;
        }
        // is busy doing something
        else if (enemyState == EnemyState.SHOOTING || enemyState == EnemyState.LAUNCHED || enemyState == EnemyState.DAZED)
        {
            return;
        }
        // In aggro range
        // TODO: Add raycast check to make sure player is visible
        else if (Vector2.Distance(player.transform.position, transform.position) <= shootRange)
        {
            path.Clear();
            shootRoutine = StartCoroutine(Shoot());
            // strafe and shoot at player as appropriate
        }
        else if ((enemyState == EnemyState.MOVING && path.Count <= 0) ||
                (enemyState == EnemyState.IDLE && Vector2.Distance(player.transform.position, transform.position) <= aggroRange))
        {
<<<<<<< Updated upstream
            // idk maybe just move around randomly or do nothin?
            // or see if nearby friendlies are aggro'd and join in
            if (enemyState == EnemyState.MOVING && pathCalcTime <= currentPathCalcTime)
=======
            PathToPlayer();
        }
    }

    IEnumerator Dazed(float duration_s)
    {
        enemyState = EnemyState.DAZED;
        yield return new WaitForSeconds(duration_s);
        enemyState = EnemyState.IDLE;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == BulletHellCommon.WALL_LAYER)
        {
            if (enemyState == EnemyState.LAUNCHED)
>>>>>>> Stashed changes
            {
                launchDestination = transform.position;
                StartCoroutine(Dazed(dazeDurationS));
            }
            else if (enemyState == EnemyState.MOVING && currentPathCalcTime < pathCalcTime)
            {
                currentPathCalcTime += Time.deltaTime;
            }
        }
        base.OnTriggerEnter2D(collision);
    }

    void FixedUpdate()
    {
        if (enemyState == EnemyState.LAUNCHED)
        {
            bool beyondLaunchTarget = Vector2.Distance(launchDestination, launchedFromPos) <= Vector2.Distance(transform.position, launchedFromPos);
            if (!beyondLaunchTarget)
            {
                rb.MovePosition((Vector2)transform.position + (moveDir * moveSpeed * Time.deltaTime));
            }
            else
            {
                StartCoroutine(Dazed(dazeDurationS));
            }
        }
        else if (enemyState == EnemyState.MOVING && path.Count > 0)
        {
            Vector2 enemyPos = (Vector2)transform.position;
            Vector2 newPos = ConvertToWorldSpace(new Vector3Int((int)path[0].x, (int)path[0].y, 0));

            rb.MovePosition(enemyPos + ((newPos - enemyPos).normalized * moveSpeed * Time.deltaTime));

            if (Vector2.Distance(enemyPos, newPos) <= 0.25f)
            {
                path.RemoveAt(0);
            }
        }
    }

    public virtual void Launch(Vector2 direction, float speed, int damage)
    {
        if (!isLaunchable)
        {
            return;
        }

        StopShooting();

        enemyState = EnemyState.LAUNCHED;
        moveDir = direction;
        moveSpeed = speed;

        launchedFromPos = transform.position;
        launchDestination = launchedFromPos * moveDir * maxLaunchDistance;
        healthComponent.TakeDamage(damage);

    }

<<<<<<< Updated upstream
    protected virtual void OnAggro()
    {

    }
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        isLaunched = false;
        int layer = collision.gameObject.layer;
        if (layer == BulletHellCommon.WALL_LAYER || layer == BulletHellCommon.BREAKABLE_LAYER)
        {
           healthComponent.TakeDamage();
        }
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

=======
>>>>>>> Stashed changes
    /// <summary>
    /// Calculate the shortest path to the player's position
    /// </summary>
    protected virtual void PathToPlayer()
    {
        path.Clear();
<<<<<<< Updated upstream
        currentPathCalcTime = 0;

=======
        enemyState = EnemyState.MOVING;
>>>>>>> Stashed changes
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
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.positionCount = 0;
        lr.positionCount = newPath.Length - 1;
        for (int i = 1; i < newPath.Length; i++) // Exclude first point (starting point)
        {
            path.Add(new Vector2(newPath[i].x, newPath[i].y));
            lr.SetPosition(i - 1, ConvertToWorldSpace(new Vector3Int(newPath[i].x, newPath[i].y)));
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

    /// <summary>
    /// Helper function to convert a tile position to world space
    /// </summary>
    /// <param name="pos">The position of the tile on the grid</param>
    /// <returns></returns>
    private Vector3 ConvertToWorldSpace(Vector3Int pos)
    {
        return OwningRoom.GetComponentInParent<Grid>().CellToWorld(pos);
    }

    public override void ResetState()
    {
        base.ResetState();
<<<<<<< Updated upstream
        enemyState = EnemyState.MOVING;
        currentPathCalcTime = pathCalcTime;
        isLaunched = false;
        isShooting = false;
=======
        enemyState = EnemyState.IDLE;
>>>>>>> Stashed changes
        shootFunc = () => ap.Shoot(new ShootParameters(originCalculation: () => transform.position, destinationCalculation: () => player.transform.position));

    }
}
