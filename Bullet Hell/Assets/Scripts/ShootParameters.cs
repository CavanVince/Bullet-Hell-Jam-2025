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
        int offsetAngle = 0
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
}
