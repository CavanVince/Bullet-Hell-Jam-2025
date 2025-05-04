using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLauncher : MonoBehaviour
{

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Vector2 origin = gameObject.transform.position;
            int numPoints = 5;
            float radius = 5f;

            for (int i = 0; i < numPoints; i++){

                float angle = 360f / numPoints * i;
                float rads = angle * Mathf.Deg2Rad;
                Vector2 v = new Vector2(
                    origin.x + Mathf.Sin(rads) * 5,
                    origin.y + Mathf.Cos(rads) * 5
                );
                BulletManager.instance.FireAerialBullet(v);
            };
        }

        else if (Input.GetKeyDown(KeyCode.T))
        {
            gameObject.GetComponent<AttackPatterns>().Pulse(() => {
                BulletManager.instance.FireBullet(Vector2.zero, Vector2.right, x => Mathf.Sin(x * 8f) * 0.1f);
                BulletManager.instance.FireBullet(Vector2.zero, Vector2.up, x => Mathf.Sin(x * 8f) * 0.1f);
                BulletManager.instance.FireBullet(Vector2.zero, -Vector2.right, x => Mathf.Cos(x * 8f) * 0.1f);
                BulletManager.instance.FireBullet(Vector2.zero, -Vector2.up, x => Mathf.Cos(x * 8f) * 0.1f);
                BulletManager.instance.FireBullet(Vector2.zero, Vector2.one, x => Mathf.Sin(x * 8f) * 0.1f);
                BulletManager.instance.FireBullet(Vector2.zero, -Vector2.one, x => Mathf.Cos(x * 8f) * 0.1f);
                BulletManager.instance.FireBullet(Vector2.zero, new Vector2(1, -1), x => Mathf.Cos(x * 8f) * 0.1f);
                BulletManager.instance.FireBullet(Vector2.zero, new Vector2(-1, 1), x => Mathf.Sin(x * 8f) * 0.1f);
            }
            , .5f, 5f);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            gameObject.GetComponent<AttackPatterns>().Pulse(() =>
            {
                Vector2 origin = gameObject.transform.position;
                int numPoints = 8;
                float radius = 5;
                for (int i = 0; i < numPoints; i++)
                {

                    float angle = 360f / numPoints * i;
                    float rads = angle * Mathf.Deg2Rad;
                    Vector2 v = new Vector2(
                        origin.x + Mathf.Sin(rads) * radius,
                        origin.y + Mathf.Cos(rads) * radius
                    );
                    BulletManager.instance.FireAerialBullet(v);
                };
            }, .25f, 2f);
        }
    }
}
