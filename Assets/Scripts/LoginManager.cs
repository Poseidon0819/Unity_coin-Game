using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using MiniJSON;
using SurvivalEngine;
using System.Runtime.InteropServices;
using Photon.Pun;


public class LoginManager : MonoBehaviour
{

    [DllImport("__Internal")]
    private static extern void Web3Connect();

    [DllImport("__Internal")]
    private static extern string ConnectAccount();

    // Start is called before the first frame update
    public InputField userIdInput;
    public InputField mapIdInput;
    public GameObject loginBtn;
    public GameObject loading;

    private string account;

    private void Start()
    {
        this.ConnectToPhoton();
    }
    public void OnLogin()
    {
        // this.account = "0xfa455c51CF7605E6E4695D6426A81765DB9c44fE";
        if(this.account == null || this.account == "") {
            return;
        }
        StartCoroutine("LoadData");
    }
    IEnumerator LoadData()
    {
        this.loginBtn.SetActive(false);
        this.loading.SetActive(true);
        while(true) {
            var url = "https://meta.birdezkingdom.com/reveal/" + mapIdInput.text;
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.LogError(www.downloadHandler.text);
                if(www.downloadHandler.text.Length < 3) {
                    loginBtn.SetActive(true);
                    loading.SetActive(false);
                    break;
                } else {
                    while(true) {
                        url = GlobalManager.instance.baseUrl + "world?user_id=" + this.account + "&map_id=" + this.mapIdInput.text;
                        GlobalManager.instance.userId = this.account;
                        GlobalManager.instance.mapId = this.mapIdInput.text;
                        GlobalManager.instance.mapData = www.downloadHandler.text;

                        www = UnityWebRequest.Get(url);
                        yield return www.SendWebRequest();

                        if (www.result != UnityWebRequest.Result.Success)
                        {
                            Debug.LogError(www.error);
                            // Dictionary<string, object> data = new Dictionary<string, object>();
                            // PlayerData.player_data = new PlayerData();
                            // PlayerData.player_data.FixData();
                            // SceneNav.GoTo("Game");
                            // break;
                        }
                        else
                        {
                            Debug.LogError(www.downloadHandler.text.Length);
                            Dictionary<string, object> data = Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
                            PlayerData.player_data = new PlayerData();
                            if(data.ContainsKey("user_id")) {
                                PlayerData.player_data.LoadSaveData(data);
                            } else {
                                PlayerData.player_data.FixData();
                            }
                            SceneNav.GoTo("Game");
                            break;
                        }
                    }
                    break;
                }
            }
        }
    }

    public void OnWalletConnect()
    {
        Web3Connect();
        OnConnected();
    }

    async private void OnConnected()
    {
        account = ConnectAccount();
        while (account == "")
        {
            await new WaitForSeconds(1f);
            account = ConnectAccount();
            Debug.Log("Metamask Address: " + account);
        };
        PlayerPrefs.SetString("PlayerAddress", account);
        Debug.Log("Wallet Address:" + account);
    }
    void ConnectToPhoton()
    {
        Debug.LogError("ConnectToPhoton");
        PhotonNetwork.GameVersion = "10"; //1
        PhotonNetwork.OfflineMode = true;
        PhotonNetwork.ConnectUsingSettings(); //2
    }
}
