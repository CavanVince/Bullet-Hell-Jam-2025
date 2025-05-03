using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public static BulletManager instance;

    private List<Bullet> spawnedBullets;

    [SerializeField]
    private GameObject bulletPrefab;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;

            // Pool 20 initial bullets
            spawnedBullets = new List<Bullet>();
            for (int i = 0; i < 20; i++)
            {
                SpawnBullet();
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
        foreach (Bullet bullet in spawnedBullets)
        {
            if (!bullet.gameObject.activeInHierarchy)
            {
                LaunchBullet(bullet, startPos, direction.normalized, movementFunc);
                return;
            }
        }

        // Spawn a new bullet if an available one can't be found in the pool
        LaunchBullet(SpawnBullet(), startPos, direction.normalized, movementFunc);
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
    private Bullet SpawnBullet()
    {
        GameObject spawnedBullet = Instantiate(bulletPrefab, transform);
        spawnedBullet.SetActive(false);
        Bullet bullet = spawnedBullet.GetComponent<Bullet>();
        spawnedBullets.Add(bullet);
        return bullet;
    }


    /// <summary>
    /// Helper function to launch a bullet
    /// </summary>
    /// <param name="bullet">Bullet to launch</param>
    /// <param name="startPos">The starting position of the bullet</param>
    /// <param name="direction">The direction of the bullet</param>
    /// <param name="movementFunc">The function that the bullet should follow while moving</param>
    private void LaunchBullet(Bullet bullet, Vector2 startPos, Vector2 direction, Func<float, float> movementFunc)
    {
        bullet.transform.position = startPos;
        bullet.gameObject.SetActive(true);
        bullet.Fire(direction, movementFunc);
    }
}
