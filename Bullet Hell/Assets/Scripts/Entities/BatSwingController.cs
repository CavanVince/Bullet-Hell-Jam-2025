using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using Unity.Burst.CompilerServices;

public enum SwingState
{
    CHARGING,
    SWINGING,
    NONE
}
public class BatSwingController : MonoBehaviour
{
    private float totalChargeTime;
    private float timeAfterFullChargeStart;
    private float timeAfterFullChargeEnd;
    public float chargeBarSpeed = 0.5f;

    private SwingState swingState;

    public float batStartAngle;
    public float batEndAngle;
    public float batLength;

    public int maxBallReturnSpeed;
    public int minBallReturnSpeed;

    public float swingTime;

    [SerializeField]
    private float batTime;

    private int currentSwingPower;
    private bool isCrit;

    [SerializeField]
    private Transform batTrans;

    [SerializeField]
    private GameObject batTrailPrefab;

    private Vector3 batAnimVelocity = Vector3.zero;
    private PlayerController playerController;

    Sprite[] allChargeBarSprites;

    Sprite[] chargeBarSprites;
    Sprite criticalChargeSprite;

    [SerializeField]
    private float critTimePlusMinus;

    [SerializeField]
    private GameObject chargeBar;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();

        allChargeBarSprites = Resources.LoadAll<Sprite>("Power-Bar");

        chargeBarSprites = allChargeBarSprites.Take(allChargeBarSprites.Length - 1).ToArray();
        criticalChargeSprite = allChargeBarSprites[allChargeBarSprites.Length - 1];

        swingState = SwingState.NONE;

