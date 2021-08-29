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
    void onTimeUpdate(int newMin, int newHour)
    {
        int totalHours = newHour * 60;
        int totalTime = (newMin + totalHours);

        float RNGMod = playerRNGMod + playerRNGSwordMod;
        float RNGModFinal = Mathf.Min(playerRNGMod + playerRNGSwordMod, 50);
        float lesserChance =      Mathf.Floor(Mathf.Pow(1.005f, (totalTime)) * 18.0f);
        float normalChance =      Mathf.Floor(Mathf.Pow(1.0065f, (totalTime)) * 3.8f);
        float rareChance =        Mathf.Floor(Mathf.Pow(1.008f, (totalTime)) * 0.2f);
        totalTime = totalTime * 2;
        float totalChance = lesserChance + normalChance + rareChance;

        float totalLesserChance = 0.0f;
        float totalNormalChance = 0.0f;
        float totalRareChance = 0.0f;

        if (totalChance >= 0)
        { 
            totalLesserChance = (lesserChance / totalChance);
            totalNormalChance = (normalChance / totalChance);
            totalRareChance = (rareChance / totalChance);
        }

        long minExpeditionTimeMod = totalTime;
        long maxExpeditionTimeMod = totalTime + totalTime;
        long minRNCurrencyGain = (minExpeditionTimeMod * minExpeditionTimeMod) * (long)((RNGMod / (60.0f - RNGModFinal)) + Mathf.Log(RNGMod * 0.5f));
        long maxRNCurrencyGain = (maxExpeditionTimeMod * maxExpeditionTimeMod) * (long)((RNGMod / (60.0f - RNGModFinal)) + Mathf.Log(RNGMod * 0.5f));

        CurrencyGainText.text = "You will gain between " + minRNCurrencyGain.ToString() + " - " + maxRNCurrencyGain.ToString() + " Currency";
        Tier1LootText.text = "Tier 1 treasure chance: " + totalLesserChance.ToString("P");
        Tier2LootText.text = "Tier 2 treasure chance: " + totalNormalChance.ToString("P");
        Tier3LootText.text = "Tier 3 treasure chance: " + totalRareChance.ToString("P");
    }

    void onClickSendExpedition()
    {
        ExpeditionButton.interactable = false;
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "SendOnExpedition",
            FunctionParameter = new
            {
                Duration = (long)(((HourSlider.value * 60) + MinuteSlider.value) * 60000)
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(request, onSendExpeditionSuccess, onSendExpeditionFail);
    }

    void onSendExpeditionSuccess(ExecuteCloudScriptResult result)
    {
        ReloadUserData(updateExpeditionButton);
    }

    void onSendExpeditionFail(PlayFabError error)
    {
        PlayFabError(error);
    }

    void onClickGetRewards()
    {
        ExpeditionButton.interactable = false;
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "GetExpeditionRewards"
        };

        PlayFabClientAPI.ExecuteCloudScript(request, onGetRewardsSuccess, onGetRewardsFail);
    }

    void onGetRewardsSuccess(ExecuteCloudScriptResult result)
    {
        var jsonObject = JSON.Parse((string)result.FunctionResult);

        string title = "Expedition Success!";
        string message = "Currency gained: " + jsonObject["RNCurrencyGain"];
        if (jsonObject["code"] == "ERROR")
        {
            PlayFabError();
        }
        if (jsonObject["isTreasureGained"].AsBool != false)
        {
            title = "Expedition EPIC success!";
            message += "\nTreasure gained: " + CatalogItemIdToCatalogItem[jsonObject["treasureName"]].DisplayName;
        }
        OpenMessageMenu(title, message, OpenGameMenu);
        ReloadUserData(updateExpeditionButton);
    }

    void onGetRewardsFail(PlayFabError error)
    {
        PlayFabError(error);
    }

    void updateExpeditionButton()
    {
        ExpeditionButton.onClick.RemoveAllListeners();
        // Setup the button text
        if (userData.Data["IsPlayerAdventuring"].Value != "true")
        {
            ExpeditionButtonText.text = "Go on an adventure for...";
            ExpeditionButton.interactable = true;
            ExpeditionButton.onClick.AddListener(onClickSendExpedition);
            return;
        }

        long startTime = long.Parse(userData.Data["PlayerAdventureStartDate"].Value);
        long endTime = long.Parse(userData.Data["PlayerAdventureEndDate"].Value);
        long duration = ((endTime - startTime) / 1000) + 1;
        long timeSinceStart = (DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime) / 1000;

        if (timeSinceStart < duration)
        {
            StartCoroutine(countdownButton(duration - timeSinceStart));
            ExpeditionButton.interactable = false;
        }
        else
        {
            ExpeditionButton.onClick.AddListener(onClickGetRewards);
            ExpeditionButtonText.text = "Collect rewards!";
            ExpeditionButton.interactable = true;
        }
    }

    void setToCollectRewards()
    {
        ExpeditionButton.onClick.RemoveAllListeners();
        ExpeditionButtonText.text = "Collect rewards!";
        ExpeditionButton.interactable = true;
        ExpeditionButton.onClick.AddListener(onClickGetRewards);
    }
}
