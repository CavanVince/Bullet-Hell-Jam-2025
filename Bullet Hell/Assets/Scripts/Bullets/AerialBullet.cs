using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AerialBullet : BaseBullet
{
    [SerializeField] private GameObject telegraphObject;
    [SerializeField] private GameObject bulletObject;

    // We don't need to declare rb here since it's inherited from BaseBullet
    private SpriteRenderer telegraphRenderer;


    [SerializeField] private float telegraphTime, activeTime, _unusedDamageRadius;

    void Awake()
    {
        telegraphTime = telegraphTime != 0 ? telegraphTime : 2f;
        activeTime = activeTime != 0 ? activeTime : .5f;


        if (telegraphObject == null)
            telegraphObject = transform.Find("Telegraph")?.gameObject;

        if (bulletObject == null)
            bulletObject = transform.Find("Bullet")?.gameObject;

        if (telegraphObject != null)
            telegraphRenderer = telegraphObject.GetComponent<SpriteRenderer>();

        // Initial setup - telegraph visible, bullet hidden
        if (telegraphObject != null)
            telegraphObject.SetActive(true);

        if (bulletObject != null)
        {
            bulletObject.SetActive(false);

            // Disable the rigidbody initially
            if (rb != null)
                rb.simulated = false;
        }
    }

    // Override the abstract methods from BaseBullet
    public void Fire(Vector2 destination)
    {
        // Start the telegraph animation
        StartCoroutine(TelegraphToBullet(destination));
    }

    private IEnumerator TelegraphToBullet(Vector2 destination)
    {
        if (telegraphRenderer == null || telegraphObject == null || bulletObject == null)
        {
            Debug.LogError("Telegraph or Bullet objects not properly assigned!");
            yield break;
        }

        // Start with transparent telegraph
        Color telegraphColor = telegraphRenderer.color;
        telegraphColor.a = 0f;
        telegraphRenderer.color = telegraphColor;

        // Gradually increase alpha over the delay period
        float elapsed = 0f;
        while (elapsed < telegraphTime)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / telegraphTime);
            telegraphColor.a = alpha;
            telegraphRenderer.color = telegraphColor;
            //Debug.Log($"Telegraph color: {telegraphRenderer.color}");
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure telegraph is fully visible at the end
        telegraphColor.a = 1f;
        telegraphRenderer.color = telegraphColor;

        // Hide telegraph, show bullet
        telegraphObject.SetActive(false);
        bulletObject.SetActive(true);
        Debug.Log("Hiding telegraph, showing bullet");
        // Activate the rigidbody
        if (rb != null)
        {
            rb.simulated = true;
            Debug.Log("Activating rb2d of bullet");
            // Calculate direction and set initial velocity
            if (destination != Vector2.zero)
            {
                Vector2 direction = (destination - (Vector2)transform.position).normalized;
                rb.velocity = direction * 10f; // You may want to adjust speed or make it configurable
            }
        }
        Debug.Log($"Waiting for {activeTime} seconds before destroying aerial bullet");
        // Wait for active time then destroy
        yield return new WaitForSeconds(activeTime);

        // Optional: fade out the bullet before destroying
        SpriteRenderer bulletRenderer = bulletObject.GetComponent<SpriteRenderer>();
        if (bulletRenderer != null)
        {
            float fadeElapsed = 0f;
            float fadeDuration = 0.25f;
            Color bulletColor = bulletRenderer.color;

            while (fadeElapsed < fadeDuration)
            {
                bulletColor.a = Mathf.Lerp(1f, 0f, fadeElapsed / fadeDuration);
                bulletRenderer.color = bulletColor;
                //Debug.Log($"Bullet color: {bulletColor}");
                fadeElapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}