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
    void CreateInventoryItems()
    {
        foreach(Transform a in InventoryContentTransform)
        {
            Destroy(a.gameObject);
        }

        ItemInstanceIDToItem.Clear();
        ItemInstanceIDToInventoryGameObject.Clear();
        foreach (var a in userInventory.Inventory)
        {
            ItemInstanceIDToItem.Add(a.ItemInstanceId, a);
            GameObject item;
            if (CatalogItemIdToCatalogItem[a.ItemId].Consumable.UsageCount.HasValue == false)
            {
                item = Instantiate(EquippablePrefab, InventoryContentTransform);
            }
            else
            {
                item = Instantiate(ConsumablePrefab, InventoryContentTransform);
            }

            var equippable = item.GetComponent<RNGHeroInventoryItem>();
            equippable.Setup(a);
            ItemInstanceIDToInventoryGameObject.Add(a.ItemInstanceId, item);
        }

        ItemInstanceIDToInventoryGameObject[userData.Data["EquippedSword"].Value].GetComponent<Button>().image.color = Color.red;
        ItemInstanceIDToInventoryGameObject[userData.Data["EquippedAmulet"].Value].GetComponent<Button>().image.color = Color.blue;
        ItemInstanceIDToInventoryGameObject[userData.Data["EquippedTreasureFinder"].Value].GetComponent<Button>().image.color = Color.green;
    }

    public void EquipItem(ItemInstance itemInstance)
    {
        OpenLoadingMenu();
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "EquipItem",
            FunctionParameter = new
            {
                ItemInstanceID = itemInstance.ItemInstanceId,
                EquipSlot = GetCatalogItemCustomData(itemInstance.ItemId)["EquipSlot"].Value
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(request, onEquipSuccess, PlayFabError);
    }

    void onEquipSuccess(ExecuteCloudScriptResult result)
    {
        ReloadUserData(OpenInventoryMenu);
    }

    public void ForceEquipItem(ItemInstance itemInstance, UnityAction action)
    {
        OpenLoadingMenu();
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "EquipItem",
            FunctionParameter = new
            {
                ItemInstanceID = itemInstance.ItemInstanceId,
                EquipSlot = GetCatalogItemCustomData(itemInstance.ItemId)["EquipSlot"]
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(request, (result)=> { action.Invoke(); }, PlayFabError);
    }
}
