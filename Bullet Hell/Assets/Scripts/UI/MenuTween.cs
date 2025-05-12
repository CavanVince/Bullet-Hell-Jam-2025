using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuTween : MonoBehaviour
{
    void Start()
    {
        Vector3 origScale = transform.localScale;
        transform.DOScale(origScale * 1.5f, 3).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
