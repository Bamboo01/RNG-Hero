using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bamboo.UI;
using DG.Tweening;

public class RNGHeroMenu : Menu
{
    public RectTransform rt;
    public Vector3 basePosition;
    public Vector3 hiddenPosition = new Vector3(-800, 0, 0);
    public float tweenDuration = 0.5f;
    public override void OnAwake()
    {
        base.OnAwake();
        rt = GetComponent<RectTransform>();
        basePosition = rt.localPosition;
    }
    public override void Open()
    {
        rt.anchoredPosition = hiddenPosition;
        rt.DOAnchorPos(basePosition, tweenDuration).OnPlay(()=> { this.gameObject.SetActive(true); }).OnComplete(()=> { OnMenuOpen?.Invoke(); });
    }

    public override void Close()
    {
        if (!gameObject.activeInHierarchy)
        {
            rt.anchoredPosition = basePosition;
            return;
        }
        rt.DOAnchorPos(hiddenPosition, tweenDuration).OnComplete(()=> { base.Close(); });
    }
}
