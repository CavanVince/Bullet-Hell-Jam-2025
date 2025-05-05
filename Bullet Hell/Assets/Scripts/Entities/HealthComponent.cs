using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField]
    protected int health;

    protected bool invulnerable;
    
    [SerializeField]
    private float invulnerabilityDurationOnHit;

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
        {
            Debug.Log($"{transform.name} is Invulnerable. No Damage taken.");
            return;
        }

        health -= damage;
        Debug.Log($"{transform.name} took {damage} damage");

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
        StartCoroutine(StartInvulnerability());
    }

    IEnumerator StartInvulnerability()
    {
        //Debug.Log($"Setting {transform.name} invulnerable for {invulnerabilityDurationOnHit}s");
        SpriteRenderer spr = GetComponentInChildren<SpriteRenderer>();
        Color originalColor = spr.color;
        Color invulnColor = Color.cyan;

        if (invulnerabilityDurationOnHit > 0)
        {
            invulnerable = true;
        }

        // flicker 'hit' color
        spr.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if (invulnerabilityDurationOnHit > 0)
        {
            spr.color = invulnColor;

            // show invuln color until no longer invulnerable
            yield return new WaitForSeconds(invulnerabilityDurationOnHit);
        }
        spr.color = originalColor;
        invulnerable = false;
    }
}
