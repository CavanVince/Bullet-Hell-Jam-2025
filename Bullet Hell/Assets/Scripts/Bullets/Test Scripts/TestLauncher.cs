using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLauncher : MonoBehaviour
{
    AttackPatterns ap;
    private bool shooting;
    private void Start()
    {
        ap = GetComponent<AttackPatterns>();
    }

    private void Update()
    {
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Running DaOctopus");
            StartCoroutine(ap.DaOctopus(() => enemy.transform.position, 5, 5, 1f, null, 0, null));
        }

        else if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Running Diverging Radial");
            if (!shooting)
            {
                shooting = true;
                StartCoroutine(ap.DivergingRadial(() => enemy.transform.position, 8, 5, 1f, null, null, 0, () => shooting = false));
            }
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("Running Shoot at Vector2.zero");
            StartCoroutine(ap.Shoot(() => enemy.transform.position, () => Vector2.zero));
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("Running Shotgun shot");
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
