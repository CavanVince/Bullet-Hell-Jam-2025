using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunShooterEnemy : BaseEnemy
{
    [SerializeField]
    private int pulseCount = 1;

    [SerializeField]
    private float pulseInterval_s = 1f;

    [SerializeField]
    private float aftershotCooldown = 0f;

    [Range(0,180)]
    private int spreadAngle = 60;

    [Range(0f,1f)]
    private float spreadNoise = 0f;

    protected override void Start()
    {
        base.Start();
        ShootParameters sp = new ShootParameters(
            originCalculation: () => transform.position,
            destinationCalculation: () => player.transform.position,
            movementFunc: null,
            numBullets: 1,
            pulseCount: pulseCount,
            pulseInterval_s: pulseInterval_s,
            cooldown: aftershotCooldown,
            spreadAngle:spreadAngle
         );

        shootFunc = () => ap.ShotgunShot(sp);
    }
}
