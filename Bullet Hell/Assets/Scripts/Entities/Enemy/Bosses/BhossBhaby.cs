using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BossState
{
    ATTACKING,
    MOVING,
    DYING
}

public enum BossPhase
{
    ONE,
    TWO
}

public enum Distance
{
    CLOSE,
    MID,
    FAR
}
public class BhossBhaby : BaseEntity
{
    public RoomGameObject OwningRoom;
    private PlayerController player;
    private Rigidbody2D rb;
    private AttackPatterns ap;

    [SerializeField]
    private GameObject enemySpawnPoints;
    private SpriteRenderer sr;

    private BossState bossState;
    private BossPhase bossPhase;
    private int phaseTwoHealthAmount;

    [SerializeField]
    private float closeRangeShootDistance;
    [SerializeField]
    private float midRangeShootDistance;
    private bool isAggroed;
    [SerializeField]
    private float aggroRange = 20f;

    private Dictionary<Distance, AttackSet[]> phaseOneMoveset;
    private IEnumerator[] phaseTwoMoveset;

    private List<Coroutine> activeShootCoroutines;

    public GameObject enemyPrefab;

    struct AttackSet
    {
        public AttackSet(Func<IEnumerator>[] moves, float duration, float probability)
        {
            this.moves = moves;
            this.duration = duration;
            this.probability = probability;
        }

        public Func<IEnumerator>[] moves;
        public float duration;
        public float probability;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        activeShootCoroutines = new List<Coroutine>();

        sr = GetComponentInChildren<SpriteRenderer>();
        base.Start();
        phaseTwoHealthAmount = healthComponent.health / 2;
        bossPhase = BossPhase.ONE;
        player = PlayerController.Instance;
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = 1f;
        ap = new AttackPatterns(EntityManager.instance);

        phaseOneMoveset = new Dictionary<Distance, AttackSet[]>
        {
            {
                Distance.CLOSE, new AttackSet[]
                { 
                    // IMPLEMENT ME

                    // shotgun burst for 6 seconds
                    new AttackSet(new Func<IEnumerator>[] { () => AerialOnPlayer(true, shotsPerSecond: 8), () => RadialFromBoss(bulletDistance: 100f, bulletSpeed: 8), () => RandomTargetRapidShot(10, bulletSpeed:5f), () => ShotgunBurst(bulletDistance: 100f), () => SpawnEnemiesAfterDelay(NearestSpawnPoint(player.transform.position), 4f, 8) }, 8f, 1f),
                }
            },
            {
                Distance.MID, new AttackSet[]
                { 
                    // IMPLEMENT ME

                    // aerials on ground around player, radial from boss for 8 seconds
                    new AttackSet(new Func<IEnumerator>[] { () => AerialOnPlayer(true, shotsPerSecond: 8), () => RadialFromBoss(bulletDistance: 100f, bulletSpeed: 8), () => RandomTargetRapidShot(10, bulletSpeed:5f), () => ShotgunBurst(bulletDistance: 100f), () => SpawnEnemiesAfterDelay(NearestSpawnPoint(player.transform.position), 4f, 8) }, 8f, 1f),
                }
            },
            {
                Distance.FAR, new AttackSet[] 
                // aerials on ground, long range aerial, and rapid fire at player for 8 seconds

                // the parameters here are AttackSet array, duration of pattern, probability of being selected
                // note that each IEnumerator 'shoot function' has its own set of optional parameters, like setting the number of bullets per sec, bullet speed, bullet distance, cooldown after completion, etc
                {
                    // FIX ME
                    new AttackSet(new Func<IEnumerator>[] { () => AerialOnPlayer(true, shotsPerSecond: 8), () => RadialFromBoss(bulletDistance: 100f, bulletSpeed: 8), () => RandomTargetRapidShot(10, bulletSpeed:5f), () => ShotgunBurst(bulletDistance: 100f), () => SpawnEnemiesAfterDelay(NearestSpawnPoint(player.transform.position), 4f, 6) }, 8f, 1f),
                    //new AttackSet(new Func<IEnumerator>[] { () => ShotgunBurst() }, 6f, 1f),
                    //new AttackSet(new Func<IEnumerator>[] { () => AerialOnPlayer(true) }, 3f, .3f),
                    //new AttackSet(new Func<IEnumerator>[] { () => RadialFromBoss(3, bulletSpeed:25f, bulletDistance:30f)}, 3f, .5f),
                    //new AttackSet(new Func<IEnumerator>[] { () => RapidShot(10, bulletSpeed:25f) }, 3f, 0f),
                  // new AttackSet(new Func<IEnumerator>[] { () => AerialOnPlayer(true), () => RadialFromBoss(3, bulletSpeed: 25f, bulletDistance:20f), () => RapidShot(10, bulletSpeed:25f) }, 8f, 0f),
                    //new AttackSet(new Func<IEnumerator>[] { () => SpawnEnemiesAfterDelay(NearestSpawnPoint(player.transform.position), 0f, 4) }, 1f, .2f )

                   // new AttackSet(new Func<IEnumerator>[] { () => LockemUp(), () => SpawnEnemiesAfterDelay(NearestSpawnPoint(player.transform.position), 4f, 4), () => RapidShot() }, 10f, .3f),
                }
            }
        };

