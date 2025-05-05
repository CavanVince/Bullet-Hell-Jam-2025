using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField]
    protected int health;

    protected bool invulnerable;
    protected float invulnerabilityDurationOnHit;

    void Start()
    {
        if (health == 0)
        {
            health = 5;
        }
        invulnerable = false;
    }

    public void TakeDamage()
    {
        TakeDamage(1);
    }

    public void TakeDamage(int damage)
    {
        if (invulnerable)
            return;

        health -= damage;
        if (health <= 0) 
        {
            Debug.Log($"{transform.name} took lethal damage!");
            if (gameObject.tag == "Player")
            {
                Debug.Log("YOU DIED. GAME OVER SCREEN WOULD GO HERE");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        if (invulnerabilityDurationOnHit > 0) {
            StartCoroutine(StartInvulnerability());
        }
    }

    protected virtual IEnumerator StartInvulnerability()
    {
        SpriteRenderer spr = transform.GetComponent<SpriteRenderer>();
        Color originalColor = spr.color;

        spr.color = Color.red;
        invulnerable = true;

        yield return new WaitForSeconds(invulnerabilityDurationOnHit);
        
        spr.color = originalColor;
        invulnerable = false;
    }
}
