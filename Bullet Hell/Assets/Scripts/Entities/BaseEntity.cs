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

    protected Dictionary<int, string> tagToFriendlyBulletLayerMap = new Dictionary<int, string>()
    {
        { BulletHellCommon.PLAYER_PROJECTILE_LAYER, "Player" },
        { BulletHellCommon.BULLET_LAYER , "Enemy" },
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
        if (parent == null) return;
        BaseBullet bullet = parent.GetComponent<BaseBullet>();
        if (bullet == null) return;

        bool isHostileBullet = transform.gameObject.tag != tagToFriendlyBulletLayerMap[bullet.gameObject.layer];
        if (isHostileBullet || (takesFriendlyFireAerialBulletDamage && bullet.GetType() == typeof(AerialBullet))) {
            Debug.Log($"Entity: {transform.name} collided with {collision.gameObject.name}");
            healthComponent?.TakeDamage(bullet.damage);
        }
    }

    public virtual void ResetState()
    {
        healthComponent.ResetState();
        moveSpeed = defaultMoveSpeed;
    }
}
