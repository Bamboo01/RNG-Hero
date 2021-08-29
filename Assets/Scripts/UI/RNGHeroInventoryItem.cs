using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;
using Bamboo.UI;
using Bamboo.Events;
using Bamboo.Utility;
using PlayFab;
using PlayFab.ClientModels;
using DG.Tweening;
using SimpleJSON;

public abstract class RNGHeroInventoryItem : MonoBehaviour
{
    public string itemInstanceID
    {
        get;
        private set;
    }
    public ItemInstance itemInstance
    {
        get;
        private set;
    }

    public virtual void Setup(ItemInstance item)
    {
        itemInstanceID = item.ItemInstanceId;
        itemInstance = item;
    }

    public abstract void OnClick();
}
