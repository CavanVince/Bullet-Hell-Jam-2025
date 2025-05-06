using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != "Player")
            return;

        if (PickupHealth(collision))
        {
            Destroy(gameObject);
        }
    }

    bool PickupHealth(Collider2D collision)
    {
        HealthComponent healthComponent = collision.gameObject.GetComponentInParent<HealthComponent>();
        if (healthComponent == null)
        {
            Debug.Log("WARNING: NO HEALTH COMPONENT WAS FOUND FOR PLAYER. FAILED TO PICK UP HEALTH");
            return false;
        }
        Debug.Log("Healed player for 1 health");
        if (healthComponent.Heal(1))
        {
            return true;
        }

        return false;
    }
}
