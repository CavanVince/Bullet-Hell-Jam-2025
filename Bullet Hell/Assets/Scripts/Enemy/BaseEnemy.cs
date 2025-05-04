using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    protected PlayerController player;
    protected float shotTimerTracker = 0;

    [SerializeField]
    protected float shotTimer; // How long between shots

    [SerializeField]
    protected float shotDistance; // How far the enemy can be before it shoots

    void Start()
    {
        // TODO: Needs to be made to not suck
        player = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if (shotTimerTracker <= 0)
        {
            ShootPlayer();
            shotTimerTracker = shotTimer;
        }
        else
        {
            shotTimerTracker -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Launches a bullet at the player if 
    /// </summary>
    protected virtual void ShootPlayer()
    {
        if (Vector2.Distance(player.transform.position, transform.position) < shotDistance)
        {
            BulletManager.instance.FireBullet(transform.position, (player.transform.position - transform.position).normalized);
        }
    }
}
