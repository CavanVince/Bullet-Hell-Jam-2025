using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombEnemy : BaseEnemy
{
    [SerializeField]
    private float distanceFromPlayerToExplode = 2f;

    [SerializeField]
    private float timeToExplode_s = .5f;

    public float bombRadius = 3f;
    public int explosionDamage = 1;

    GameObject explosionObject;

    protected override void Start()
    {
        base.Start();
        // there's probably a better way to do this
        for(int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.tag == "ExplosionRadius")
            {
                explosionObject = transform.GetChild(i).gameObject;
                break;
            }
        }
    }

    protected override void OnAggro()
    {
        if (enemyState == EnemyState.EXPLODING) return;

        // move towards player
        if (Vector2.Distance(player.transform.position, transform.position) <= distanceFromPlayerToExplode)
        {
            StartCoroutine(Explode());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.gameObject.transform.parent.GetComponent<BaseEntity>().healthComponent.TakeDamage(explosionDamage);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnTriggerEnter2D(collision.collider);
    }

    private IEnumerator Explode()
    {
        enemyState = EnemyState.EXPLODING;
        GetComponentInChildren<SpriteRenderer>().color = Color.cyan;
        yield return new WaitForSeconds(timeToExplode_s);

        explosionObject.GetComponent<CircleCollider2D>().radius = bombRadius;
        yield return new WaitForFixedUpdate();
        enemyState = EnemyState.IDLE;
        healthComponent.TakeDamage(999);
        //EntityManager.instance.Repool(gameObject);
        Destroy(gameObject);
    }
}
