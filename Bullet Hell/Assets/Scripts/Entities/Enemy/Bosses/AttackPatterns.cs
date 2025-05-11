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

    private List<Vector2> GetNRadialPointsBetweenAngles(Vector2 origin, int n, float radius, float startRadian=0, float endRadian=2*Mathf.PI)
    {
        List<Vector2> points = new List<Vector2>();

        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n; // n-1 so start and end points are included
            float angle = Mathf.Lerp(startRadian, endRadian, t);

            Vector2 v = new Vector2(
                origin.x + radius * Mathf.Cos(angle),
                origin.y + radius * Mathf.Sin(angle)
            );
            points.Add(v);
        }

        return points;
    }

    float NormalizeRadian(float rad)
    {
        rad = rad % (2 * Mathf.PI);
        if (rad < 0) rad += 2 * Mathf.PI;
        return rad;
    }

    public IEnumerator ShotgunShot(ShootParameters shootParams)
    {
        Vector3 directionToTarget = (shootParams.destinationCalculation() - shootParams.originCalculation()).normalized;

        float baseRad = Mathf.Atan2(directionToTarget.y, directionToTarget.x);
        float lowerBoundRad = NormalizeRadian(baseRad - (shootParams.spreadAngle / 2)*Mathf.Deg2Rad);
        float upperBoundRad = NormalizeRadian(baseRad + (shootParams.spreadAngle / 2)*Mathf.Deg2Rad);

        List<Vector2> points = GetNRadialPointsBetweenAngles(shootParams.originCalculation(), shootParams.numBullets, shootParams.numBullets, lowerBoundRad, upperBoundRad);
        
        for(int i = 0; i < shootParams.pulseCount; i++)
        {
            foreach(Vector2 point in points) {
                entityManager.FireBullet(typeof(StandardBullet), shootParams.originCalculation(), shootParams.movementFunc);
            }
            yield return new WaitForSeconds(shootParams.pulseInterval_s);
        }

        //float baseAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

        //for (int i = 0; i < shootParams.numBullets; i++)
        //{
        //    float angleOffset = Mathf.Lerp(shootParams.spreadAngle / 2, shootParams.spreadAngle / 2, (float)i / (shootParams.numBullets - 1));
        //    float bulletAngle = baseAngle + angleOffset;
        //    Vector3 bulletDir = new Vector3(Mathf.Cos(bulletAngle * Mathf.Deg2Rad), Mathf.Sin(bulletAngle * Mathf.Deg2Rad), 0);

        //    entityManager.FireBullet(typeof(StandardBullet), shootParams.originCalculation(), bulletDir.normalized);
        //}
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

        List<Vector2> points = GetNRadialPointsBetweenAngles(origin, shootParams.numBullets, 1);
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
            List<Vector2> points = GetNRadialPointsBetweenAngles(target, r * 2, r);

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