        // IMPLEMENT ME
        //phaseTwoMoveset = new Dictionary<Distance, AttackSet[]>;
    }

    AttackSet GetWeightedRandomAttackSet(AttackSet[] attackSets)
    {
        float rand = UnityEngine.Random.value;
        float currentProb = 0;

        foreach (AttackSet attackSet in attackSets)
        {
            currentProb += attackSet.probability;
            if (rand <= currentProb)
            {
                return attackSet;
            }
        }

        throw new Exception("All probabilities in AttackSet must sum to 1");
    }

    // get the spawn point in the room that is closest to the player. Useful for summoning turrets / enemies / etc
    GameObject NearestSpawnPoint(Vector2 position)
    {
        GameObject closest = null;
        float distance = 999;
        for (int i = 0; i < enemySpawnPoints.transform.childCount; i++)
        {
            GameObject newObj = enemySpawnPoints.transform.GetChild(i).gameObject;
            float newDist = Vector2.Distance(newObj.transform.position, position);
            if (newDist < distance)
            {
                closest = newObj;
                distance = newDist;
            }
        }
        return closest;
    }

    // note that this is a NESTED IEnumerator being returned
    private IEnumerator SpawnEnemiesAfterDelay(GameObject spawnPoint, float delay, int enemies, float cooldown = 6f)
    {
        transform.GetChild(0).GetComponent<Animator>().SetBool("IsAttacking", true);
        IEnumerator DoSpawn()
        {
            yield return new WaitForSeconds(delay);
            for (int i = 0; i < enemies; i++)
            {
                GameObject enemy = EntityManager.instance.SummonEnemy(typeof(ShotgunEnemy), spawnPoint.transform.position, Quaternion.identity);
                enemy.GetComponent<BaseEnemy>().OwningRoom = OwningRoom;
                enemy.GetComponent<BaseEnemy>().aggroRange = 30;
            }
            yield return new WaitForSeconds(cooldown);
        }

        transform.GetChild(0).GetComponent<Animator>().SetBool("IsAttacking", false);
        yield return DoSpawn();
    }

    // Call this to stop shooting immediately
    void StopShooting()
    {
        foreach (Coroutine co in activeShootCoroutines)
        {
            StopCoroutine(co);
        }
        activeShootCoroutines.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = transform.GetChild(0).localPosition;
        newPos.y = MathF.Sin(Time.time * 0.5f);
        transform.GetChild(0).localPosition = newPos;

        if (!isAggroed)
        {
            if (Vector2.Distance(player.gameObject.transform.position, transform.position) <= aggroRange)
            {
                isAggroed = true;
                bossState = BossState.MOVING;
                bossPhase = BossPhase.ONE;
            }
            else
            {
                return;
            }
        }
        switch (bossPhase)
        {
            case BossPhase.ONE:
                if (healthComponent.health <= phaseTwoHealthAmount)
                {
                    Debug.Log("Entered phase 2");
                    bossPhase = BossPhase.TWO;
                }
                else
                    PhaseOne();
                break;

            case BossPhase.TWO:
                PhaseTwo();
                break;
        }
    }


    /// <summary>
    /// Call this to start the boss shooting for 'duration' seconds. Each of the shoot functions will loop until duration is hit
    /// </summary>
    /// <param name="shootFuncs"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    IEnumerator Shoot(Func<IEnumerator>[] shootFuncs, float duration = 10f)
    {
        IEnumerator _ShootForDuration(Func<IEnumerator> f, float t)
        {
            float elapsedTime = 0;
            float startTime = Time.time;
            while (Time.time - startTime < t)
            {
                Coroutine co = StartCoroutine(f());
                activeShootCoroutines.Add(co);
                yield return co;
                elapsedTime += Time.deltaTime;
            }
        }

        bossState = BossState.ATTACKING;
        List<Coroutine> activeShootFuncRoutines = new List<Coroutine>();

        foreach (Func<IEnumerator> shootFunc in shootFuncs)
        {
            activeShootFuncRoutines.Add(StartCoroutine(_ShootForDuration(shootFunc, duration)));
        }

        // wait for all to complete (aka join the threads)
        foreach (Coroutine routine in activeShootFuncRoutines)
        {
            yield return routine;
        }

        activeShootCoroutines.Clear();
        bossState = BossState.MOVING;
    }

