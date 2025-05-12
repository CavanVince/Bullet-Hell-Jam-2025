using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableItem : BaseEntity
{
    // Start is called before the first frame update
    
    String gameObjectSprite;
    public Sprite[] sprites;
    int index = 0;
    public GameObject heartContainer;
    void Start()
    {
        gameObjectSprite = GetComponent<SpriteRenderer>().sprite.name.Split('_')[0];
        sprites = Resources.LoadAll<Sprite>(gameObjectSprite);
        
    }
    public void Break()
    {
        float random = UnityEngine.Random.Range(0f, 1f);

        index++;
        GetComponent<SpriteRenderer>().sprite = sprites[index];
        transform.GetComponent<AudioSource>().Play();
        if (index == sprites.Length - 1)
        {
            GetComponentInChildren<Collider2D>().enabled = false;
        }
        if (random < .5f)
        {
            Instantiate(heartContainer, transform.position, transform.rotation);
        }
    
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == BulletHellCommon.BULLET_LAYER || collision.gameObject.layer == BulletHellCommon.PLAYER_PROJECTILE_LAYER || collision.gameObject.layer == BulletHellCommon.ENEMY_LAYER)
        {
            Break();
        }
    }
}
