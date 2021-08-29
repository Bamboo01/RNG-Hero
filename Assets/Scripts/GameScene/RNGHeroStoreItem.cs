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

public class RNGHeroStoreItem : MonoBehaviour
{
    [SerializeField] TMP_Text buttonText;
    [SerializeField] Button button;

    public string itemID
    {
        get;
        private set;
    }
    public StoreItem storeItem
    {
        get;
        private set;
    }

    public virtual void Setup(StoreItem item)
    {
        itemID = item.ItemId;
        storeItem = item;
        button.onClick.AddListener(onClick);
        buttonText.text = GameScenePlayfab.Instance.GetCatalogItem(itemID).DisplayName;
    }

    public void onClick()
    {
        GameScenePlayfab.Instance.OnShopItemClick(storeItem);
    }
}
