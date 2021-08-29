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

public partial class GameScenePlayfab : Singleton<GameScenePlayfab>
{
    [Header("Info Bar Text")]
    [SerializeField] TMP_Text RNGCurrency;
    [SerializeField] TMP_Text RNGPower;
    [SerializeField] TMP_Text TimeMod;

    [Header("Main game UI")]
    [SerializeField] TMP_Text ExpeditionTitle;
    [SerializeField] TMP_Text TreasureChance;
    [SerializeField] Button ExpeditionButton;
    [SerializeField] TMP_Text ExpeditionButtonText;
    [SerializeField] Slider HourSlider;
    [SerializeField] TMP_Text HourText;
    [SerializeField] Slider MinuteSlider;
    [SerializeField] TMP_Text MinuteText;
    [SerializeField] TMP_Text CurrencyGainText;
    [SerializeField] TMP_Text Tier1LootText;
    [SerializeField] TMP_Text Tier2LootText;
    [SerializeField] TMP_Text Tier3LootText;
    [SerializeField] RectTransform SideMenuContent;
    [Space(10)]

    [Header("Main game Menu Buttons")]
    [SerializeField] Button InventoryButton;
    [SerializeField] Button StoreButton;
    [SerializeField] Button FriendsButton;
    [SerializeField] Button LogoutButton;
    [SerializeField] Button TradesButton;
    [SerializeField] Button BackButton;
    [SerializeField] Button DeleteAccountButton;
    [Space(10)]

    [Header("Inventory Menu")]
    [SerializeField] Transform InventoryContentTransform;
    [SerializeField] GameObject ConsumablePrefab;
    [SerializeField] GameObject EquippablePrefab;
    [Space(10)]

    [Header("Store Menu")]
    [SerializeField] Button StoreBuyButton;
    [SerializeField] Transform StoreContentTransform;
    [SerializeField] GameObject StoreItemPrefab;
    [SerializeField] TMP_Text ShopTitleText;
    [SerializeField] TMP_Text ShopDescriptionText;
    [SerializeField] string SelectedItemID;
    [Space(10)]

    [Header("Message Menu")]
    [SerializeField] TMP_Text MessageTitle;
    [SerializeField] TMP_Text MessageBody;
    [SerializeField] Button MessageButton;
    [Space(10)]

    [Header("Friends Menu")]
    [SerializeField] TMP_InputField FriendEmailInput;
    [SerializeField] Button AddFriendButton;
    [SerializeField] Button UnfriendButton;
    [SerializeField] Button TradeWithFriendButton;
    [SerializeField] Transform FriendsContentTransform;
    [SerializeField] GameObject FriendsCellPrefab;
    [SerializeField] string ActiveFriendID;
    [Space(10)]

    [Header("Friend Trade Menu")]
    [SerializeField] Transform YourInventoryContent;
    [SerializeField] Transform FriendInventoryContent;
    [SerializeField] Button SendTradeButton;
    [SerializeField] GameObject TradeInventoryCellPrefab;
    [SerializeField] GameObject FriendTradeInventoryCellPrefab;
    [SerializeField] TMP_Text FriendTradeTitle;
    [Space(10)]

    [Header("My Trade Menu")]
    [SerializeField] RectTransform MyTradeContent;
    [SerializeField] GameObject TradeCellPrefab;
    [Space(10)]

    [Header("Delete Account Menu")]
    [SerializeField] Button PlsDeleteAccount;
    [SerializeField] Button DunDeleteAccount;

    // Dictionaries
    private Dictionary<string, GameObject> FriendIDToFriendCell = new Dictionary<string, GameObject>();
    private Dictionary<string, JSONNode> CatalogItemIdToCustomData = new Dictionary<string, JSONNode>();
    private Dictionary<string, JSONNode> StoreItemIdToCustomData = new Dictionary<string, JSONNode>();
    private Dictionary<string, CatalogItem> CatalogItemIdToCatalogItem = new Dictionary<string, CatalogItem>();
    private Dictionary<string, ItemInstance> ItemInstanceIDToItem = new Dictionary<string, ItemInstance>();
    private Dictionary<string, GameObject> ItemInstanceIDToInventoryGameObject = new Dictionary<string, GameObject>();

