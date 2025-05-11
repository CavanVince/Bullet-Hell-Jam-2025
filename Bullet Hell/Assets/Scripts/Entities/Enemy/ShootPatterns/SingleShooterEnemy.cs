using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleShooterEnemy : BaseEnemy
{
    [Header("Shooter Parameters")]
    [SerializeField]
    private float pulseInterval_s = 1f;

    [SerializeField]
    private float aftershotCooldown = 0f;

    [SerializeField]
    private int numBullets = 3;

    [SerializeField]
    private float bulletDistance = 10f;

    protected override void Start()
    {
        base.Start();
        ShootParameters sp = new ShootParameters(
            originCalculation: () => transform.position,
            destinationCalculation: () => player.transform.position,
            movementFunc: null,
            numBullets: numBullets,
            pulseInterval_s: pulseInterval_s,
            cooldown: aftershotCooldown,
            bulletDistance: bulletDistance
         );

        shootFunc = () => ap.Shoot(sp);
    }
}
