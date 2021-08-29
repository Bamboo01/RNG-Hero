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

public class TradeFriendInventoryItem : MonoBehaviour
{
    [SerializeField] TMP_Text buttonText;
    [SerializeField] Button button;
    public string ItemInstanceId;
    public string ItemId;
    public bool isSelected = false;
    public void Setup(string itemid, string instanceid)
    {
        buttonText.text = GameScenePlayfab.Instance.GetCatalogItem(itemid).DisplayName;
        ItemInstanceId = instanceid;
        ItemId = itemid;
        button.onClick.AddListener(onClick);
    }

    public void onClick()
    {
        isSelected = !isSelected;
        button.image.color = isSelected ? Color.green : Color.white;
        GameScenePlayfab.Instance.AddToFriendTradeSelectedItem(ItemInstanceId, !isSelected, this);
    }
}
