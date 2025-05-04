using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLauncher : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            BulletManager.instance.FireBullet(Vector2.zero, Vector2.right, x => Mathf.Sin(x * 8f) * 0.1f);
            BulletManager.instance.FireBullet(Vector2.zero, Vector2.up, x => Mathf.Sin(x * 8f) * 0.1f);
            BulletManager.instance.FireBullet(Vector2.zero, -Vector2.right, x => Mathf.Cos(x * 8f) * 0.1f);
            BulletManager.instance.FireBullet(Vector2.zero, -Vector2.up, x => Mathf.Cos(x * 8f) * 0.1f);
            BulletManager.instance.FireBullet(Vector2.zero, Vector2.one, x => Mathf.Sin(x * 8f) * 0.1f);
            BulletManager.instance.FireBullet(Vector2.zero, -Vector2.one, x => Mathf.Cos(x * 8f) * 0.1f);
            BulletManager.instance.FireBullet(Vector2.zero, new Vector2(1, -1), x => Mathf.Cos(x * 8f) * 0.1f);
            BulletManager.instance.FireBullet(Vector2.zero, new Vector2(-1, 1), x => Mathf.Sin(x * 8f) * 0.1f);
        }
    }
}
