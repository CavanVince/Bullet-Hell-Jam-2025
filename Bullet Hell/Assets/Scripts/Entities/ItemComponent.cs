using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    HEALTH_PICKUP
}
public class ItemComponent : MonoBehaviour
{
    public ItemType itemType;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != "Player")
            return;
        
        switch(itemType)
        {
            case ItemType.HEALTH_PICKUP:
                PickupHealth(collision);
                break;

            default: break;
        }

        Destroy(gameObject);
    }

    void PickupHealth(Collider2D collision)
    {
        HealthComponent healthComponent = collision.gameObject.GetComponentInParent<HealthComponent>();
        if (healthComponent == null) {
            Debug.Log("WARNING: NO HEALTH COMPONENT WAS FOUND FOR PLAYER. FAILED TO PICK UP HEALTH");
            return;
        }
        Debug.Log("Healed player for 1 health");
        healthComponent.Heal(1);
    }
}
