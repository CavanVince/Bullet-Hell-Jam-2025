using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardBullet : BaseBullet
{
    public Vector2 moveDir;

    private Vector2 perpendicularDirection;
    private float timeAlive;

    [SerializeField]
    private float moveSpeed;

    private void FixedUpdate()
    {
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
    public void Fire(Vector2 direction, Func<float, float> movementFunc)
    {
        moveDir = direction;
        perpendicularDirection = new Vector2(-direction.y, direction.x);
        timeAlive = Time.time;
        moveFunc = movementFunc;
    }

    /// <summary>
    /// Overload of the fire function where bullets move in a straight line
    /// </summary>
    /// <param name="direction">The direction that the bullet should move</param>
    public void Fire(Vector2 direction)
    {
        Fire(direction, x => 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool hitPlayer = collision.gameObject.tag == "Player" && transform.gameObject.layer == BulletHellCommon.BULLET_LAYER;
        bool hitEnemy = collision.gameObject.tag == "Enemy" && transform.gameObject.layer == BulletHellCommon.PLAYER_PROJECTILE_LAYER;
        if (hitEnemy || hitPlayer)
        {
            BulletManager.instance.RepoolBullet(gameObject);
        }
    }
}
