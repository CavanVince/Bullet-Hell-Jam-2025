using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;


public enum BulletType
{
    STANDARD,
    AERIAL
}
public class BulletManager : MonoBehaviour
{
    public static BulletManager instance;

    private Dictionary<BulletType, List<BaseBullet>> spawnedBulletPools;

    [SerializeField]
    private GameObject standardBulletPrefab, aerialBulletPrefab;

    public Camera camera;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;

            // Pool 20 initial bullets
            spawnedBulletPools = new Dictionary<BulletType, List<BaseBullet>>()
            {
                {BulletType.STANDARD, new List<BaseBullet>() },
                {BulletType.AERIAL, new List<BaseBullet>() }
            };

            foreach(KeyValuePair<BulletType, List<BaseBullet>> kv in spawnedBulletPools)
            {
                SpawnBullet(kv.Key);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Grabs an available bullet from the pool and launches it in the specified direction
    /// </summary>
    /// <param name="startPos">Starting position of the bullet</param>
    /// <param name="direction">Direction the bullet is launched in</param>
    public void FireBullet(Vector2 startPos, Vector2 direction, Func<float, float> movementFunc)
    {
        // Iterate through the pooled bullets and determine if one can be fired
        foreach (StandardBullet bullet in spawnedBulletPools[BulletType.STANDARD])
        {
            if (!bullet.gameObject.activeInHierarchy)
            {
                LaunchBullet(bullet, startPos, direction.normalized, movementFunc);
                return;
            }
        }

        // Spawn a new bullet if an available one can't be found in the pool
        LaunchBullet((StandardBullet) SpawnBullet(BulletType.STANDARD), startPos, direction.normalized, movementFunc);
    }

    public void FireAerialBullet(Vector2 destination)
    {
        foreach (AerialBullet bullet in spawnedBulletPools[BulletType.AERIAL])
        {
            if (!bullet.gameObject.activeInHierarchy)
            {
                LaunchAerialBullet(bullet, destination);
                return;
            }
        }

        LaunchAerialBullet((AerialBullet)SpawnBullet(BulletType.AERIAL), destination);
    }

    /// <summary>
    /// Overload of the FireBullet function that doesn't require a movement function (moves it in a straight line)
    /// </summary>
    /// <param name="startPos">Starting position of the bullet</param>
    /// <param name="direction">Direction the bullet is launched in</param>
    public void FireBullet(Vector2 startPos, Vector2 direction)
    {
        FireBullet(startPos, direction, x => 0);
    }

    /// <summary>
    /// Instantiate a bullet in the world to be pooled
    /// </summary>
    private BaseBullet SpawnBullet(BulletType bulletType)
    {
        GameObject bulletPrefab;
        switch(bulletType)
        {
            case BulletType.AERIAL:
                bulletPrefab = aerialBulletPrefab;
                break;
            case BulletType.STANDARD:
            default:
                bulletPrefab = standardBulletPrefab;
                break;
        }

        GameObject spawnedBullet = Instantiate(bulletPrefab, transform);
        spawnedBullet.SetActive(false);
        BaseBullet bullet = spawnedBullet.GetComponent<BaseBullet>();

        spawnedBulletPools[bulletType].Add(bullet);

        return bullet;
    }


    /// <summary>
    /// Helper function to launch a bullet
    /// </summary>
    /// <param name="bullet">Bullet to launch</param>
    /// <param name="startPos">The starting position of the bullet</param>
    /// <param name="direction">The direction of the bullet</param>
    /// <param name="movementFunc">The function that the bullet should follow while moving</param>
    private void LaunchBullet(StandardBullet bullet, Vector2 startPos, Vector2 direction, Func<float, float> movementFunc)
    {
        bullet.transform.position = startPos;
        bullet.gameObject.SetActive(true);
        bullet.Fire(direction, movementFunc);
    }

    private void LaunchAerialBullet(AerialBullet bullet, Vector2 destination)
    {
        Debug.Log($"Aerial Destination: {destination}");
        bullet.transform.position = destination;
        bullet.gameObject.SetActive(true);
        bullet.Fire(destination);
    }
}
