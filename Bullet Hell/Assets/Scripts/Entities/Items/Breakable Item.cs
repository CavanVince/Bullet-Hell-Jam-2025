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
    void Start()
    {
        gameObjectSprite = GetComponent<SpriteRenderer>().sprite.name.Split('_')[0];
        sprites = Resources.LoadAll<Sprite>(gameObjectSprite);
        
    }
    public void Break()
    {
        index++;
        GetComponent<SpriteRenderer>().sprite = sprites[index];
        if (index == sprites.Length - 1)
        {
            StartCoroutine(Destroy());
        }
    
    }
    IEnumerator Destroy()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject.layer);
        if (collision.gameObject.layer == BulletHellCommon.BULLET_LAYER || collision.gameObject.layer == BulletHellCommon.PLAYER_PROJECTILE_LAYER || collision.gameObject.layer == BulletHellCommon.ENEMY_LAYER)
        {
            Debug.Log("hit");
            Break();
        }
    }
}
