using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

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
    public GameObject heartContainer;
    

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
    IEnumerator PlayHitSound()
    {
        AudioClip hitSound;
        AudioClip hitsound1 = Resources.Load<AudioClip>("Sounds/WallHitOrc1");
        AudioClip hitsound2 = Resources.Load<AudioClip>("Sounds/WallHitOrc2");
        float rand = Random.Range(0f, 1f);
        if (rand < .5f)
        {
            hitSound = hitsound1;
        }
        else
        {
            hitSound = hitsound2;
        }
        GetComponent<AudioSource>().PlayOneShot(hitSound);
        yield return new WaitForSeconds(hitSound.length);
    }
    public void TakeDamage(int damage)
    {
        float random = Random.Range(0f, 1f);
        if (invulnerable)
        {
            Debug.Log($"{transform.name} is Invulnerable. No Damage taken.");
            return;
        }

        health -= damage;

        if (gameObject.CompareTag("Player"))
        {
            //Render stuff
            FullScreenPassRendererFeature renderFeature = Camera.main.GetComponent<FollowCamera>().RenderFeature;
            renderFeature.passMaterial = new Material(renderFeature.passMaterial);

            float distortVal = (defaultStartingHealth - health) / (float)defaultStartingHealth * 0.25f;
            distortVal = Mathf.Clamp(distortVal, 0f, 0.25f);
            renderFeature.passMaterial.SetFloat("_Distort_Intensity", distortVal);
        }
        if (gameObject.layer == BulletHellCommon.ENEMY_LAYER)
        {
            StartCoroutine(PlayHitSound());
        }


        //Debug.Log($"{transform.name} took {damage} damage");

        if (health <= 0)
        {
            Debug.Log($"{transform.name} took lethal damage!");
            if (gameObject.tag == "Player")
            {
                //Debug.Log("YOU DIED. GAME OVER SCREEN WOULD GO HERE");
                SceneManager.LoadScene("Map Generation");
            }
            else
            {
                if (random < .33f)
                {
                    Instantiate(heartContainer, transform.position, transform.rotation);
                }
                GetComponent<BaseEnemy>()?.OwningRoom?.EnemyDied();
                EntityManager.instance.Repool(gameObject);
                return;
            }
        }
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(StartInvulnerability());
        }
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
