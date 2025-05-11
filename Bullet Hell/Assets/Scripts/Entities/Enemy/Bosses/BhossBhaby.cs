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
        public AttackSet(Func<IEnumerator>[] moves, float duration)
        {
            this.moves = moves;
            this.duration = duration;
        }

        public Func<IEnumerator>[] moves;
        public float duration;
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
                    // shotgun burst for 6 seconds
                    new AttackSet(new Func<IEnumerator>[] { () => ShotgunBurst() }, 6f) 
                } 
            },
            { 
                Distance.MID, new AttackSet[] 
                { 
                    // aerials on ground around player, radial from boss for 8 seconds
                    new AttackSet(new Func<IEnumerator>[] { () => AerialOnPlayer(true), () => RadialFromBoss() }, 8f) 
                } 
            },
            { 
                Distance.FAR, new AttackSet[] 
                // aerials on ground, long range aerial, and rapid fire at player for 8 seconds
                { 
                    //new AttackSet(new Func<IEnumerator>[] { () => AerialOnPlayer(true), () => RadialFromBoss(), () => RapidShot() }, 8f),
                    new AttackSet(new Func<IEnumerator>[] { () => LockemUp(), () => SpawnEnemiesAfterDelay(NearestSpawnPoint(player.transform.position), 4f, 4), () => RapidShot() }, 10f),
                } 
            }
        };
        phaseTwoMoveset = new IEnumerator[] {  };
    }

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

    private IEnumerator SpawnEnemiesAfterDelay(GameObject spawnPoint, float delay, int enemies, float cooldown=6f)
    {
        IEnumerator DoSpawn() {
            yield return new WaitForSeconds(delay);
            for (int i = 0; i < enemies; i++)
            {
                GameObject enemy = EntityManager.instance.SummonEnemy(typeof(BaseEnemy), spawnPoint.transform.position, Quaternion.identity);
                enemy.GetComponent<BaseEnemy>().OwningRoom = OwningRoom;
            }
            yield return new WaitForSeconds(cooldown);
        }
        yield return DoSpawn();
    }

    void StopShooting()
    {
        foreach(Coroutine co in activeShootCoroutines)
        {
            StopCoroutine(co);
        }
        activeShootCoroutines.Clear();
    }

    // Update is called once per frame
    void Update()
    {
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
        switch(bossPhase)
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


    IEnumerator Shoot(Func<IEnumerator>[] shootFuncs, float duration=10f)
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


        Debug.Log($"Running shoot coroutine for {duration} seconds");

        bossState = BossState.ATTACKING;
        List<Coroutine> activeShootFuncRoutines = new List<Coroutine>();

        foreach(Func<IEnumerator> shootFunc in shootFuncs)
        {
            activeShootFuncRoutines.Add(StartCoroutine(_ShootForDuration(shootFunc, duration)));
        }

        // wait for all to complete (aka join the threads)
        foreach(Coroutine routine in activeShootFuncRoutines)
        {
            yield return routine;
        }

        activeShootCoroutines.Clear();
        bossState = BossState.MOVING;
    }

    IEnumerator AerialOnPlayer(bool randomScatterAfterSpawn=false, int shotsPerSecond=15)
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
            pulseInterval_s: 1f/shotsPerSecond,
            cooldown: 0f
        );
        yield return ap.Aerial(sp);
    }

    IEnumerator LockemUp()
    {
        yield return null;
    }

    IEnumerator ShotgunBurst(int shotsPerSecond=3, float cooldown=2f)
    {
        ShootParameters sp = new ShootParameters(
            originCalculation: () => transform.position,
            destinationCalculation: () => player.transform.position,
            movementFunc: null,
            numBullets: 6,
            pulseCount: shotsPerSecond,
            pulseInterval_s: 1f/shotsPerSecond,
            cooldown: cooldown,
            bulletSpeed:8f
        );
        yield return ap.ShotgunShot(sp);
    }

    IEnumerator RadialFromBoss(int shotsPerSecond=3, float cooldown=0f)
    {
        ShootParameters sp = new ShootParameters(
            originCalculation: () => transform.position,
            destinationCalculation: () => player.transform.position,
            movementFunc: null,
            numBullets: 8,
            pulseCount: shotsPerSecond,
            pulseInterval_s: 1f/shotsPerSecond,
            cooldown: cooldown,
            bulletSpeed: 8f
        );
        yield return ap.DivergingRadial(sp);

    }

    IEnumerator RapidShot(int shotsPerSecond=5, float cooldown=0f)
    {
        ShootParameters sp = new ShootParameters(
            originCalculation: () => transform.position,
            destinationCalculation: () => player.transform.position,
            movementFunc: null,
            pulseCount: shotsPerSecond,
            pulseInterval_s: 1f/shotsPerSecond,
            cooldown: cooldown,
            bulletSpeed: 15f,
            bulletDistance:40f
        );
        yield return ap.Shoot(sp);
    }

    void PhaseOne()
    {
        float distToPlayer = Vector2.Distance(player.transform.position, transform.position);

        switch(bossState)
        {
            case BossState.MOVING:
                AttackSet[] possibleAttacks;
                AttackSet selectedAttackSet;
                if (distToPlayer <= closeRangeShootDistance)
                {
                    sr.color = Color.yellow;
                    possibleAttacks = phaseOneMoveset[Distance.CLOSE];
                }
                else if (distToPlayer <= midRangeShootDistance)
                {
                    sr.color = Color.blue;
                    possibleAttacks = phaseOneMoveset[Distance.MID];
                }
                else
                {
                    sr.color = Color.red;
                    possibleAttacks = phaseOneMoveset[Distance.FAR];
                }
                selectedAttackSet = possibleAttacks[UnityEngine.Random.Range(0, possibleAttacks.Length)];
                StartCoroutine(Shoot(selectedAttackSet.moves, selectedAttackSet.duration));

                break;
        }
    }

    void PhaseTwo()
    {

    }

}
