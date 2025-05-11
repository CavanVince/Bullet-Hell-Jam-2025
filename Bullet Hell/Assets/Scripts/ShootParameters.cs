using System;
using UnityEngine;

public struct ShootParameters
{
    public ShootParameters(
        Func<Vector2> originCalculation = null,
        Func<Vector2> destinationCalculation = null,
        Func<float, float> movementFunc = null,
        int numBullets = 1,
        int pulseCount = 3,
        float pulseInterval_s = .5f,
        float cooldown = 0,
        int spreadAngle = 60,
        int offsetAngle = 0,
        bool predictPlayerMovement = false,
        float bulletDistance = 10f,
        float bulletSpeed = 6f
    )
    {
        this.originCalculation = originCalculation;
        this.destinationCalculation = destinationCalculation;
        this.movementFunc = movementFunc;
        this.pulseCount = pulseCount;
        this.pulseInterval_s = pulseInterval_s;
        this.cooldown = cooldown;
        this.numBullets = numBullets;
        this.spreadAngle = spreadAngle;
        this.offsetAngle = offsetAngle;
        this.predictPlayerMovement = predictPlayerMovement;
        this.bulletDistance = bulletDistance;
        this.bulletSpeed = bulletSpeed;
    }
    public Func<Vector2> originCalculation;
    public Func<Vector2> destinationCalculation;
    public Func<float, float> movementFunc;
    public int pulseCount;
    public float pulseInterval_s;
    public float cooldown;
    public int numBullets;
    public int spreadAngle;
    public int offsetAngle;
    public bool predictPlayerMovement;
    public float bulletDistance;
    public float bulletSpeed;
}
