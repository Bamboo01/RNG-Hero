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

public class InventoryEquippable : RNGHeroInventoryItem
{
    [SerializeField] TMP_Text buttonText;
    [SerializeField] Button button;

    void OnEnable()
    {
        button.onClick.AddListener(OnClick);
    }

    void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }

    public override void Setup(ItemInstance item)
    {
        base.Setup(item);
        buttonText.text = item.DisplayName;
    }

    public override void OnClick()
    {
        GameScenePlayfab.Instance.EquipItem(itemInstance);
    }
}
