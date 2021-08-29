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

public partial class GameScenePlayfab
{
    private string storeSelectedItemID = "";
    private string storeSelectedDisplayName;
    private string storeSelectedDisplayDescription;
    private uint storeItemCost;

    public void OnShopItemClick(StoreItem storeItem)
    {
        storeSelectedItemID = storeItem.ItemId;
        storeItemCost = storeItem.VirtualCurrencyPrices["RN"];
        storeSelectedDisplayName = CatalogItemIdToCatalogItem[storeSelectedItemID].DisplayName;
        storeSelectedDisplayDescription = "Cost: " + storeItemCost.ToString() + "\n" + StoreItemIdToCustomData[storeSelectedItemID]["Description"];
        ShopTitleText.text = storeSelectedDisplayName;
        ShopDescriptionText.text = storeSelectedDisplayDescription;
    }

    public void OnStoreBuyClick()
    {
        if (storeSelectedItemID.Length == 0)
        {
            return;
        }

        OpenLoadingMenu();
        PlayFabClientAPI.PurchaseItem(
        new PurchaseItemRequest
        {
            // In your game, this should just be a constant matching your primary catalog
            CatalogVersion = "1.00",
            ItemId = storeSelectedItemID,
            VirtualCurrency = "RN",
            Price = (int)storeItemCost
        },
        onBuySuccess,
        onBuyFail
        );
    }

    public void onBuySuccess(PurchaseItemResult result)
    {
        ReloadUserData(OpenStoreMenu);
    }

    public void onBuyFail(PlayFabError error)
    {
        OpenMessageMenu("Failed to purchase item!", error.ErrorMessage, OpenStoreMenu);
    }

    void CreateStoreItems()
    {
        foreach (Transform a in StoreContentTransform)
        {
            Destroy(a.gameObject);
        }

        foreach (var a in storeData.Store)
        {
            GameObject item = Instantiate(StoreItemPrefab, StoreContentTransform);
            var storeitem = item.GetComponent<RNGHeroStoreItem>();
            storeitem.Setup(a);
        }
    }
}