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


public class InventoryConsumable : RNGHeroInventoryItem
{
    [SerializeField] TMP_Text buttonText;
    [SerializeField] Button button;

    void OnEnable()
    {
        button.onClick.AddListener(OnClick);
    }

    void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }

    public override void Setup(ItemInstance item)
    {
        base.Setup(item);
        buttonText.text = item.DisplayName;
        button.image.color = Color.magenta;
    }

    public override void OnClick()
    {
        GameScenePlayfab.Instance.OpenLoadingMenu();
        PlayFabClientAPI.ConsumeItem(
            new ConsumeItemRequest
            {
                ConsumeCount = 1,
                ItemInstanceId = itemInstanceID
            },
            onConsumeSuccess,
            GameScenePlayfab.Instance.PlayFabError);
    }

    public void onConsumeSuccess(ConsumeItemResult result)
    {
        GameScenePlayfab.Instance.OpenMessageMenu("Potion consumed!", GameScenePlayfab.Instance.GetCatalogItemCustomData(itemInstance.ItemId)["ConsumptionDescription"], ()=> { GameScenePlayfab.Instance.ReloadUserData(GameScenePlayfab.Instance.OpenInventoryMenu); });
    }
}
