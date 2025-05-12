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

    Color spriteColor;

    protected override void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.tag == "ExplosionRadius")
            {
                explosionObject = transform.GetChild(i).gameObject;
                break;
            }
        }
        spriteColor = GetComponentInChildren<SpriteRenderer>().color;
        base.Start();
    }

    protected override void Update()
    {
        if (enemyState == EnemyState.EXPLODING) return;


        if (Vector2.Distance(player.transform.position, transform.position) <= distanceFromPlayerToExplode)
        {
            // explosion state triggered
            StartCoroutine(Explode());
        }
        else
        {
            // move towards player

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        BaseEntity entity = collision.gameObject.transform.parent.GetComponentInParent<BaseEntity>();
        if (entity != null)
        {
            entity?.healthComponent?.TakeDamage(explosionDamage);
        }
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
        OwningRoom.EnemyDied(gameObject);
        EntityManager.instance.Repool(gameObject);
    }

    public override void ResetState()
    {
        base.ResetState();
        explosionObject.GetComponent<CircleCollider2D>().radius = .5f;
        GetComponentInChildren<SpriteRenderer>().color = spriteColor;
        enemyState = EnemyState.IDLE;

    }
}
