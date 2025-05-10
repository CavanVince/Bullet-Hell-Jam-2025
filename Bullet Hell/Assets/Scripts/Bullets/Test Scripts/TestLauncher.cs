using UnityEngine;

public class TestLauncher : MonoBehaviour
{
    AttackPatterns ap;
    GameObject nearestEnemy;
    private void Start()
    {
        ap = new AttackPatterns(EntityManager.instance);
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {

            if (Input.GetKeyDown(KeyCode.F))
            {
                EntityManager.instance.FireBullet(typeof(AerialBullet), Camera.main.ScreenToWorldPoint(Input.mousePosition));
                return;
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                EntityManager.instance.SummonEnemy(typeof(BombEnemy), Camera.main.ScreenToWorldPoint(Input.mousePosition), Quaternion.identity);
                return;
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                EntityManager.instance.SummonEnemy(typeof(BaseEnemy), Camera.main.ScreenToWorldPoint(Input.mousePosition), Quaternion.identity);
                return;
            }

            nearestEnemy = EntityManager.instance.GetEntityClosestTo(gameObject, typeof(BaseEnemy));
            if (nearestEnemy == null)
            {
                Debug.Log("Enemy not found");
                return;
            }
            BaseEnemy enemy = nearestEnemy.GetComponent<BaseEnemy>();
            if (enemy == null)
            {
                Debug.Log("BaseEnemy component not found");
                return;
            }

            ShootParameters sp = new ShootParameters(originCalculation: () => enemy.transform.position, destinationCalculation: () => enemy.player.transform.position, numBullets: 5, pulseCount: 5, pulseInterval_s: .5f, cooldown: 1f);
            if (Input.GetKeyDown(KeyCode.R))
            {
                enemy.shootFunc = () => ap.DaOctopus(sp);
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                enemy.shootFunc = () => ap.DivergingRadial(sp);
            }
            else if (Input.GetKeyDown(KeyCode.Y))
            {
                enemy.shootFunc = () => ap.Shoot(sp);
            }
            nearestEnemy = null;
        }
    }
}
