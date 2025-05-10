using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    public static EntityManager instance;
    private Dictionary<Type, List<GameObject>> entityPools;

    [SerializeField]
    private int maxPoolSizePerInstance = 100;

    // Bullets
    [SerializeField]
    private GameObject standardBulletPrefab, aerialBulletPrefab;

    // Enemies
    [SerializeField]
    private GameObject basicEnemyPrefab, bombEnemyPrefab;

    [SerializeField]
    private int poolSize;
    public Sprite[] bulletSprites;

    private void Start()
    {
        SummonEnemy(typeof(BaseEnemy), new Vector2(0, 2), Quaternion.identity);
        bulletSprites = Resources.LoadAll<Sprite>("Bullet");
    }

    /// <summary>
    /// Grabs an available bullet from the pool and launches it in the specified direction
    /// </summary>
    /// <param name="origin">Starting position of the bullet</param>
    /// <param name="direction">Direction the bullet is launched in</param>
    public void FireBullet(Type bulletType, Vector2 origin, Vector2 destination, Func<float, float> movementFunc = null)
    {
        GameObject go = SummonEntity(bulletType);
        go.layer = BulletHellCommon.BULLET_LAYER;
        go.transform.position = origin;

        BaseBullet bullet = go.GetComponent<BaseBullet>();
        bullet.damage = bullet.baseDamage;
        go.SetActive(true);
        bullet.Fire(origin, destination, bullet.moveSpeed, movementFunc);
    }

    public void FireBullet(Type bulletType, Vector2 origin, Func<float, float> movementFunc = null)
    {
        FireBullet(bulletType, origin, Vector2.zero, movementFunc);
    }

    /// <summary>
    /// Summons an entity from the pool and places them in the world at a given position and rotation.
    /// </summary>
    public GameObject SummonEnemy(Type type, Vector2 position, Quaternion rotation)
    {
        GameObject go = SummonEntity(type);
        BaseEntity entity = go.GetComponent<BaseEntity>();

        if (entity == null)
        {
            throw new Exception($"Type {type} has no related script that inherits from BaseEntity attached to it");
        }

        go.transform.position = position;
        go.transform.rotation = rotation;
        go.SetActive(true);
        return go;
    }

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;

            entityPools = new Dictionary<Type, List<GameObject>>()
            {
                { typeof(StandardBullet),  new List<GameObject>(poolSize) },
                { typeof(AerialBullet), new List<GameObject>(poolSize) },
                { typeof(BaseEnemy), new List<GameObject>(poolSize) },
                { typeof(BombEnemy), new List<GameObject>(poolSize) }
            };
            InitPools();

            // pull any enemies from the hierarchy to be managed by this
            
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject GetEntityClosestTo(GameObject origin, Type entityType)
    {
        GameObject closest = null;
        float distance = 0f;
        foreach(GameObject go in entityPools[entityType]) {
            if (!go.activeInHierarchy) continue;

            float newDist = Vector2.Distance(origin.transform.position, go.transform.position);
            if (closest == null || newDist < distance)
            {
                closest = go;
                distance = newDist;
            }
        }

        return closest;
    }

    /// <summary>
    /// Pulls the next available entity of the given type from its pool. 
    /// If none are available, it will return a new instantiated, unpooled object of the given type.
    /// </summary>
    /// <param name="type">Type of object to retrieve from pool</param>
    /// <returns></returns>
    private GameObject SummonEntity(Type type)
    {
        if (!entityPools.ContainsKey(type))
        {
            throw new Exception($"Type {type} does not have an object pool to summon from");
        }
        // Check pool first and use first unused if any
        foreach (GameObject go in entityPools[type])
        {
            if (!go.activeInHierarchy)
            {
                return go;
            }
        }
        // Otherwise, spawn a new one
        GameObject spawnedObj = Spawn(type);
        // TODO: if we want to pool this newly spawned entity, that would go here.
        List<GameObject> list = entityPools[type];
        if (list.Count < maxPoolSizePerInstance)
            list.Add(spawnedObj);
        return spawnedObj;
    }

    /// <summary>
    /// Return a newly instantiated prefab of a given type
    /// </summary>
    /// <param name="type">The type of prefab to return</param>
    /// <returns>Gameobject prefab of the newly instantiated prefab</returns>
    /// <exception cref="Exception">Thrown if the given type is not implemented in the function.</exception>
    private GameObject Spawn(Type type)
    {
        GameObject spawnedObject;
        if (type == typeof(StandardBullet))
        {
            spawnedObject = standardBulletPrefab;
        }
        else if (type == typeof(AerialBullet))
        {
            spawnedObject = aerialBulletPrefab;
        }
        else if (type == typeof(BaseEnemy))
        {
            spawnedObject = basicEnemyPrefab;
        }
        else if (type == typeof(BombEnemy))
        {
            spawnedObject = bombEnemyPrefab;
        }
        else
        {
            throw new Exception($"Encountered an unhandled type of object to spawn: {type}");
        }
        spawnedObject = Instantiate(spawnedObject, transform);
        spawnedObject.SetActive(false);
        return spawnedObject;
    }

    /// <summary>
    /// Initialize each pool of prefabs by object type
    /// </summary>
    private void InitPools()
    {
        GameObject spawnedObject;

        foreach (KeyValuePair<Type, List<GameObject>> type_list in entityPools)
        {

            for (int i = 0; i < poolSize; i++)
            {
                spawnedObject = Spawn(type_list.Key);
                spawnedObject.SetActive(false);

                entityPools[type_list.Key].Add(spawnedObject);
            }
        }
    }

    public void Repool(GameObject gameObj)
    {

        BaseBullet bullet = gameObj.GetComponent<BaseBullet>();
        BaseEntity entity = gameObj.GetComponent<BaseEntity>();

        if (bullet != null)
        {
            bullet.ResetState();
        }
        else if (entity != null)
        {
            entity.ResetState();
        }
        else
        {
            throw new Exception($"Unknown gameobject to repool {gameObj.transform.name}");
        }
        gameObj.SetActive(false);

    }
}
