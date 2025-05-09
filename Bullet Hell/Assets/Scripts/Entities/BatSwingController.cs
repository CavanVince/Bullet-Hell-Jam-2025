using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    private int currentSwingPower;
    private bool isCrit;

    Sprite[] allChargeBarSprites;

    Sprite[] chargeBarSprites;
    Sprite criticalChargeSprite;
    
    [SerializeField]
    private float critTimePlusMinus;

    [SerializeField]
    private GameObject chargeBar;

    private void Start()
    {
        allChargeBarSprites = Resources.LoadAll<Sprite>("Power-Bar");
        
        chargeBarSprites = allChargeBarSprites.Take(allChargeBarSprites.Length - 1).ToArray();
        criticalChargeSprite = allChargeBarSprites[allChargeBarSprites.Length - 1];

        swingState = SwingState.NONE;

        if (swingTime == 0)
        {
            swingTime = .2f;
        }
        if (minBallReturnSpeed== 0)
        {
            minBallReturnSpeed= 1;
        }
        if (maxBallReturnSpeed== 0) 
        { 
            maxBallReturnSpeed= 1;
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
        Debug.Log($"Total Charge Time: {totalChargeTime}");
        if ((totalChargeTime <= critTimePlusMinus))
        {
            chargeBar.GetComponent<SpriteRenderer>().sprite = criticalChargeSprite;
            isCrit = true;
        }
        swingState = SwingState.SWINGING;
        Debug.Log($"Swing power: {currentSwingPower}");
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
                chargeBar.GetComponent<SpriteRenderer>().sprite = chargeBarSprites[currentSwingPower-1];
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
        float normalizedSwingPower = (float) (swingPower - minSwingPower) / (float) (maxSwingPower - minSwingPower);
        float ballReturnSpeedModifier = Mathf.Lerp(minBallReturnSpeed, maxBallReturnSpeed, normalizedSwingPower);
        Debug.Log($"normalized swing power: {normalizedSwingPower}");
        Debug.Log($"ballReturnSpeedModifier: {ballReturnSpeedModifier}");
        GetComponent<PlayerController>().dashAvailable = false;
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

        Vector3 origin = transform.GetChild(0).position;
        Vector3 dir = Quaternion.Euler(0f, 0f, batStartAngle) * Vector3.right;

        List<GameObject> hits = new List<GameObject>();

        while (elapsed < swingTime)
        {
            origin = transform.GetChild(0).position;
            float t = elapsed / swingTime;
            float currentAngle = Mathf.Lerp(batStartAngle, batEndAngle, t);
            dir = Quaternion.Euler(0f, 0f, currentAngle) * adjCenter;
            Debug.DrawRay(origin, dir * batLength, Color.red);

            RaycastHit2D hit = Physics2D.Raycast(origin, dir.normalized, batLength, 1 << 6 | 1 << 7);
            if (hit && !hits.Contains(hit.transform.gameObject))
            {
                int layer = hit.transform.gameObject.layer;
                Debug.Log(hit.transform.name);
                Vector3 camMousePos3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 camMousePos2D = new Vector2(camMousePos3D.x, camMousePos3D.y);
                Vector2 reflectDir = (camMousePos2D - new Vector2(hit.transform.position.x, hit.transform.position.y)).normalized;
                if (reflectDir.magnitude < batLength)
                {
                    reflectDir = (camMousePos2D - new Vector2(transform.GetChild(0).position.x, transform.GetChild(0).position.y)).normalized;
                }

                BaseBullet bullet = hit.transform.GetComponent<BaseBullet>();
                if (layer == BulletHellCommon.BULLET_LAYER && bullet != null && bullet.GetType() == typeof(StandardBullet))
                {
                    Debug.Log($"Hit Bullet {hit.transform.name} with bat");
                    hit.transform.gameObject.layer = BulletHellCommon.PLAYER_PROJECTILE_LAYER; // Player Projectile layer
                    if (isCrit)
                    {
                        ballReturnSpeedModifier *= 2;
                        bullet.damage *= 2;
                        Debug.Log($"Crit! Dealing {bullet.damage} damage");
                        isCrit = false;
                    }
                    StandardBullet standard_bullet = (StandardBullet) bullet;
                    standard_bullet.Fire(reflectDir * ballReturnSpeedModifier);
                }
                if (layer == BulletHellCommon.ENEMY_LAYER)
                {
                    Debug.Log($"Hit enemy {hit.transform.name} with bat");
                    hit.transform.GetComponent<BaseEnemy>().Launch(reflectDir * ballReturnSpeedModifier* 5f);
                }
                hits.Add(hit.transform.gameObject);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        GetComponent<PlayerController>().dashAvailable = true;
        swingState = SwingState.NONE;
    }

}
