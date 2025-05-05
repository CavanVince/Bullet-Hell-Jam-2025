using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AttackPatterns : MonoBehaviour
{    
    public void Pulse(Action shootFunction, float interval, float duration)
    {
        IEnumerator PulseEvery(Action shootFunction, float interval, float duration)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                shootFunction();
                yield return new WaitForSeconds(interval);
                elapsedTime += interval;
            }
        }

        StartCoroutine(PulseEvery(shootFunction, interval, duration));
    }

    public void Circle(Action shootFunction, Vector2 origin, float radius)
    {

    }
    
}
