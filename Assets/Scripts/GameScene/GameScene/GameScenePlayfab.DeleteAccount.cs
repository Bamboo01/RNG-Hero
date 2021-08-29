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
    void DeleteAccount()
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "DeleteAccount",
            FunctionParameter = new
            {
            }
        };

        PlayFabClientAPI.ExecuteCloudScript(request, (result) =>
        {
            OpenMessageMenu("Account deleted!", "", LogOut);

        }, PlayFabError);
    }
}
