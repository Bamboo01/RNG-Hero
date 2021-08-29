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
    void GetMyTrades()
    {
        foreach(Transform a in MyTradeContent)
        {
            Destroy(a.gameObject);
        }
        TradeIDToTradeInfo.Clear();
        var request = new GetPlayerTradesRequest();
        PlayFabClientAPI.GetPlayerTrades(request, onGetMyTradesSuccess, PlayFabError);
    }

    void onGetMyTradesSuccess(GetPlayerTradesResponse result)
    {
        foreach (var a in result.AcceptedTrades)
        {
            GameObject go = Instantiate(TradeCellPrefab, MyTradeContent);
            go.GetComponent<TradeCell>().Setup(a);
            TradeIDToTradeInfo.Add(a.TradeId, a);
        }

        foreach (var a in result.OpenedTrades)
        {
            GameObject go = Instantiate(TradeCellPrefab, MyTradeContent);
            go.GetComponent<TradeCell>().Setup(a);
            TradeIDToTradeInfo.Add(a.TradeId, a);
        }

        GetTradeRequests();
    }

    void GetTradeRequests()
    {
        JSONArray arr = JSON.Parse(userData.Data["PlayerReceivedTradeRequests"].Value).AsArray;
        for (int i = 0; i < arr.Count; i++)
        {
            var request = new GetTradeStatusRequest();
            var obj = arr[i];
            request.OfferingPlayerId = obj["PlayerId"].Value;
            request.TradeId = obj["TradeId"].Value;

            PlayFabClientAPI.GetTradeStatus(request, onGetTradeRequestsSuccess, PlayFabError);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(MyTradeContent);
    }

    void onGetTradeRequestsSuccess(GetTradeStatusResponse response)
    {
        TradeIDToTradeInfo.Add(response.Trade.TradeId, response.Trade);
        GameObject go = Instantiate(TradeCellPrefab, MyTradeContent);
        go.GetComponent<TradeCell>().Setup(response.Trade);
    }

    public void AcceptTradeRequest(string tradeID)
    {
        UnityEvent ReEquipCallback = new UnityEvent();

        OpenLoadingMenu();
        var request = new AcceptTradeRequest();
        request.TradeId = tradeID;
        request.OfferingPlayerId = TradeIDToTradeInfo[tradeID].OfferingPlayerId;
        request.AcceptedInventoryInstanceIds = new List<string>();

        foreach (var requestedItemId in TradeIDToTradeInfo[tradeID].RequestedCatalogItemIds)
        {
            var foundItem = userInventory.Inventory.Find(
                (inventoryitem) =>
                {
                    return inventoryitem.ItemId == requestedItemId &&
                    (request.AcceptedInventoryInstanceIds.Find((item) => item == inventoryitem.ItemInstanceId )) == null;
                });
            if (foundItem == null)
            {
                OpenMessageMenu("You don't have to required items to accept this trade!", "", OpenMyTrades);
                return;
            }
            request.AcceptedInventoryInstanceIds.Add(foundItem.ItemInstanceId);
        }

        PlayFabClientAPI.AcceptTrade(request, onAcceptTradeRequestSuccess, (error) => { OpenMessageMenu("Error accepting trade!", error.ErrorMessage, OpenMyTrades); });
    }

    void onAcceptTradeRequestSuccess(AcceptTradeResponse response)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "OnAcceptTradeRequest",
            FunctionParameter = new
            {
                tradeID = response.Trade.TradeId
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(request, (result) => 
        {
            OpenMessageMenu("Trade accepted!", "", () =>
            {
                OpenLoadingMenu();
                ReloadUserData(() =>
                {
                    OpenMyTrades();
                });
            });
        }, PlayFabError);
    }

    public void CancelTradeRequest(string tradeID)
    {
        OpenLoadingMenu();
        var request = new CancelTradeRequest();
        request.TradeId = tradeID;
        PlayFabClientAPI.CancelTrade(request, onCancelTradeRequestSuccess, PlayFabError);
    }

    void onCancelTradeRequestSuccess(CancelTradeResponse response)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "OnCancelTradeRequest",
            FunctionParameter = new
            {
                friendID = response.Trade.AllowedPlayerIds[0],
                tradeID = response.Trade.TradeId
            }
        };

        PlayFabClientAPI.ExecuteCloudScript(request, (result) =>
        {
            OpenMessageMenu("Trade Cancelled!", "", () =>
            {
                OpenLoadingMenu();
                ReloadUserData(() =>
                {
                    OpenMyTrades();
                });
            });

        }, PlayFabError);
    }
}
