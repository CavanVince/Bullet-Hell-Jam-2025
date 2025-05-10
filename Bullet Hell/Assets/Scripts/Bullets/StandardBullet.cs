using System;
using UnityEngine;

public class StandardBullet : BaseBullet
{
    public Vector2 moveDir;

    private Vector2 origin;
    private Vector2 perpendicularDirection;
    private float timeAlive;

    [SerializeField]
    private float range = 10f;


    protected new void Start()
    {
        base.Start();
        origin = transform.position;
    }

    private void FixedUpdate()
    {
        if (range > 0 && Vector2.Distance(origin, transform.position) > range)
        {
            // out of range and died
            EntityManager.instance.Repool(gameObject);
        }
        Vector2 velocityVector = moveDir * moveSpeed * Time.deltaTime;

        Vector2 moveFuncResult;
        if (moveFunc!= null )
        {
            moveFuncResult = moveFunc(Time.time - timeAlive) * perpendicularDirection;
        }
        else
        {
            moveFuncResult = Vector2.zero;
        }

        // Update the bullet's position
        Vector2 newPosition = (Vector2) transform.position + velocityVector + moveFuncResult;
        rb.MovePosition(newPosition);
    }

    /// <summary>
    /// Set all of the values necessary for the bullet to calculate it's path
    /// </summary>
    /// <param name="direction">The direction that the bullet should move</param>
    /// <param name="movementFunc">The function that the bullet should follow while moving</param>
    public override void Fire(Vector2 startPos, Vector2 direction, float speed, Func<float, float> movementFunc=null)
    {
        moveDir = direction;
        perpendicularDirection = new Vector2(-direction.y, direction.x);
        timeAlive = Time.time;
        moveFunc = movementFunc;
        moveSpeed = speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool hitPlayer = collision.gameObject.tag == "Player" && transform.gameObject.layer == BulletHellCommon.BULLET_LAYER;
        bool hitEnemy = collision.gameObject.tag == "Enemy" && transform.gameObject.layer == BulletHellCommon.PLAYER_PROJECTILE_LAYER;
        if (hitEnemy || hitPlayer)
        {
            EntityManager.instance.Repool(gameObject);
        }
    }
}
