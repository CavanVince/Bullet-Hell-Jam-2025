using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialShooterEnemy : BaseEnemy
{
    [Header("Shooter Parameters")]
    [SerializeField]
    private int pulseCount = 1;

    [SerializeField]
    private float pulseInterval_s = 1f;

    [SerializeField]
    private float aftershotCooldown = 0f;

    [SerializeField]
    private int numBullets = 8;

    [SerializeField]
    private float bulletDistance = 10f;

    [Header("Rotation (WIP)")]
    [Range(-1, 1)]
    private int rotation = 0;
    private float rotationSpeed = 0f;

    protected override void Start()
    {
        base.Start();
        ShootParameters sp = new ShootParameters(
            originCalculation: () => transform.position,
            destinationCalculation: () => player.transform.position,
            movementFunc: null,
            numBullets: numBullets,
            pulseCount: pulseCount,
            pulseInterval_s: pulseInterval_s,
            cooldown: aftershotCooldown,
            bulletDistance: bulletDistance
         );

        shootFunc = () => ap.DivergingRadial(sp);
    }
}