    IEnumerator AerialOnPlayer(bool randomScatterAfterSpawn = false, int shotsPerSecond = 15)
    {
        Vector2 randomOffset = new Vector2(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized * UnityEngine.Random.Range(0f, 2f);

        Vector2 randomDropAroundPlayer = (Vector2)player.transform.position + randomOffset;


        ShootParameters sp = new ShootParameters(
            originCalculation: () =>
            {
                if (!randomScatterAfterSpawn) return player.transform.position;

                Vector2 randomOffset = new Vector2(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-1f, 1f)
                ).normalized * UnityEngine.Random.Range(0f, 4f);
                Vector2 randomDropAroundPlayer = (Vector2)player.transform.position + randomOffset + (player.moveDir * 2);
                return randomDropAroundPlayer;
            },
            destinationCalculation: () => randomScatterAfterSpawn ? randomDropAroundPlayer : player.transform.position,
            movementFunc: null,
            numBullets: 1,
            pulseCount: shotsPerSecond,
            pulseInterval_s: 1f / shotsPerSecond,
            cooldown: 0f
        );
        yield return ap.Aerial(sp);
    }

    IEnumerator LockemUp()
    {
        yield return null;
    }

    IEnumerator ShotgunBurst(int shotsPerSecond = 3, float cooldown = 2f, float bulletDistance = 30, float bulletSpeed = 8f)
    {
        ShootParameters sp = new ShootParameters(
            originCalculation: () => transform.position,
            destinationCalculation: () => player.transform.position,
            movementFunc: null,
            numBullets: 6,
            pulseCount: shotsPerSecond,
            pulseInterval_s: 1f / shotsPerSecond,
            cooldown: cooldown,
            bulletSpeed: 8f,
            bulletDistance: bulletDistance

        );
        yield return ap.ShotgunShot(sp);
    }

    IEnumerator RadialFromBoss(int shotsPerSecond = 3, float bulletSpeed = 15f, float bulletDistance = 10f, float cooldown = 0f)
    {
        ShootParameters sp = new ShootParameters(
            originCalculation: () => transform.position,
            destinationCalculation: () => player.transform.position,
            movementFunc: null,
            numBullets: 8,
            pulseCount: shotsPerSecond,
            pulseInterval_s: 1f / shotsPerSecond,
            cooldown: cooldown,
            bulletSpeed: bulletSpeed,
            bulletDistance: bulletDistance
        );
        yield return ap.DaOctopus(sp);

    }

    IEnumerator RapidShot(int shotsPerSecond = 5, float bulletSpeed = 15f, float cooldown = 0f)
    {
        ShootParameters sp = new ShootParameters(
            originCalculation: () => transform.position,
            destinationCalculation: () => player.transform.position,
            movementFunc: null,
            pulseCount: shotsPerSecond,
            pulseInterval_s: 1f / shotsPerSecond,
            cooldown: cooldown,
            bulletSpeed: bulletSpeed,
            bulletDistance: 40f
        );
        yield return ap.Shoot(sp);
    }

    // TODO: KEVIN IMPLEMENT ME
    IEnumerator RandomTargetRapidShot(int shotsPerSecond = 5, float bulletSpeed = 15f, float cooldown = 0f)
    {
        // this one should do basically the same thing as the rapid shot, but have a randomly determined target somewhat around the player
        // (refer to the AerialOnPlayer function above to see how that one calculates a random scatter around the player),
        // specifically 'originCalculation' but it would instead be used to determine the destinationCalculation.

        // FIXME
        ShootParameters sp = new ShootParameters(
            originCalculation: () => transform.position,
            destinationCalculation: () => 
            {

                Vector2 randomOffset = new Vector2(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-1f, 1f)
                ).normalized * UnityEngine.Random.Range(0f, 6f);
                Vector2 randomDropAroundPlayer = (Vector2)player.transform.position + randomOffset + (player.moveDir * 2);
                return randomDropAroundPlayer;
            },
            movementFunc: null,
            pulseCount: shotsPerSecond,
            pulseInterval_s: 1f / shotsPerSecond,
            cooldown: cooldown,
            bulletSpeed: bulletSpeed,
            bulletDistance: 40f
        );
        yield return ap.Shoot(sp);
    }

    void PhaseOne()
    {
        float distToPlayer = Vector2.Distance(player.transform.position, transform.position);

        switch (bossState)
        {
            case BossState.MOVING:
                AttackSet[] possibleAttacks;
                AttackSet selectedAttackSet;
                if (distToPlayer <= closeRangeShootDistance)
                {
                    possibleAttacks = phaseOneMoveset[Distance.CLOSE];
                }
                else if (distToPlayer <= midRangeShootDistance)
                {
                    possibleAttacks = phaseOneMoveset[Distance.MID];
                }
                else
                {
                    possibleAttacks = phaseOneMoveset[Distance.FAR];
                }
                // selects a random attack set being mindful of their probability weights.
                selectedAttackSet = GetWeightedRandomAttackSet(possibleAttacks);
                StartCoroutine(Shoot(selectedAttackSet.moves, selectedAttackSet.duration));

                break;
        }
    }

    void PhaseTwo()
    {
        PhaseOne();
    }

}