    // My Trade Dictionaries
    private Dictionary<string, TradeInfo> TradeIDToTradeInfo = new Dictionary<string, TradeInfo>();

    // Trade Dictionaries
    private Dictionary<string, JSONNode> FriendInventory = new Dictionary<string, JSONNode>();
    private Dictionary<string, TradeInventoryItem> YourSelectedTradeItems = new Dictionary<string, TradeInventoryItem>();
    private Dictionary<string, TradeFriendInventoryItem> FriendSelectedTradeItems = new Dictionary<string, TradeFriendInventoryItem>();

    // PlayFab Info
    private GetCatalogItemsResult gameCatalogInfo;
    private GetAccountInfoResult userAccountInfo;
    private GetUserInventoryResult userInventory;
    private GetUserDataResult userData;
    private GetStoreItemsResult storeData;
    private GetFriendsListResult friendsList;

    // Menu Manager
    private MenuManager menuManager;

    // Player Stats
    private int playerRNGMod;
    private int playerRNGSwordMod;
    private int playerTimeMod;
    private int playerTimeAmuletMod;
    private int playerTreasureMod;
    private int playerTreasureFinderMod;

    // Start is called before the first frame update
    void Start()
    {
        menuManager = MenuManager.Instance;

        HourSlider.onValueChanged.AddListener((newValue) =>
        {
            HourText.text = ((int)newValue).ToString() + " Hours";
            onTimeUpdate((int)MinuteSlider.value, (int)newValue);
        });

        MinuteSlider.onValueChanged.AddListener((newValue) =>
        {
            MinuteText.text = ((int)newValue).ToString() + " Mins";
            onTimeUpdate((int)newValue, (int)HourSlider.value);
        });

        FriendEmailInput.onValueChanged.AddListener(
            (newValue) =>
            {
                if (newValue.Length == 0)
                {
                    AddFriendButton.interactable = false;
                }
                else if (AddFriendButton.interactable != true)
                {
                    AddFriendButton.interactable = true;
                }
            });

        // Main game column Buttons
        FriendsButton.onClick.AddListener(OpenFriendsMenu);
        InventoryButton.onClick.AddListener(OpenInventoryMenu);
        StoreButton.onClick.AddListener(OpenStoreMenu);
        TradesButton.onClick.AddListener(OpenMyTrades);
        LogoutButton.onClick.AddListener(LogOut);
        DeleteAccountButton.onClick.AddListener(()=> { menuManager.OnlyOpenThisMenu("DeleteAccountMenu"); });

        // Friends list buttons
        AddFriendButton.onClick.AddListener(onClickAddFriend);
        UnfriendButton.onClick.AddListener(onClickRemoveFriend);
        TradeWithFriendButton.onClick.AddListener(OpenFriendTradeMenu);

        // Store buttons
        StoreBuyButton.onClick.AddListener(OnStoreBuyClick);

        // Trade Buttons
        SendTradeButton.onClick.AddListener(SendTrade);

        // Delete account buttons
        PlsDeleteAccount.onClick.AddListener(DeleteAccount);
        DunDeleteAccount.onClick.AddListener(OpenGameMenu);

        HourSlider.value = 24;
        MinuteSlider.value = 60;
        OpenLoadingMenu();
        StartLoadingGame();
    }

    public JSONNode GetCatalogItemCustomData(string ItemID) 
    {
        return CatalogItemIdToCustomData[ItemID];
    }

    public CatalogItem GetCatalogItem(string ItemID)
    {
        return CatalogItemIdToCatalogItem[ItemID];
    }

    public void AddToFriendTradeSelectedItem(string InstanceID, bool remove, TradeFriendInventoryItem item)
    {
        if (remove)
        {
            FriendSelectedTradeItems.Remove(InstanceID);
        }
        else
        {
            FriendSelectedTradeItems.Add(InstanceID, item);
        }
    }

