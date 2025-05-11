using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StandardBullet : BaseBullet
{
    public Vector2 moveDir;

    private Vector2 origin;
    private Vector2 perpendicularDirection;
    private float timeAlive;

    public float bulletDistance = 10f;
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    public bool isReflectable = true;
    

    protected new void Start()
    {
        base.Start();
        origin = transform.position;
        spriteRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (bulletDistance > 0 && Vector2.Distance(origin, transform.position) > bulletDistance)
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
        HandleSpriteAndColliderRotation(moveDir);
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
        origin = startPos;
    }
    private void HandleSpriteAndColliderRotation(Vector2 dirVector)
    {
       Vector2 tempDir = dirVector.normalized;
        float angleRelXAxis = Mathf.Atan2(tempDir.y, tempDir.x) * Mathf.Rad2Deg;
        float angleRelYAxis = Mathf.Atan2(tempDir.x, tempDir.y) * Mathf.Rad2Deg;
        if (angleRelXAxis < 0)
        {
            angleRelXAxis += 360;
        }
        transform.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, -angleRelYAxis);   
        Vector3[] directions = BulletHellCommon.directions;
        int directionIndex = Mathf.RoundToInt(angleRelXAxis / 45) % 8;
        float snappedAngle = directionIndex * 45f;
        float correctionAngle = angleRelXAxis - snappedAngle;
        transform.GetChild(1).transform.rotation = Quaternion.Euler(0, 0, correctionAngle);

        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnTriggerEnter2D(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null | collision.transform.parent == null || collision.transform.parent.gameObject == null)
            return;
        
        bool hitPlayer = collision.transform.parent.gameObject.tag == "Player" && transform.gameObject.layer == BulletHellCommon.BULLET_LAYER;
        bool hitEnemy = new List<string>() {"Enemy", "Boss"}.Contains(collision.transform.parent.gameObject.tag) && transform.gameObject.layer == BulletHellCommon.PLAYER_PROJECTILE_LAYER;
        bool hitWall = collision.transform.parent.gameObject.layer == BulletHellCommon.WALL_LAYER;

        if (hitEnemy || hitPlayer || hitWall)
        {
            EntityManager.instance.Repool(gameObject);
        }
    }
}
