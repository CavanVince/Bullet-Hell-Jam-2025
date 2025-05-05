using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseBullet : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Func<float, float> moveFunc;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool hitPlayer = collision.transform.GetComponent<PlayerController>() != null && transform.gameObject.layer == BulletHellCommon.BULLET_LAYER;
        bool hitEnemy = collision.transform.GetComponent<BaseEnemy>() != null && transform.gameObject.layer == BulletHellCommon.PLAYER_PROJECTILE_LAYER;
       if (hitEnemy || hitPlayer)
       {
           OnHit();
       }
    }
    protected virtual void OnHit()
    {
        gameObject.SetActive(false);
    }
}