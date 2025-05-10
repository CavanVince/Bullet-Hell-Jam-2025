using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BossState
{
    AERIAL_ATTACK,
    SHOOT_ATTACK,
    CHANNELING_RADIALS,

    MOVING,
    IDLE,
    DYING
}
public class BhossBhaby : BaseEntity
{
    private BossState bossState;
    public PlayerController player;
    private Rigidbody2D rb;
    private AttackPatterns ap;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        bossState = BossState.IDLE;
        player = PlayerController.Instance;
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = 1f;
        ap = new AttackPatterns(EntityManager.instance);

    }

    // Update is called once per frame
    void Update()
    {
        switch(bossState)
        {
            case BossState.IDLE:
                //if (Vector2.Distance(player))
                //{

                //}
                break;
            case BossState.MOVING:
                break;
            case BossState.DYING:
                break;
            case BossState.AERIAL_ATTACK:
                case BossState.SHOOT_ATTACK:
                case BossState.CHANNELING_RADIALS:
                break;
            default:
                throw new System.Exception($"Unknown boss state {bossState}");
        }
    }

}
