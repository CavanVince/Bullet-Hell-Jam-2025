using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AerialShooterEnemy : BaseEnemy
{
    [SerializeField]
    private int pulseCount = 1;

    [SerializeField]
    private float pulseInterval_s = 1f;

    [SerializeField]
    private float aftershotCooldown = 0f;

    [Header("Player Prediction (WIP)")]
    [SerializeField]
    private bool predictPlayerMovement = false;
    [SerializeField]
    private float travelSpeed = 1f;

    protected override void Start()
    {
        base.Start();
        ShootParameters sp = new ShootParameters(
            originCalculation:() => transform.position,
            destinationCalculation: () => player.transform.position,
            movementFunc: null,
            numBullets:1,
            pulseCount:pulseCount,
            pulseInterval_s:pulseInterval_s,
            cooldown:aftershotCooldown,
            predictPlayerMovement:predictPlayerMovement
         );

        shootFunc = () => ap.Aerial(sp);
    }
}
