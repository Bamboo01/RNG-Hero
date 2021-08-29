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
    private string currentTradeID = "";

    void CreateTradeItems()
    {
        foreach (Transform a in YourInventoryContent)
        {
            Destroy(a.gameObject);
        }

        foreach (Transform a in FriendInventoryContent)
        {
            Destroy(a.gameObject);
        }

        foreach (var item in userInventory.Inventory)
        {
            if (CatalogItemIdToCatalogItem[item.ItemId].IsTradable)
            {
                if (item.ItemInstanceId == userData.Data["EquippedAmulet"].Value            ||
                    item.ItemInstanceId == userData.Data["EquippedSword"].Value             ||
                    item.ItemInstanceId == userData.Data["EquippedTreasureFinder"].Value
                    )
                {
                    continue;
                }
                GameObject go = Instantiate(TradeInventoryCellPrefab, YourInventoryContent);
                go.GetComponent<TradeInventoryItem>().Setup(item.ItemId, item.ItemInstanceId);
            }
        }

        foreach (var kvp in FriendInventory)
        {
            var item = kvp.Value;
            if (CatalogItemIdToCatalogItem[item["ItemId"].Value].IsTradable)
            {
                GameObject go = Instantiate(FriendTradeInventoryCellPrefab, FriendInventoryContent);
                go.GetComponent<TradeFriendInventoryItem>().Setup(item["ItemId"].Value, kvp.Key);
            }
        }
    }

    void GetFriendInventory()
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "GetOthersInventory",
            FunctionParameter = new
            {
                ID = ActiveFriendID
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(request, onGetFriendInventorySuccess, onGetFriendInventoryFail);
    }

    void onGetFriendInventorySuccess(ExecuteCloudScriptResult result)
    {
        FriendInventory.Clear();
        var resultJSON = JSON.Parse((string)result.FunctionResult).AsObject;
        
        foreach (KeyValuePair<string, JSONNode> a in resultJSON)
        {
            FriendInventory.Add(a.Key, a.Value);
        }

        CreateTradeItems();

        menuManager.OnlyOpenThisMenu("FriendTradeMenu");
        menuManager.OpenMenu("BackButtonMenu");
        OpenBackButton(OpenFriendsMenu);
    }

    void onGetFriendInventoryFail(PlayFabError error)
    {
        OpenMessageMenu("Failed to open trade with friend!", error.ErrorMessage, OpenFriendsMenu);
    }

    void SendTrade()
    {
        // I have depression
        OpenLoadingMenu();
        if (YourSelectedTradeItems.Count == 0 && FriendSelectedTradeItems.Count == 0)
        {
            return;
        }

        List<string> offeredInstanceIDs = new List<string>();
        List<string> requestedItemIDs = new List<string>();

        if (YourSelectedTradeItems.Count != 0)
        {
            foreach (var kvp in YourSelectedTradeItems)
            {
                offeredInstanceIDs.Add(kvp.Key);
            }
        }

        if (FriendSelectedTradeItems.Count != 0)
        {
            foreach (var kvp in FriendSelectedTradeItems)
            {
                requestedItemIDs.Add(kvp.Value.ItemId);
            }
        }

        // Client sends a request to trade, upon success, will execute a cloudscript to let the receiving player know the
        // Trade ID, which will be added to userdata in order to facilitate the reception of trade requests
        // aka playfab trading api sucks dick
        // Also my rules are down so I have no clue if I can even call a playstream callback upon trade request LOL
        var request = new OpenTradeRequest
        {
            AllowedPlayerIds = new List<string> { ActiveFriendID },
            OfferedInventoryInstanceIds = new List<string>(offeredInstanceIDs),
            RequestedCatalogItemIds = new List<string>(requestedItemIDs)
        };

        PlayFabClientAPI.OpenTrade(request, onOpenTradeSuccess, onOpenTradeFail);
    }

    void onOpenTradeSuccess(OpenTradeResponse response)
    {
        // MFW TRADE EVENT ON PLAYFAB DUN WORK AHA
        UpdateOtherPlayerRequests(response);
    }

    void onOpenTradeFail(PlayFabError error)
    {
        OpenMessageMenu("Failed to send trade request!", error.ErrorMessage, OpenFriendsMenu);
    }

    void UpdateOtherPlayerRequests(OpenTradeResponse tradeResponse)
    {
        currentTradeID = tradeResponse.Trade.TradeId;
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "OnSendTradeRequest",
            FunctionParameter = new
            {
                friendID = ActiveFriendID,
                tradeID = tradeResponse.Trade.TradeId
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(request, onUpdateOtherPlayerRequestsSuccess, onUpdateOtherPlayerRequestsFail);
    }

    void onUpdateOtherPlayerRequestsSuccess(ExecuteCloudScriptResult response)
    {
        if (JSON.Parse(response.FunctionResult.ToString())["code"].Value != "ERROR")
        {
            ReloadUserData(() =>
            {
                OpenMessageMenu("Successfully sent trade request!", "", OpenFriendsMenu);
            });
        }
        else
        {
            OpenMessageMenu("Failed to send trade request!", "", ()=> { CancelTrade(currentTradeID); });
        }
    }

    void onUpdateOtherPlayerRequestsFail(PlayFabError error)
    {
        OpenMessageMenu("Failed to send trade request!", error.ErrorMessage, () => { CancelTrade(currentTradeID); });
    }

    void CancelTrade(string tradeID)
    {
        OpenLoadingMenu();
        var request = new CancelTradeRequest();
        request.TradeId = tradeID;
        PlayFabClientAPI.CancelTrade(request, onCancelTradeSuccess, PlayFabError);
    }

    void onCancelTradeSuccess(CancelTradeResponse response)
    {
        OpenFriendsMenu();
    }
}
