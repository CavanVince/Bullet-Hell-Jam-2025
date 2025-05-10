using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPatterns
{
    EntityManager entityManager;
    public AttackPatterns(EntityManager entityManager)
    {
        this.entityManager = entityManager;
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

    public IEnumerator ShotgunShot(ShootParameters shootParams)
    {
        Vector3 directionToTarget = (shootParams.destinationCalculation() - shootParams.originCalculation()).normalized;
        float baseAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

        for (int i = 0; i < shootParams.numBullets; i++)
        {
            float angleOffset = Mathf.Lerp(shootParams.spreadAngle / 2, shootParams.spreadAngle / 2, (float)i / (shootParams.numBullets - 1));
            float bulletAngle = baseAngle + angleOffset;
            Vector3 bulletDir = new Vector3(Mathf.Cos(bulletAngle * Mathf.Deg2Rad), Mathf.Sin(bulletAngle * Mathf.Deg2Rad), 0);

            entityManager.FireBullet(typeof(StandardBullet), shootParams.originCalculation(), bulletDir.normalized);
        }
        yield return new WaitForSeconds(shootParams.cooldown);
    }

    public IEnumerator Shoot(ShootParameters shootParams)
    {
        for (int i = 0; i < shootParams.numBullets; i++)
        {
            Vector3 directionToPlayer = (shootParams.destinationCalculation() - shootParams.originCalculation()).normalized;
            entityManager.FireBullet(typeof(StandardBullet), shootParams.originCalculation(), directionToPlayer);

            yield return new WaitForSeconds(shootParams.pulseInterval_s);
        }
        yield return new WaitForSeconds(shootParams.cooldown);
    }

    public IEnumerator DivergingRadial(ShootParameters shootParams)
    {
        Vector2 origin = shootParams.originCalculation();

        List<Vector2> points = GetNRadialPointsAroundOrigin(origin, shootParams.numBullets, 1);
        for (int i = 0; i < shootParams.pulseCount; i++)
        {
            foreach(Vector2 point in points)
            {
                entityManager.FireBullet(typeof(StandardBullet), origin, (point - origin).normalized, shootParams.movementFunc);
            }
            yield return new WaitForSeconds(shootParams.pulseInterval_s);
        }

        yield return new WaitForSeconds(shootParams.cooldown);
    }

    public IEnumerator DaOctopus(ShootParameters shootParams)
    {
        shootParams.movementFunc = x => Mathf.Sin(x * 8f) * .1f;
        yield return DivergingRadial(shootParams);
    }

    public IEnumerator RadialAerialAroundPlayer(ShootParameters shootParams)
    {

        for (int r = shootParams.pulseCount; r > 0; r--)
        {
            Vector2 target = shootParams.destinationCalculation();
            List<Vector2> points = GetNRadialPointsAroundOrigin(target, r * 2, r);

            foreach(Vector2 point in points)
            {
                entityManager.FireBullet(typeof(AerialBullet), point, point);
            }

            yield return new WaitForSeconds(shootParams.pulseInterval_s);
        }

        yield return new WaitForSeconds(shootParams.cooldown);
    }

    // sorry bro, this shit is broke
    //public IEnumerator WalkDaLineAerial(ShootParameters shootParams)
    //{
    //    Vector2 origin = shootParams.originCalculation();
    //    Vector2 target = shootParams.destinationCalculation();

    //    Vector2 dir = (target - origin).normalized;

    //    Vector2 current = origin;

    //    while (Vector2.Distance(current, target) > spacing)
    //    {
    //        entityManager.FireBullet(typeof(AerialBullet), current, current);
    //        current = current + (dir * spacing);

    //        yield return new WaitForSeconds(shootParams.pulseInterval_s);
    //    }

    //    yield return new WaitForSeconds(shootParams.cooldown);
    //}
}
