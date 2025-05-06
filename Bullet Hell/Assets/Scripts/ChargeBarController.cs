using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class ChargeBarController : MonoBehaviour
{
    
    private int currChargeBarLevel;
    private int finalChargeBarLevel;
    public float chargeBarSpeed = 0.1f;
    
    public int StartChargeBarLevel()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Power-Bar");
        currChargeBarLevel = 0;
        StartCoroutine(ChargeBar());
        return finalChargeBarLevel;
    }

    private IEnumerator ChargeBar()
    {
        while(currChargeBarLevel <= 11)
        {
            currChargeBarLevel++;
            chargeBarSpeed -= 0.01f;

            yield return new WaitForSeconds(chargeBarSpeed);
        }
    }
}
