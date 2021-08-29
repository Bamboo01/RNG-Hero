using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;

public partial class LoginScenePlayfab
{
    void onClickLogin()
    {
        SilenceAllButtons();
        var loginRequest = new LoginWithEmailAddressRequest { Email = loginEmailInput.text, Password = loginPasswordInput.text };
        loginRequest.InfoRequestParameters = new GetPlayerCombinedInfoRequestParams { GetUserAccountInfo = true };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, onLoginSuccess, onLoginFail);
    }

    void onLoginSuccess(LoginResult result)
    {
        UnsiilenceAllButtons();
        OpenMessageMenu("Login Success!", "Welcome back to RNG Hero, " + result.InfoResultPayload.AccountInfo.Username + ".", () => { SceneManager.LoadScene("GameScene"); });
    }

    void onLoginFail(PlayFabError error)
    {
        UnsiilenceAllButtons();
        OpenMessageMenu("Error logging in!", error.ErrorMessage,  OpenLoginMenu);
    }
}
