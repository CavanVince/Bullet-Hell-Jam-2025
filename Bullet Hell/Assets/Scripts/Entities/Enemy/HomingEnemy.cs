using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class HomingEnemy : BaseEnemy
{
    [SerializeField]
    private float explosionRadius;
    [SerializeField]
    private int explosionDamage;
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, 3f * Time.deltaTime);
    }

    protected override void ShootPlayer()
    {

         
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GetComponent<CircleCollider2D>().radius = explosionRadius;
          
    }
}
