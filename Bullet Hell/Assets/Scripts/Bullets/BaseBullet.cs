using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseBullet : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Func<float, float> moveFunc;
    public bool isHittable;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        bool hitPlayer = collision.transform.GetComponent<PlayerController>() != null && isHittable && transform.gameObject.layer == 6;
        bool hitEnemy = collision.transform.GetComponent<BaseEnemy>() != null && transform.gameObject.layer == 8;
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