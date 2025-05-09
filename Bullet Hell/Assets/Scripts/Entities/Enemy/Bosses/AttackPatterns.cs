using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AttackPatterns
{
    BulletManager bulletManager;
    public AttackPatterns(BulletManager bulletManager)
    {
        this.bulletManager = bulletManager;
    }

    private List<Vector2> GetNRadialPointsAroundOrigin(Vector2 origin, int n, float radius)
    {
        List<Vector2> points = new List<Vector2>();

        for (int i = 0; i < n; i++)
        {
            Vector2 v = new Vector2(
                origin.x + radius * Mathf.Cos(2 * Mathf.PI * ((float) i / n)),
                origin.y + radius * Mathf.Sin(2 * Mathf.PI * ((float) i / n))
            );
            points.Add(v);
        }
        return points;
    }

    public IEnumerator ShotgunShot(Func<Vector2> calcOrigin, Func<Vector2> target, int numBullets = 5, float spreadAngle = 30f, float cooldown=0f, Action onCompletion = null)
    {
        Vector3 directionToTarget = (target() - calcOrigin()).normalized;
        float baseAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

        for (int i = 0; i < numBullets; i++)
        {
            float angleOffset = Mathf.Lerp(-spreadAngle / 2, spreadAngle / 2, (float)i / (numBullets - 1));
            float bulletAngle = baseAngle + angleOffset;
            Vector3 bulletDir = new Vector3(Mathf.Cos(bulletAngle * Mathf.Deg2Rad), Mathf.Sin(bulletAngle * Mathf.Deg2Rad), 0);

            bulletManager.FireBullet(calcOrigin(), bulletDir.normalized);
        }
        yield return new WaitForSeconds(cooldown);
        if (onCompletion != null)
        {
            onCompletion();
        }
    }

    public IEnumerator Shoot(Func<Vector2> calcOrigin, Func<Vector2> calcTarget, int numBullets = 5, float intervalBetweenShots = .5f, float cooldown=0f, Action onCompletion = null)
    {
        for (int i = 0; i < numBullets; i++)
        {
            Vector3 directionToPlayer = (calcTarget() - calcOrigin()).normalized;
            bulletManager.FireBullet(calcOrigin(), directionToPlayer);

            yield return new WaitForSeconds(intervalBetweenShots);
        }
        yield return new WaitForSeconds(cooldown);
        if (onCompletion != null)
        {
            onCompletion();
        }
    }

    public IEnumerator DivergingRadial(Func<Vector2> calcOrigin, int numBullets, int pulseCount, float pulseInterval, Func<float, float> movementFunc, Func<float, float> angleOffset, float cooldown=0f, Action onComplete = null)
    {
        Vector2 origin = calcOrigin();

        List<Vector2> points = GetNRadialPointsAroundOrigin(origin, numBullets, 1);
        for (int i = 0; i < pulseCount; i++)
        {
            foreach(Vector2 point in points)
            {
                bulletManager.FireBullet(origin, (point - origin).normalized, movementFunc);
            }
            yield return new WaitForSeconds(pulseInterval);
        }

        yield return new WaitForSeconds(cooldown);
        if (onComplete!= null)
        {
            onComplete();
        }
    }

    public IEnumerator DaOctopus(Func<Vector2> calcOrigin, int numBullets, int pulseCount, float pulseInterval, Func<float, float> angleOffset, float cooldown=0f, Action onComplete=null)
    {
        yield return DivergingRadial(calcOrigin, numBullets, pulseCount, pulseInterval, x => Mathf.Sin(x * 8f) * 0.1f, angleOffset, cooldown, onComplete);
    }

    public IEnumerator RadialAerialAroundPlayer(Func<Vector2> calcTarget, int pulseCount, float pulseInterval, float cooldown=0f, Action onComplete=null)
    {

        for (int r = pulseCount; r > 0; r--)
        {
            Vector2 target = calcTarget();
            List<Vector2> points = GetNRadialPointsAroundOrigin(target, r * 2, r);

            foreach(Vector2 point in points)
            {
                bulletManager.FireAerialBullet(point);
            }

            yield return new WaitForSeconds(pulseInterval);
        }

        yield return new WaitForSeconds(cooldown);
        if (onComplete != null)
        {
            onComplete();
        }
    }

    public IEnumerator WalkDaLineAerial(Func<Vector2> calcOrigin, Func<Vector2> calcTarget, float spacing, float pulseInterval, float cooldown=0f, Action onComplete=null)
    {
        Vector2 origin = calcOrigin();
        Vector2 target = calcTarget();

        Vector2 dir = (target - origin).normalized;

        Vector2 current = origin;

        while (Vector2.Distance(current, target) > spacing)
        {
            bulletManager.FireAerialBullet(current);
            current = current + (dir * spacing);

            yield return new WaitForSeconds(pulseInterval);
        }

        yield return new WaitForSeconds(cooldown);
        if (onComplete != null)
        {
            onComplete();
        }
    }
}
