using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunEnemy : BaseEnemy
{
    override
    protected void ShootPlayer()
    {
        int numberOfBullets = 5;
        float spreadAngle = 30f;
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        for (int i = 0; i < numberOfBullets; i++)
        {
            float angleOffset = Mathf.Lerp(-spreadAngle / 2, spreadAngle / 2, (float)i / (numberOfBullets - 1));
            float bulletAngle = baseAngle + angleOffset;
            Vector3 bulletDir = new Vector3(Mathf.Cos(bulletAngle * Mathf.Deg2Rad), Mathf.Sin(bulletAngle * Mathf.Deg2Rad), 0);

            BulletManager.instance.FireBullet(transform.position, bulletDir.normalized);
        }
    }
}
