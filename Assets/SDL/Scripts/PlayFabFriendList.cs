using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabFriendList : MonoBehaviour
{
    List<FriendInfo> _friends = null;
    [SerializeField] InputField login_email;
    [SerializeField] InputField login_password;

    [SerializeField] InputField register_email;
    [SerializeField] InputField register_username;
    [SerializeField] InputField register_password;

    [SerializeField] InputField friendEmail;

    [SerializeField] Button loginButton;
    [SerializeField] Button registerButton;
    [SerializeField] Button openregisterButton;
    [SerializeField] Button addfriendButton;
    [SerializeField] Button getfriendButton;

    [SerializeField] GameObject login;
    [SerializeField] GameObject registration;
    [SerializeField] GameObject loggedIn;

    enum FriendIdType { PlayFabId, Username, Email, DisplayName };

    void Start()
    {
        login.SetActive(true);
        registration.SetActive(false);
        loggedIn.SetActive(false);

        //Note: Setting title Id here can be skipped if you have set the value in Editor Extensions already.
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            PlayFabSettings.TitleId = "2E51D"; // Please change this value to your own titleId from PlayFab Game Manager
        }

        loginButton.onClick.AddListener(onClickLogin);

        registerButton.onClick.AddListener(onClickRegister);
        openregisterButton.onClick.AddListener(OnClickOpenRegister);
        addfriendButton.onClick.AddListener(OnAddFriendClicked);
        getfriendButton.onClick.AddListener(OnGetFriendsListClicked);
    }

    void AddFriend(FriendIdType idType, string friendId)
    {
        var request = new AddFriendRequest();
        switch (idType)
        {
            case FriendIdType.PlayFabId:
                request.FriendPlayFabId = friendId;
                break;
            case FriendIdType.Username:
                request.FriendUsername = friendId;
                break;
            case FriendIdType.Email:
                request.FriendEmail = friendId;
                break;
            case FriendIdType.DisplayName:
                request.FriendTitleDisplayName = friendId;
                break;
        }
        // Execute request and update friends when we are done
        PlayFabClientAPI.AddFriend(request, result => {
            Debug.Log("Friend added successfully!");
        }, DisplayPlayFabError);
    }

    public void onClickRegister()
    {
        // register the player
        var registerRequest = new RegisterPlayFabUserRequest { Email = register_email.text, Username = register_username.text, Password = register_password.text };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterFail);
    }

    public void onClickLogin()
    {
        // login using this username
        var loginRequest = new LoginWithEmailAddressRequest { Email = login_email.text, Password = login_password.text };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        // Debug.Log("Congratulations, you made your first successful API call!");
        Debug.Log("Login is successful");
        login.SetActive(false);
        registration.SetActive(false);
        loggedIn.SetActive(true);
    }
    private void OnLoginFailure(PlayFabError error)
    {
        // Debug.LogWarning("Something went wrong with your first API call. :(");
        // Debug.LogError("Here's some debug information:");
        Debug.LogError("Login failed with error: \n" + error.GenerateErrorReport());
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Registration is successful");
        login.SetActive(true);
        registration.SetActive(false);
        loggedIn.SetActive(false);
    }

    private void OnRegisterFail(PlayFabError error)
    {
        Debug.LogError("Registration failed with error: \n" + error.GenerateErrorReport());
    }

    public void GetFriends()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            IncludeSteamFriends = false,
            IncludeFacebookFriends = false,
            XboxToken = null
        }, result => {
            _friends = result.Friends;
            DisplayFriends(_friends); // triggers your UI
        }, DisplayPlayFabError);
    }
    public void DisplayFriends(List<FriendInfo> friendsCache)
    {
        Debug.Log(string.Format("There are {0} friend(s)", friendsCache.Count));
        friendsCache.ForEach(f => Debug.Log(f.Username));
    }

    public void DisplayPlayFabError(PlayFabError error) { Debug.Log(error.GenerateErrorReport()); }

    public void OnGetFriendsListClicked()
    {
        GetFriends();
    }

    public void OnAddFriendClicked()
    {
        // second parameter should be another player account in your title
        AddFriend(FriendIdType.Email, friendEmail.text);
    }

    public void OnClickOpenRegister()
    {
        login.SetActive(false);
        registration.SetActive(true);
        loggedIn.SetActive(false);
    }
}
