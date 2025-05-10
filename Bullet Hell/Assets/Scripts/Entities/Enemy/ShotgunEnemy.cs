using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunEnemy : BaseEnemy
{
    // TODO: IMPLEMENT. This is intended to represent the total number of bullets emitted per shotgun blast, and the number of consecutive shotgun blasts to pulse.
    [SerializeField]
    private int numBullets, pulseCount;

    // TODO: IMPLEMENT. This is intended to represent the interval (in seconds) between shotgun blasts specified in pulseCount.
    private float pulseInterval;

    // TODO: IMPLEMENT. This is intended to represent the total arc area in front of the Enemy that bullets are allowed to spawn when ShootPlayer is called.
    [Range(0, 180)]
    private int arcLength = 60;

    protected override void ShootPlayer()
    {
        int numberOfBullets = 5;
        float spreadAngle = 30f; // TODO: might be cool to have the spread angle wider if the player is further away, and narrower if they are close (making shotguns more accurate in close range)
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        for (int i = 0; i < numberOfBullets; i++)
        {
            float angleOffset = Mathf.Lerp(-spreadAngle / 2, spreadAngle / 2, (float)i / (numberOfBullets - 1));
            float bulletAngle = baseAngle + angleOffset;
            Vector3 bulletDir = new Vector3(Mathf.Cos(bulletAngle * Mathf.Deg2Rad), Mathf.Sin(bulletAngle * Mathf.Deg2Rad), 0);

            EntityManager.instance.FireBullet(typeof(StandardBullet), transform.position, bulletDir.normalized);
        }
    }
}
