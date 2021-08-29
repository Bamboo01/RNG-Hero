using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bamboo.UI;

public class FriendsMenuAddon : MonoBehaviour
{
    void OnEnable()
    {
        //var menuitem = GetComponent<Menu>();
       // menuitem.OnMenuClose.AddListener(()=> { GameScenePlayfab.Instance.SetActiveFriendID(""); });
    }

    void OnDisable()
    {
        //var menuitem = GetComponent<Menu>();
        //menuitem.OnMenuClose.RemoveAllListeners();
    }
}
