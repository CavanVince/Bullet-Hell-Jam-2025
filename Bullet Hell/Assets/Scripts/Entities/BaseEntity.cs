using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEntity : MonoBehaviour
{
    [HideInInspector]
    public HealthComponent healthComponent;

    public float defaultMoveSpeed = 1f;

    [HideInInspector]
    public float moveSpeed;

    protected float maxMoveSpeed;
    protected bool takesFriendlyFireAerialBulletDamage;

    protected Dictionary<int, List<string>> tagToFriendlyBulletLayerMap = new Dictionary<int, List<string>>()
    {
        { BulletHellCommon.PLAYER_PROJECTILE_LAYER, new List<string> {"Player"} },
        { BulletHellCommon.BULLET_LAYER , new List<string> { "Enemy", "Boss" } },
    };

    protected virtual void Start()
    {
        moveSpeed = defaultMoveSpeed;
        healthComponent= GetComponent<HealthComponent>();
        // REMOVE 'true' if non-player entities should not take aerial damage
        takesFriendlyFireAerialBulletDamage = true || gameObject.tag == "Player";
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D collider = collision.collider;
        OnTriggerEnter2D(collider);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        Transform parent = collision.transform.parent;
        if (parent == null)
        {
            Debug.Log("No parent");
            return;
        }
        BaseBullet bullet = parent.GetComponent<BaseBullet>();
        if (bullet == null) return;

        bool isHostileBullet = !tagToFriendlyBulletLayerMap[bullet.gameObject.layer].Contains(transform.gameObject.tag);
        if (isHostileBullet || (takesFriendlyFireAerialBulletDamage && bullet.GetType() == typeof(AerialBullet))) {
            healthComponent?.TakeDamage(bullet.damage);
        }
    }

    public virtual void ResetState()
    {
        healthComponent.ResetState();
        moveSpeed = defaultMoveSpeed;
    }
}
