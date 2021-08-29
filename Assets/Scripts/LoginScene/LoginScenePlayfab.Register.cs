using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;

public partial class LoginScenePlayfab
{
    void onClickRegister()
    {
        SilenceAllButtons();
        var registerRequest = new RegisterPlayFabUserRequest { Email = registerEmailInput.text, Password = registerPasswordInput.text, Username = registerDisplayNameInput.text };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, onRegisterSuccess, onRegisterFail);
    }

    void onRegisterSuccess(RegisterPlayFabUserResult result)
    {
        UnsiilenceAllButtons();
        OpenMessageMenu("Registration Success!", "Welcome to RNG Hero, " + result.Username + ".", () => { SceneManager.LoadScene("GameScene"); });
    }

    void onRegisterFail(PlayFabError error)
    {
        UnsiilenceAllButtons();
        OpenMessageMenu("Error registering!", error.ErrorMessage, OpenRegisterMenu);
    }
}
