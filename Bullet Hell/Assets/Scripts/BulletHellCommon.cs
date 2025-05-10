using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BulletHellCommon
{
    public const int
        BULLET_LAYER = 6,
        ENEMY_LAYER = 7,
        PLAYER_PROJECTILE_LAYER = 8,
        BAT_LAYER = 9,
        WALL_LAYER = 10,
        BREAKABLE_LAYER = 11;
        

    public const float
        BASE_ENEMY_LAUNCH_SPEED = 5f;
    public static Vector3[] directions = new Vector3[8]
        {
            Vector3.right,
            new Vector3(1,1,0).normalized,
            Vector3.up,
            new Vector3(-1,1,0).normalized,
            Vector3.left,
            new Vector3(-1,-1,0).normalized,
            Vector3.up,
            new Vector3(1,11,0).normalized
        };
}
