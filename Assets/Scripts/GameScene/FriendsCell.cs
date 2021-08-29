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


public class FriendsCell : MonoBehaviour
{
    [SerializeField] TMP_Text buttonText;
    [SerializeField] Button button;
    private FriendInfo friendInfo;

    void OnEnable()
    {
        button.onClick.AddListener(OnClick);
    }

    void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }

    public void Setup(FriendInfo friend)
    {
        friendInfo = friend;
        buttonText.text = friend.Username;
    }

    public void OnClick()
    {
        GameScenePlayfab.Instance.SetActiveFriendID(friendInfo.FriendPlayFabId);
    }

    public void HighlightButton(bool b)
    {
        button.image.color = b ? Color.green : Color.white;
    }
}
