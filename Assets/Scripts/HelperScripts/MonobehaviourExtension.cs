using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bamboo.Utility;

public class CoroutineManager : Singleton<CoroutineManager>
{
    // YO NICE CHEESE LOL
    void Awake()
    {
        _persistent = true;
    }
}

public static class GameObjectExtensions
{
    public static void SetActiveDelayed(this GameObject go, bool a, float t)
    {
        CoroutineManager.Instance.StartCoroutine(_SetActiveDelayed(t, a, go));
    }

    private static IEnumerator _SetActiveDelayed(float t, bool a, GameObject go)
    {
        yield return new WaitForSeconds(t);
        go.SetActive(a);
    }
}
