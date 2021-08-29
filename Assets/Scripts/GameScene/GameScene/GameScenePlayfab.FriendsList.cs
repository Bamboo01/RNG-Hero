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
    void CreateFriendsListItems()
    {
        FriendIDToFriendCell.Clear();

        foreach (Transform a in FriendsContentTransform)
        {
            Destroy(a.gameObject);
        }

        foreach (var friend in friendsList.Friends)
        {
            GameObject cell = Instantiate(FriendsCellPrefab, FriendsContentTransform);
            FriendIDToFriendCell.Add(friend.FriendPlayFabId, cell);
            cell.GetComponent<FriendsCell>().Setup(friend);
        }
        ActiveFriendID = "";
    }

    void onClickAddFriend()
    {
        OpenLoadingMenu();
        var request = new AddFriendRequest();
        request.FriendEmail = FriendEmailInput.text;
        PlayFabClientAPI.AddFriend(request, OnAddFriendSuccess, onAddFail);
    }

    void onAddFail(PlayFabError error)
    {
        OpenMessageMenu("Failed to add friend!", error.ErrorMessage, OpenFriendsMenu);
    }

    void OnAddFriendSuccess(AddFriendResult result)
    {
        OpenMessageMenu("Successfully added friend!", "" , OpenFriendsMenu);
    }

    void onClickRemoveFriend()
    {
        OpenLoadingMenu();
        var request = new RemoveFriendRequest { FriendPlayFabId = ActiveFriendID };
        PlayFabClientAPI.RemoveFriend(request, OnRemoveFriendSuccess, onRemoveFail);
    }

    void OnRemoveFriendSuccess(RemoveFriendResult result)
    {
        OpenMessageMenu("Successfully removed friend!", "", OpenFriendsMenu);
    }

    void onRemoveFail(PlayFabError error)
    {
        OpenMessageMenu("Failed to remove friend!", error.ErrorMessage, OpenFriendsMenu);
    }

    void OnSelectedFriendIDChange(string oldFriendID, string newFriendID)
    {
        if (oldFriendID.Length > 0)
        {
            FriendIDToFriendCell[oldFriendID].GetComponent<FriendsCell>().HighlightButton(false);
        }
        if (newFriendID.Length > 0)
        {
            FriendIDToFriendCell[newFriendID].GetComponent<FriendsCell>().HighlightButton(true);
        }
    }

    FriendInfo GetFriend(string friendID)
    {
        return friendsList.Friends.Find((friend) =>{ return friend.FriendPlayFabId == friendID; });
    }
}