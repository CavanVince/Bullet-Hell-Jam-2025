using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunEnemy : BaseEnemy
{
    // TODO: IMPLEMENT. This is intended to represent the total number of bullets emitted per shotgun blast, and the number of consecutive shotgun blasts to pulse.
    [SerializeField]
    private int numBullets, pulseCount;

    // TODO: IMPLEMENT. This is intended to represent the interval (in seconds) between shotgun blasts specified in pulseCount.
    private float pulseInterval;

    // TODO: IMPLEMENT. This is intended to represent the total arc area in front of the Enemy that bullets are allowed to spawn when ShootPlayer is called.
    [Range(0, 180)]
    private int arcLength = 60;

}
