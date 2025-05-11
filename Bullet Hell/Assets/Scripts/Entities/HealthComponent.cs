using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour
{

    public int maxHealth = 5;

    public int defaultStartingHealth = 3;

    [HideInInspector]
    public int health;

    protected bool invulnerable;
    
    [SerializeField]
    private float invulnerabilityDurationOnHit;

    SpriteRenderer spr;
    Color originalColor;

    void Start()
    {
        spr = GetComponentInChildren<SpriteRenderer>();
        originalColor = spr.color;
        health = defaultStartingHealth;
        invulnerable = false;
    }


    public bool Heal()
    {
        return Heal(1);
    }
    public bool Heal(int amount)
    {
        if (health + amount > maxHealth)
            return false;

        health += amount;
        return true;
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
                GetComponent<BaseEnemy>()?.OwningRoom?.EnemyDied();
                EntityManager.instance.Repool(gameObject);
                return;
            }
        }
        StartCoroutine(StartInvulnerability());
    }

    IEnumerator StartInvulnerability()
    {
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

    public void ResetState()
    {
        if (spr != null)
            spr.color = originalColor;
        Start();
    }
}
