using System.Collections;
using System.Timers;
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
    private UnityEvent onGetCatalogFinished = new UnityEvent();
    private UnityEvent onGetStoreFinished = new UnityEvent();
    private UnityEvent onGetAccountInfoFinished = new UnityEvent();
    private UnityEvent onGetUserDataFinished = new UnityEvent();
    private UnityEvent onLoadInventoryFinished = new UnityEvent();
    private UnityEvent onLoadUserFriendsListFinished = new UnityEvent();

    void StartLoadingGame()
    {
        onGetCatalogFinished.AddListener(GetStore);
        onGetStoreFinished.AddListener(GetAccountInfo);
        onGetAccountInfoFinished.AddListener(GetUserData);
        onGetUserDataFinished.AddListener(GetUserInventory);
        onLoadInventoryFinished.AddListener(
        () =>{
            Debug.Log("Done loading all playfab data.");
            updateExpeditionButton();
            OpenGameMenu();
            SetupMainGame();
        });
        GetCatalogInfo();
    }

    public void ReloadUserData(UnityAction action = null)
    {
        onGetAccountInfoFinished.AddListener(GetUserData);
        onGetUserDataFinished.AddListener(GetUserInventory);
        onLoadInventoryFinished.AddListener(SetupMainGame);
        if (action != null)
        {
            onLoadInventoryFinished.AddListener(action.Invoke);
        }
        GetAccountInfo();
    }
    void SetupMainGame()
    {
        Debug.Log("Setting up main game column");
        if (userData.Data == null || userInventory.Inventory == null)
        {
            OpenMessageMenu("Error loading player inventory and user data!", "", () => { SceneManager.LoadScene("LoginScene"); });
        }

        var Sword = userInventory.Inventory.Find(item => item.ItemInstanceId == userData.Data["EquippedSword"].Value);
        var Amulet = userInventory.Inventory.Find(item => item.ItemInstanceId == userData.Data["EquippedAmulet"].Value);
        var TreasureFinder = userInventory.Inventory.Find(item => item.ItemInstanceId == userData.Data["EquippedTreasureFinder"].Value);

        if (Sword == null)
        {
            ForceEquipItem(ItemInstanceIDToItem["PracticeRNGSword"], ()=> { ReloadUserData(); });
            return;
        }
        if (Amulet == null)
        {
            ForceEquipItem(ItemInstanceIDToItem["PracticeTimeAmulet"], () => { ReloadUserData(); });
            return;
        }
        if (TreasureFinder == null)
        {
            ForceEquipItem(ItemInstanceIDToItem["PracticeTreasureFinder"], () => { ReloadUserData(); });
            return;
        }

        // Setup the stats UI
        ExpeditionTitle.text = userAccountInfo.AccountInfo.Username + "'s Expedition Crib";
        RNGCurrency.text = "RNCurrency: " + userInventory.VirtualCurrency["RN"].ToString();
            
        playerRNGMod = int.Parse(userData.Data["PlayerBaseRNGMod"].Value);
        playerRNGSwordMod = int.Parse(CatalogItemIdToCustomData[Sword.ItemId]["RNGMod"]);
        playerTimeMod = int.Parse(userData.Data["PlayerBaseTimeMod"].Value);
        playerTimeAmuletMod = int.Parse(CatalogItemIdToCustomData[Amulet.ItemId]["TimeMod"]);
        playerTreasureMod = int.Parse(userData.Data["PlayerBaseTreasureMod"].Value);
        playerTreasureFinderMod = int.Parse(CatalogItemIdToCustomData[TreasureFinder.ItemId]["TreasureMod"]);

        RNGPower.text = "RNG Power: " + (playerRNGSwordMod + playerRNGMod).ToString();
        TimeMod.text = "Time Modifier: -" + (playerTimeMod + playerTimeAmuletMod).ToString() + "%";
        TreasureChance.text = "Treasure Chance: " + (playerTreasureMod + playerTreasureFinderMod).ToString() + "%";
        CreateInventoryItems();
        CreateStoreItems();
        onTimeUpdate((int)MinuteSlider.value, (int)HourSlider.value);
    }

    #region PlayFab Callbacks
    void GetCatalogInfo()
    {
        Debug.Log("Getting catalog info");
        var request = new GetCatalogItemsRequest();
        PlayFabClientAPI.GetCatalogItems(request, onGetCatalogInfoSuccess, PlayFabError);
    }

    void onGetCatalogInfoSuccess(GetCatalogItemsResult result)
    {
        Debug.Log("Loaded catalog info");
        gameCatalogInfo = result;
        foreach (var a in gameCatalogInfo.Catalog)
        {
            CatalogItemIdToCatalogItem.Add(a.ItemId, a);
            if (a.CustomData == null)
            {
                CatalogItemIdToCustomData.Add(a.ItemId, null);
                continue;
            }
            string data = a.CustomData;
            if (data.Length != 0)
            {
                CatalogItemIdToCustomData.Add(a.ItemId, JSON.Parse(data));
            }
            else
            {
                CatalogItemIdToCustomData.Add(a.ItemId, null);
            }
        }

        onGetCatalogFinished?.Invoke();
        onGetCatalogFinished?.RemoveAllListeners();
    }

    void GetStore()
    {
        Debug.Log("Getting store");
        string catalogVer = "1.00";
        string storeId = "RNGShop";

        var request = new GetStoreItemsRequest
        {
            CatalogVersion = catalogVer,
            StoreId = storeId
        };

        PlayFabClientAPI.GetStoreItems(request, onGetStoreSuccess, PlayFabError);
    }

    void onGetStoreSuccess(GetStoreItemsResult result)
    {
        Debug.Log("Loaded catalog");
        storeData = result;

        foreach (var a in storeData.Store)
        {
            if (a.CustomData == null)
            {
                StoreItemIdToCustomData.Add(a.ItemId, null);
                continue;
            }
            string data = a.CustomData.ToString();
            if (data.Length != 0)
            {
                StoreItemIdToCustomData.Add(a.ItemId, JSON.Parse(data));
            }
            else
            {
                CatalogItemIdToCustomData.Add(a.ItemId, null);
            }
        }

        onGetStoreFinished?.Invoke();
        onGetStoreFinished?.RemoveAllListeners();
    }

    void GetAccountInfo()
    {
        Debug.Log("Getting account info");
        var request = new GetAccountInfoRequest();
        PlayFabClientAPI.GetAccountInfo(request, onGetAccountInfoSuccess, PlayFabError);
    }

    void onGetAccountInfoSuccess(GetAccountInfoResult result)
    {
        Debug.Log("Loaded account info");
        userAccountInfo = result;
        onGetAccountInfoFinished?.Invoke();
        onGetAccountInfoFinished?.RemoveAllListeners();
    }

    void GetUserData()
    {
        Debug.Log("Getting user data");
        var getUserDataReq = new GetUserDataRequest();
        PlayFabClientAPI.GetUserReadOnlyData(getUserDataReq, onLoadGetUserDataSuccess, PlayFabError);
    }

    void onLoadGetUserDataSuccess(GetUserDataResult result)
    {
        Debug.Log("Loaded user data");
        userData = result;
        onGetUserDataFinished?.Invoke();
        onGetUserDataFinished?.RemoveAllListeners();
    }

    void GetUserInventory()
    {
        Debug.Log("Getting user inventory");
        var getUserInventoryReq = new GetUserInventoryRequest();
        PlayFabClientAPI.GetUserInventory(getUserInventoryReq, onLoadGetUserInventorySuccess, PlayFabError);
    }

    void onLoadGetUserInventorySuccess(GetUserInventoryResult result)
    {
        Debug.Log("Loaded user inventory");
        userInventory = result;
        onLoadInventoryFinished?.Invoke();
        onLoadInventoryFinished?.RemoveAllListeners();
    }

    void GetFriendsList()
    {
        Debug.Log("Getting users friends");
        var getFriendsListReq = new GetFriendsListRequest{
            IncludeSteamFriends = false,
            IncludeFacebookFriends = false,
            XboxToken = null};

        PlayFabClientAPI.GetFriendsList(getFriendsListReq, onGetFriendsListSuccess, PlayFabError);
    }

    void onGetFriendsListSuccess(GetFriendsListResult result)
    {
        Debug.Log("Loaded users friends list");
        friendsList = result;
        onLoadUserFriendsListFinished?.Invoke();
        onLoadUserFriendsListFinished?.RemoveAllListeners();
    }
    #endregion
}