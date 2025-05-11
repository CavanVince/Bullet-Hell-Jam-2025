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

    [SerializeField] private const float telegraphTime=2f, activeTime=.5f;

    void Awake()
    {
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
    public void Fire(Vector2 startPos, Vector2 _unused_destination, float moveSpeed, Func<float, float> moveFunc, float telegraphTime=telegraphTime, float activeTime=activeTime)
    {
        // Start the telegraph animation
        StartCoroutine(TelegraphToBullet(startPos));
    }

    public override void Fire(Vector2 startPos, Vector2 _unused_destination, float moveSpeed, Func<float, float> moveFunc)
    {
        Fire(startPos, _unused_destination, moveSpeed, moveFunc);
    }

    private IEnumerator TelegraphToBullet(Vector2 origin)
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

        // gradually increase alpha over the delay period, replace with ani
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

        telegraphColor.a = 1f;
        telegraphRenderer.color = telegraphColor;

        // hide telegraph, show bullet
        telegraphObject.SetActive(false);
        bulletObject.SetActive(true);
        Debug.Log("Hiding telegraph, showing bullet");
        if (rb != null)
        {
            rb.simulated = true;
            // calculate direction and set initial velocity
            if (origin != Vector2.zero)
            {
                Vector2 direction = (origin - (Vector2)transform.position).normalized;
                rb.velocity = direction * 10f;
            }
        }

        Debug.Log($"Waiting for {activeTime} seconds before destroying aerial bullet");
        yield return new WaitForSeconds(activeTime);

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
                fadeElapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        telegraphObject.SetActive(true);
        bulletObject.SetActive(false);
        gameObject.SetActive(false);
    }
}