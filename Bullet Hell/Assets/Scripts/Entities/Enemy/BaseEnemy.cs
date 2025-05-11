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
    protected AttackPatterns ap;

    private float pathCalcTime = 1;
    private float currentPathCalcTime;

    [HideInInspector]
    public Func<IEnumerator> shootFunc;
    private Coroutine shootRoutine, shootFuncRoutine;

    private Animator animator;


    protected override void Start()
    {
        base.Start();
        player = PlayerController.Instance;
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = 1f;
        path = new List<Vector2>();
        ap = new AttackPatterns(EntityManager.instance);
        animator = GetComponentInChildren<Animator>();

        ResetState();
    }

    protected IEnumerator Shoot()
    {
        enemyState = EnemyState.SHOOTING;
        shootFuncRoutine = StartCoroutine(shootFunc());
        yield return shootFuncRoutine;
        enemyState = EnemyState.IDLE;
        shootRoutine = null;
        shootFuncRoutine = null;
    }

    protected void StopShooting()
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
        if (enemyState == EnemyState.MOVING)
        {
            // idk maybe just move around randomly or do nothin?
            // or see if nearby friendlies are aggro'd and join in
            if (pathCalcTime <= currentPathCalcTime)
                PathToPlayer();
            else
                currentPathCalcTime += Time.deltaTime;
        }
        else 
        {
            animator.SetFloat("Input X", (player.transform.position - transform.GetChild(0).transform.position).normalized.x);
            animator.SetFloat("Input Y", (player.transform.position - transform.GetChild(0).transform.position).normalized.y);
            animator.SetBool("IsMoving", false);
        }

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
        else if ((enemyState == EnemyState.IDLE && Vector2.Distance(player.transform.position, transform.position) <= aggroRange))
        {
            enemyState = EnemyState.MOVING;
        }
    }

    protected IEnumerator Dazed(float duration_s)
    {
        enemyState = EnemyState.DAZED;
        yield return new WaitForSeconds(duration_s);
        enemyState = EnemyState.IDLE;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == BulletHellCommon.WALL_LAYER || collision.gameObject.layer == BulletHellCommon.BREAKABLE_LAYER)
        {
            if (enemyState == EnemyState.LAUNCHED)
            {
                launchDestination = transform.position;
                healthComponent.TakeDamage(3);
                StartCoroutine(Dazed(dazeDurationS));
            }
            else if (enemyState == EnemyState.MOVING && currentPathCalcTime < pathCalcTime)
            {
                currentPathCalcTime += Time.deltaTime;
            }
        }
        base.OnTriggerEnter2D(collision);
    }

    protected void FixedUpdate()
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

            // Have enemy face player
            animator.SetFloat("Input X", (newPos - (Vector2)transform.GetChild(0).transform.position).normalized.x);
            animator.SetFloat("Input Y", (newPos - (Vector2)transform.GetChild(0).transform.position).normalized.y);
            animator.SetBool("IsMoving", true);

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
        currentPathCalcTime = 0;
        enemyState = EnemyState.MOVING;
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
    protected Vector3Int ConvertToRoomSpace(Vector2 pos)
    {
        return OwningRoom.GetComponentInParent<Grid>().WorldToCell(pos);
    }

    /// <summary>
    /// Helper function to convert a tile position to world space
    /// </summary>
    /// <param name="pos">The position of the tile on the grid</param>
    /// <returns></returns>
    protected Vector3 ConvertToWorldSpace(Vector3Int pos)
    {
        return OwningRoom.GetComponentInParent<Grid>().CellToWorld(pos);
    }

    public override void ResetState()
    {
        base.ResetState();
        currentPathCalcTime = pathCalcTime;
        enemyState = EnemyState.IDLE;
    }
}