    public void AddToYourTradeSelectedItem(string InstanceID, bool remove, TradeInventoryItem item)
    {
        if (remove)
        {
            YourSelectedTradeItems.Remove(InstanceID);
        }
        else
        {
            YourSelectedTradeItems.Add(InstanceID, item);
        }
    }

    public void OpenMyTrades()
    {
        GetMyTrades();
        menuManager.OnlyOpenThisMenu("MyTradesMenu");
        OpenBackButton(OpenGameMenu);
    }

    public void OpenLoadingMenu()
    {
        menuManager.OnlyOpenThisMenu("LoadingMenu");
    }

    public void OpenMessageMenu(string title, string message, UnityAction action)
    {
        MessageTitle.text = title;
        MessageBody.text = message;
        MessageButton.onClick.RemoveAllListeners();
        MessageButton.onClick.AddListener(action);

        menuManager.OnlyOpenThisMenu("MessageMenu");
    }
    public void OpenStoreMenu()
    {
        menuManager.OnlyOpenThisMenu("StoreMenu");
        OpenBackButton(OpenGameMenu);
    }

    public void OpenGameMenu()
    {
        menuManager.OnlyOpenThisMenu("MainGameMenu");
    }

    public void OpenInventoryMenu()
    {
        menuManager.OnlyOpenThisMenu("InventoryMenu");
        OpenBackButton(OpenGameMenu);
    }

    public void OpenFriendsMenu()
    {
        OpenLoadingMenu();
        onLoadUserFriendsListFinished.AddListener(() => 
        {
            CreateFriendsListItems();
            SetActiveFriendID("");
            menuManager.OnlyOpenThisMenu("FriendsMenu");
            OpenBackButton(OpenGameMenu);
        });
        GetFriendsList();
    }

    public void OpenFriendTradeMenu()
    {
        YourSelectedTradeItems.Clear();
        FriendSelectedTradeItems.Clear();
        OpenLoadingMenu();
        GetFriendInventory();
    }

    public void OpenBackButton(UnityAction action)
    {
        menuManager.OpenMenu("BackButtonMenu");
        BackButton.onClick.RemoveAllListeners();
        BackButton.onClick.AddListener(action);
    }

    public void PlayFabError(PlayFabError result = null)
    {
        if (result == null)
        {
            OpenMessageMenu("Error", "An error occured during cloudscript...", () => { PlayFabClientAPI.ForgetAllCredentials(); SceneManager.LoadScene("LoginScene"); });
            return;
        }
        OpenMessageMenu("Error " + result.Error.ToString(), "Error " + result.ErrorDetails + ", " + result.ErrorMessage, () => { PlayFabClientAPI.ForgetAllCredentials(); SceneManager.LoadScene("LoginScene"); });
    }

    public void LogOut()
    {
        PlayFabClientAPI.ForgetAllCredentials(); 
        SceneManager.LoadScene("LoginScene");
    }

    public void SetActiveFriendID(string s)
    {
        if (s.Length > 0)
        {
            OnSelectedFriendIDChange(ActiveFriendID, s);
            UnfriendButton.interactable = true;
            TradeWithFriendButton.interactable = true;
        }
        else
        {
            UnfriendButton.interactable = false;
            TradeWithFriendButton.interactable = false;
        }
        ActiveFriendID = s;
    }

    public bool isPlayerId(string id)
    {
        return id == userAccountInfo.AccountInfo.PlayFabId;
    }

    IEnumerator countdownButton(long duration)
    {
        while(duration > -1)
        {
            TimeSpan t = TimeSpan.FromSeconds(duration);

            string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                            t.Hours + (t.Days * 24),
                            t.Minutes,
                            t.Seconds);

            ExpeditionButtonText.text = "Time till reward:\n" + answer;
            yield return new WaitForSeconds(1.0f);
            duration -= 1;
        }
        setToCollectRewards();
        yield break;
    }
}