        if (swingTime == 0)
        {
            swingTime = .2f;
        }
        if (minBallReturnSpeed == 0)
        {
            minBallReturnSpeed = 1;
        }
        if (maxBallReturnSpeed == 0)
        {
            maxBallReturnSpeed = 1;
        }
    }

    public void StartSwingWindup()
    {
        isCrit = false;
        if (swingState != SwingState.NONE) return;
        chargeBar.GetComponent<SpriteRenderer>().sprite = chargeBarSprites[0];

        swingState = SwingState.CHARGING;
        StartCoroutine(ShowChargeBarAnimation());
    }

    public void StopSwingWindup()
    {
        if (swingState != SwingState.CHARGING) return;
        timeAfterFullChargeEnd = Time.time;
        totalChargeTime = timeAfterFullChargeEnd - timeAfterFullChargeStart;
        if ((totalChargeTime <= critTimePlusMinus))
        {
            chargeBar.GetComponent<SpriteRenderer>().sprite = criticalChargeSprite;
            isCrit = true;
        }
        swingState = SwingState.SWINGING;
        StartCoroutine(SwingBat(currentSwingPower));
    }

    IEnumerator ShowChargeBarAnimation()
    {
        chargeBar.SetActive(true);
        currentSwingPower = 1;

        while (swingState == SwingState.CHARGING)
        {
            yield return new WaitForSeconds(chargeBarSpeed + (currentSwingPower * .0025f));
            if (currentSwingPower < chargeBarSprites.Length)
            {
                // set next sprite
                currentSwingPower++;
                chargeBar.GetComponent<SpriteRenderer>().sprite = chargeBarSprites[currentSwingPower - 1];
                if (chargeBar.GetComponent<SpriteRenderer>().sprite == chargeBarSprites[chargeBarSprites.Length - 1])
                {
                    timeAfterFullChargeStart = Time.time;
                }
            }
        }
        chargeBar.SetActive(false);
    }

    private IEnumerator SwingBat(int swingPower)
    {
        int maxSwingPower = chargeBarSprites.Length;
        int minSwingPower = 1;
        float normalizedSwingPower = (float)(swingPower - minSwingPower) / (float)(maxSwingPower - minSwingPower);
        float ballReturnSpeedModifier = Mathf.Lerp(minBallReturnSpeed, maxBallReturnSpeed, normalizedSwingPower);
        playerController.dashAvailable = false;
        float elapsed = 0f;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector3 aimVector = (mousePosition - transform.position).normalized;
        Vector3 centerPerpend = new Vector3(-aimVector.y, aimVector.x, 0);
        float angle = Mathf.Atan2(centerPerpend.y, centerPerpend.x) * Mathf.Rad2Deg;

        if (angle < 0)
        {
            angle += 360;
        }
        int directionIndex = Mathf.RoundToInt(angle / 45) % 8;
        Vector3[] directions = new Vector3[8]
        {
            Vector3.left,
            new Vector3(-1,-1,0).normalized,
            Vector3.down,
            new Vector3(1,-1,0).normalized,
            Vector3.right,
            new Vector3(1,1,0).normalized,
            Vector3.up,
            new Vector3(-1,1,0).normalized
        };

        Vector3 adjCenter = directions[directionIndex];
        List<GameObject> hits = new List<GameObject>();

        // Instantiate bat trail
        GameObject batTrail = Instantiate(batTrailPrefab, new Vector2(batTrans.position.x - 0.7f, batTrans.position.y + 0.7f), Quaternion.identity, batTrans);

        while (elapsed < swingTime)
        {
            // Calculate values 
            // Most of this stuff is now just used to animate the bat, as we now use a circle collider
            Vector3 origin = transform.GetChild(0).position;
            float t = elapsed / batTime;
            float currentAngle = Mathf.Lerp(batStartAngle, batEndAngle, t);
            Vector3 dir = Quaternion.Euler(0f, 0f, currentAngle) * adjCenter;
            // Debug.DrawRay(origin, dir.normalized * batLength, Color.red);

            // Bat animation
            batTrans.position = origin + dir.normalized * (batLength);

            // Detect collisions
            Collider2D[] cols = Physics2D.OverlapCircleAll(origin, batLength);
            Vector3 characterToCollider;
            float dot;
            foreach (Collider2D collider in cols)
            {
                if(collider.transform.parent == null){
                    continue;
                }
                GameObject colliderParent = collider.transform.parent.gameObject;
                // If the collider isn't a bullet, enemy, or breakable, continue.
                if (colliderParent.layer != BulletHellCommon.BULLET_LAYER && colliderParent.layer != BulletHellCommon.ENEMY_LAYER && colliderParent.layer != BulletHellCommon.BREAKABLE_LAYER) continue;

                // If the collider has already been hit, continue
                if (hits.Contains(colliderParent)) continue;

                characterToCollider = (collider.transform.position - origin).normalized;
                dot = Vector3.Dot(characterToCollider, aimVector);
                if (dot >= Mathf.Cos(55))
                {
                    // Object hit
                    int layer = colliderParent.layer;
                    Vector3 camMousePos3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 camMousePos2D = new Vector2(camMousePos3D.x, camMousePos3D.y);
                    Vector2 reflectDir = (camMousePos2D - new Vector2(collider.transform.position.x, collider.transform.position.y)).normalized;
                    if (reflectDir.magnitude < batLength)
                    {
                        reflectDir = (camMousePos2D - new Vector2(transform.GetChild(0).position.x, transform.GetChild(0).position.y)).normalized;
                    }

                    BaseBullet bullet = colliderParent.transform.GetComponent<BaseBullet>();

                    if (layer == BulletHellCommon.BULLET_LAYER && bullet != null && bullet.GetType() == typeof(StandardBullet))
                    {
                        StandardBullet sb = bullet as StandardBullet;
                        if (!sb.isReflectable)
                        {
                            continue;
                        }
                        colliderParent.gameObject.layer = BulletHellCommon.PLAYER_PROJECTILE_LAYER; // Player Projectile layer
                        if (isCrit)
                        {
                            ballReturnSpeedModifier *= 1.5f;
                            bullet.damage *= 2;
                            isCrit = false;
                        }
                        StandardBullet standardBullet = (StandardBullet)bullet;
                        standardBullet.Fire(standardBullet.transform.position, reflectDir, ballReturnSpeedModifier * standardBullet.moveSpeed);
                    }
                    if (layer == BulletHellCommon.ENEMY_LAYER)
                    {
                        if (colliderParent.gameObject.tag == "Boss")
                        {
                            colliderParent.transform.GetComponent<HealthComponent>().TakeDamage(1);
                        }
                        else
                        {
                            BaseEnemy enemy = colliderParent.transform.GetComponent<BaseEnemy>();
                            if (enemy != null)
                            {
                                enemy.Launch(reflectDir, ballReturnSpeedModifier * BulletHellCommon.BASE_ENEMY_LAUNCH_SPEED, 1);
                            }
                        }
                    }
                    if (layer == BulletHellCommon.BREAKABLE_LAYER)
                    {
                        colliderParent.GetComponent<BreakableItem>().Break();
                    }
                    hits.Add(colliderParent.gameObject);
                }

            }
            elapsed += Time.deltaTime;

            // Detach the trail
            if (elapsed >= batTime)
                batTrail.transform.parent = null;

            yield return null;
        }

        playerController.dashAvailable = true;
        swingState = SwingState.NONE;
    }

    private void Update()
    {
        if (swingState == SwingState.SWINGING) return;

        // Adjust the bat's position
        Vector3 newBatPos = transform.GetChild(0).position;

        if (playerController.PreviousDir.y > 0 || (playerController.PreviousDir.y == 0 && playerController.PreviousDir.x > 0)) newBatPos.x += 1;
        newBatPos.y += Mathf.Sin(Time.time) * 0.25f;

        batTrans.position = Vector3.SmoothDamp(batTrans.position, newBatPos, ref batAnimVelocity, 0.05f);
    }

}
