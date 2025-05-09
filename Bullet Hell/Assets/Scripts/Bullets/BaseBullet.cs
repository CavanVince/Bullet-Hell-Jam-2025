using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseBullet : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Func<float, float> moveFunc;

    public int baseDamage = 1;
    public int damage;


    void Start()
    {
        damage = baseDamage;
        rb = GetComponent<Rigidbody2D>();
    }
}