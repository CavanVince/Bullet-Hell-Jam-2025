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

    private GameObject[] bulletPylons;
    private Coroutine shootFuncCoroutine, shootCoroutine;
    private SpriteRenderer sr;

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

    IEnumerator Shoot(IEnumerator shootFunc)
    {
        bossState = BossState.ATTACKING;
        shootFuncCoroutine = StartCoroutine(shootFunc);
        yield return shootFuncCoroutine;
        bossState = BossState.MOVING;
        shootCoroutine = null;
    }

    void PhaseOne()
    {
        float distToPlayer = Vector2.Distance(player.transform.position, transform.position);

        switch(bossState)
        {
            case BossState.MOVING:
                if (distToPlayer <= closeRangeShootDistance)
                {
                    ShootParameters sp = new ShootParameters(
                        originCalculation:() => transform.position, 
                        destinationCalculation:() => player.transform.position, 
                        movementFunc:null, 
                        numBullets:6, 
                        pulseCount:3, 
                        pulseInterval_s:1f, 
                        cooldown:3f
                   );

                    sr.color = Color.yellow;
                    shootCoroutine = StartCoroutine(Shoot(ap.ShotgunShot(sp)));
                    // use a random close range attack pattern
                }
                else if (distToPlayer <= midRangeShootDistance)
                {
                    if (sr.color != orange)
                    {
                        sr.color = orange;
                    }
                    // use a random mid range attack pattern
                }
                else
                {
                    sr.color = Color.red;
                    // use a max range attack pattern
                }
                break;

        }
    }

    void PhaseTwo()
    {

    }

}
