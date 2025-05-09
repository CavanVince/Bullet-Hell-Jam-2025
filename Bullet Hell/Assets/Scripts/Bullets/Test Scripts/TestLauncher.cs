using UnityEngine;

public class TestLauncher : MonoBehaviour
{
    AttackPatterns ap;
    private bool shooting;
    private void Start()
    {
        ap = new AttackPatterns(BulletManager.instance);
    }

    private void Update()
    {
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(ap.DaOctopus(() => enemy.transform.position, 5, 5, 1f, null, 0, null));
        }

        else if (Input.GetKeyDown(KeyCode.T))
        {
            if (!shooting)
            {
                shooting = true;
                StartCoroutine(ap.DivergingRadial(() => enemy.transform.position, 8, 5, 1f, null, null, 0, () => shooting = false));
            }
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            StartCoroutine(ap.Shoot(() => enemy.transform.position, () => Vector2.zero));
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            StartCoroutine(ap.RadialAerialAroundPlayer(() => transform.position, 5, 1, 0, null));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            Vector2 start = transform.position;
            StartCoroutine(ap.WalkDaLineAerial(() => start, () => start + new Vector2(0, 10), 1, .05f, 0, () =>
            {
                start = start + new Vector2(1, 10);
                StartCoroutine(ap.WalkDaLineAerial(() => start, () => start - new Vector2(0, -10), 1, .05f));
            }));
        }
    }
}
