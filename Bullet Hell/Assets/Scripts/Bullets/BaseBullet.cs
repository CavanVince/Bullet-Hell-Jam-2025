using System;
using UnityEngine;

public abstract class BaseBullet : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Func<float, float> moveFunc;

    public int baseDamage = 1;
    public int damage;

    public float moveSpeed = 1f;

    protected virtual void Start()
    {
        damage = baseDamage;
        rb = GetComponent<Rigidbody2D>();
    }

    public abstract void Fire(Vector2 start, Vector2 destination, float moveSpeed, Func<float, float> moveFunc=null);
}