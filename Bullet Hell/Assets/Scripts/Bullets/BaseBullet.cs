using System;
using UnityEngine;

public abstract class BaseBullet : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Func<float, float> moveFunc;

    public int baseDamage = 1;

    [HideInInspector]
    public int damage;

    public float baseMoveSpeed = 1f;

    [HideInInspector]
    public float moveSpeed;

    protected virtual void Start()
    {
        damage = baseDamage;
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = baseMoveSpeed;
    }

    public virtual void ResetState()
    {
        damage = baseDamage;
        moveSpeed = baseMoveSpeed;
    }

    public abstract void Fire(Vector2 start, Vector2 destination, float moveSpeed, Func<float, float> moveFunc=null);
}