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
public class BhossBhaby : BaseEntity
{

    public RoomGameObject OwningRoom;

    private BossState bossState;
    private BossPhase bossPhase;
    private int phaseTwoHealthAmount;

    [SerializeField]
    private float closeRangeShootDistance;
    [SerializeField]
    private float midRangeShootDistance;
    private bool isAggroed;
    [SerializeField]
    private float aggroRange = 15f;

    private PlayerController player;
    private Rigidbody2D rb;
    private AttackPatterns ap;

    Color orange = new Color(.255f, .127f, .80f);

    private GameObject enemySpawnPoints;

    private Coroutine shootFuncCoroutine, shootCoroutine;
    private SpriteRenderer sr;

    private IEnumerator[] phaseOneMoveset;
    private IEnumerator[] phaseTwoMoveset;


    // Start is called before the first frame update
    protected override void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        base.Start();
        phaseTwoHealthAmount = healthComponent.health / 2;
        bossPhase = BossPhase.ONE;
        player = PlayerController.Instance;
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = 1f;
        ap = new AttackPatterns(EntityManager.instance);

        phaseOneMoveset = new IEnumerator[] {  };
        phaseTwoMoveset = new IEnumerator[] {  };
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

    IEnumerator Shoot(IEnumerator[] shootFuncs)
    {
        bossState = BossState.ATTACKING;
        List<Coroutine> activeShootFuncRoutines = new List<Coroutine>();
        foreach(IEnumerator shootFunc in shootFuncs)
        {
            activeShootFuncRoutines.Add(StartCoroutine(shootFunc));
        }
        // wait for all to complete (aka join the threads)
        foreach(Coroutine routine in activeShootFuncRoutines)
        {
            yield return routine;
        }
        bossState = BossState.MOVING;
        shootCoroutine = null;
        shootFuncCoroutine = null;
    }

    IEnumerator AerialOnPlayer(bool randomScatterAfterSpawn=false)
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

                Vector2 randomDropAroundPlayer = (Vector2)player.transform.position + randomOffset;
                return randomDropAroundPlayer;
            },
            destinationCalculation: () => randomScatterAfterSpawn ? randomDropAroundPlayer : player.transform.position,
            movementFunc: null,
            numBullets: 1,
            pulseCount: 5,
            pulseInterval_s: .1f,
            cooldown: 0f
        );
        yield return ap.Aerial(sp);
    }

    IEnumerator ShotgunBurst(int numBursts=3)
    {
        ShootParameters sp = new ShootParameters(
            originCalculation: () => transform.position,
            destinationCalculation: () => player.transform.position,
            movementFunc: null,
            numBullets: 6,
            pulseCount: numBursts,
            pulseInterval_s: .1f,
            cooldown: 1f,
            bulletSpeed:8f
        );
        yield return ap.ShotgunShot(sp);
    }

    IEnumerator RadialFromBoss()
    {
        ShootParameters sp = new ShootParameters(
            originCalculation: () => transform.position,
            destinationCalculation: () => player.transform.position,
            movementFunc: null,
            numBullets: 8,
            pulseCount: 1,
            pulseInterval_s: .5f,
            cooldown: 0f,
            bulletSpeed: 8f
        );
        yield return ap.DivergingRadial(sp);

    }

    IEnumerator RapidShot()
    {
        ShootParameters sp = new ShootParameters(
            originCalculation: () => transform.position,
            destinationCalculation: () => player.transform.position,
            movementFunc: null,
            numBullets: 10,
            pulseCount: 1,
            pulseInterval_s: .2f,
            cooldown: 2f,
            bulletSpeed: 15f,
            bulletDistance:40f
        );
        yield return ap.Shoot(sp);
    }

    //IEnumerator SummonBombs()
    //{

    //}

    //void SummonTurrets()
    //{
    //    for (int i = 0; i < /*enemySpawnPoints.transform.childCount*/ 1; i++)
    //    {
    //        GameObject enemy = EntityManager.instance.SummonEnemy(typeof(TurretEnemy), enemySpawnPoints.transform.GetChild(i).transform.position, Quaternion.identity);
    //        enemy.GetComponent<BaseEnemy>().OwningRoom = this;
    //        enemyCount++;
    //    }
    //}

    void PhaseOne()
    {
        float distToPlayer = Vector2.Distance(player.transform.position, transform.position);

        switch(bossState)
        {
            case BossState.MOVING:
                if (distToPlayer <= closeRangeShootDistance)
                {

                    sr.color = Color.yellow;
                    StartCoroutine(Shoot(new IEnumerator[] { RapidShot() }));

                    // use a random close range attack pattern
                }
                else if (distToPlayer <= midRangeShootDistance)
                {
                    if (sr.color != orange)
                    {
                        sr.color = orange;
                    }
                    StartCoroutine(Shoot(new IEnumerator[] { AerialOnPlayer(true), RadialFromBoss() }));
                    // use a random mid range attack pattern
                }
                else
                {
                    sr.color = Color.red;
                    StartCoroutine(Shoot(new IEnumerator[] { RapidShot() }));

                    //StartCoroutine(Shoot(new IEnumerator[] { AerialOnPlayer(true)}));

                    // use a max range attack pattern
                }
                break;
        }
    }

    void PhaseTwo()
    {

    }

}
