using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquiggleEnemy : BaseEnemy
{
    [SerializeField]
    private float amplitude;

    [SerializeField]
    private float freq;
    protected override void ShootPlayer()
    {
        if (Vector2.Distance(player.transform.position, transform.position) < shotDistance)
        {
            BulletManager.instance.FireBullet(transform.position, (player.transform.position - transform.position).normalized, x => Mathf.Sin(x * freq) * amplitude);
            

        }
    }

}
