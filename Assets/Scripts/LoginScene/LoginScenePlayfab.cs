using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Bamboo.UI;
using Bamboo.Events;
using Bamboo.Utility;

public partial class LoginScenePlayfab : MonoBehaviour
{
    [Header("Login Menu")]
    [SerializeField] TMP_InputField loginEmailInput;
    [SerializeField] TMP_InputField loginPasswordInput;
    [SerializeField] Button loginButton;
    [SerializeField] Button gotoRegisterButton;
    [Space(10)]

    [Header("Register Menu")]
    [SerializeField] TMP_InputField registerDisplayNameInput;
    [SerializeField] TMP_InputField registerEmailInput;
    [SerializeField] TMP_InputField registerPasswordInput;
    [SerializeField] Button registerButton;
    [SerializeField] Button gotoLoginButton;
    [Space(10)]

    [Header("Message Menu")]
    [SerializeField] TMP_Text messageTitle;
    [SerializeField] TMP_Text messageBody;
    [SerializeField] Button messageButton;
    [Space(10)]

    private MenuManager menuManager;
    
    void Start()
    {
        gotoRegisterButton.onClick.AddListener(OpenRegisterMenu);
        gotoLoginButton.onClick.AddListener(OpenLoginMenu);
        loginButton.onClick.AddListener(onClickLogin);
        registerButton.onClick.AddListener(onClickRegister);

        menuManager = MenuManager.Instance;
        OpenLoginMenu();
    }

    void SilenceAllButtons()
    {
        var buttonList = Resources.FindObjectsOfTypeAll<Button>();
        foreach (var button in buttonList)
        {
            button.interactable = false;
        }
    }

    void UnsiilenceAllButtons()
    {
        var buttonList = Resources.FindObjectsOfTypeAll<Button>();
        foreach (var button in buttonList)
        {
            button.interactable = true;
        }
    }

    void OpenLoginMenu()
    {
        menuManager.OnlyOpenThisMenu("LoginMenu");
        menuManager.OpenMenu("EssentialsMenu");
    }

    void OpenRegisterMenu()
    {
        menuManager.OnlyOpenThisMenu("RegisterMenu");
        menuManager.OpenMenu("EssentialsMenu");
    }

    void OpenMessageMenu(string title, string message, UnityAction action)
    {
        messageTitle.text = title;
        messageBody.text = message;
        messageButton.onClick.RemoveAllListeners();
        messageButton.onClick.AddListener(action);

        menuManager.OnlyOpenThisMenu("MessageMenu");
    }
}
