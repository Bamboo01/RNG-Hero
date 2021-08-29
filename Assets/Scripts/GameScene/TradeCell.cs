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
public class TradeCell : MonoBehaviour
{
   [SerializeField] GameObject ItemTextPrefab;
   [SerializeField] TMP_Text TradeTitle;
   [SerializeField] TMP_Text Player1Title;
   [SerializeField] TMP_Text Player2Title;
   [SerializeField] Transform Player1Content;
   [SerializeField] Transform Player2Content;
   [SerializeField] Button AcceptButton;
   [SerializeField] TMP_Text AcceptButtonText;

    RectTransform rt;
    TradeInfo tradeInfo;

    public void Setup(TradeInfo info)
    {
        tradeInfo = info;
        TradeTitle.text = "Trade ID: " + info.TradeId;
        Player1Title.text = "Player ID:" + info.OfferingPlayerId;
        Player2Title.text = "Player ID:" + info.AllowedPlayerIds[0];

        if (GameScenePlayfab.Instance.isPlayerId(info.OfferingPlayerId))
        {
            Player1Title.text = "You";
        }
        if (GameScenePlayfab.Instance.isPlayerId(info.AllowedPlayerIds[0]))
        {
            Player2Title.text = "You";
        }

        if (info.OfferedCatalogItemIds != null)
        {
            foreach (var item in info.OfferedCatalogItemIds)
            {
                GameObject go = Instantiate(ItemTextPrefab, Player1Content);
                go.GetComponent<TMP_Text>().text = GameScenePlayfab.Instance.GetCatalogItem(item).DisplayName;
            }
        }

        if (info.RequestedCatalogItemIds != null)
        {
            foreach (var item in info.RequestedCatalogItemIds)
            {
                GameObject go = Instantiate(ItemTextPrefab, Player2Content);
                go.GetComponent<TMP_Text>().text = GameScenePlayfab.Instance.GetCatalogItem(item).DisplayName;
            }
        }

        AcceptButton.interactable = false;
        AcceptButtonText.text = "Trade Closed";

        if (info.Status == TradeStatus.Open)
        {
            if (!GameScenePlayfab.Instance.isPlayerId(info.OfferingPlayerId))
            {
                AcceptButton.interactable = true;
                AcceptButtonText.text = "Accept";
                AcceptButton.onClick.AddListener(onClickAccept);
            }
            else
            {
                AcceptButton.interactable = true;
                AcceptButtonText.text = "Cancel";
                AcceptButton.onClick.AddListener(onClickCancel);
            }
        }

        rt = GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    void onClickAccept()
    {
        GameScenePlayfab.Instance.AcceptTradeRequest(tradeInfo.TradeId);
    }

    void onClickCancel()
    {
        GameScenePlayfab.Instance.CancelTradeRequest(tradeInfo.TradeId);
    }
}
