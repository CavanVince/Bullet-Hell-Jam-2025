using System;
using UnityEngine;

public abstract class BaseBullet : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Func<float, float> moveFunc;

    public int baseDamage = 1;

    [HideInInspector]
    public int damage;

    [HideInInspector]
    public float moveSpeed = 6f;

    protected float startingMoveSpeed;

    protected virtual void Start()
    {
        damage = baseDamage;
        rb = GetComponent<Rigidbody2D>();
    }

    public virtual void ResetState()
    {
        damage = baseDamage;
    }

    public abstract void Fire(Vector2 start, Vector2 destination, float moveSpeed, Func<float, float> moveFunc=null);
}