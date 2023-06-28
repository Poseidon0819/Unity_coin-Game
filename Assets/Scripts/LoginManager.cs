using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using MiniJSON;
using SurvivalEngine;
using System.Runtime.InteropServices;
using Photon.Pun;
using Photon.Realtime;

public class LoginManager : MonoBehaviourPunCallbacks
{

    [DllImport("__Internal")]
    private static extern void Web3Connect();

    [DllImport("__Internal")]
    private static extern string ConnectAccount();

    // Start is called before the first frame update
    public InputField userIdInput;
    public Text mapIdInput;
    public GameObject loginBtn;
    public GameObject loginVisitorBtn;
    public GameObject loading;
    public GameObject LoginUI;
    public GameObject RegUI;
    public Text loadingDesc;

#if UNITY_WEBGL
    public WebGLTransfer20 transfer;
#endif
    private string account;
    void Awake()
    {
        Debug.Log("Awake!!!");
        //4 
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    private void Start()
    {
        #if UNITY_WEBGL || WEBSOCKET || ((UNITY_XBOXONE || UNITY_GAMECORE) && UNITY_EDITOR)
        Debug.LogError("gdgagdsa");
        #endif

        this.ConnectToPhoton();
        this.loadingDesc.text = "";
        this.LoginUI.SetActive(false);
        this.RegUI.SetActive(true);
        // StartCoroutine(GetSelectedMapId());
    }
    IEnumerator GetSelectedMapId()
    {
        var url = GlobalManager.instance.baseUrl + "map/getselected?user_id=" + this.account;
        while(true)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.LogError(www.downloadHandler.text);
                Dictionary<string, object> data = Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
                if(data != null && data.ContainsKey("map_id"))
                {
                    this.mapIdInput.text = data["map_id"].ToString();
                    StartCoroutine(GetUserInfo());
                    break;
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    IEnumerator GetUserInfo()
    {
        string url = GlobalManager.instance.baseUrl + "user/getInfo?id=" + this.account;
        Debug.LogError(url);
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.LogError(www.downloadHandler.text);
            if(www.downloadHandler.text != "") {
                Dictionary<string, object> dicData = Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
                if(dicData["user_name"] != null)
                    userIdInput.text = dicData["user_name"].ToString();
            }
            this.RegUI.SetActive(false);
            this.LoginUI.SetActive(true);
        }
    }
    public void OnLoginNormal()
    {
        CheckLandOwner();
        this.OnLogin();
    }
    void OnLogin()
    {
        if(this.userIdInput.text == "")
            return;
        if(this.account == null || this.account == "") {
            return;
        }
        else if (PhotonNetwork.IsConnected)
        {
            StartCoroutine("LoadData");
        }
        else
        {
            this.ConnectToPhoton();
        }

    }
    public void OnLoginasVisitor()
    {
        GlobalManager.instance.playMode = PlayMode.Visitor;
        this.OnLogin();
    }

    async void CheckLandOwner()
    {
        string chain = "ethereum";
        string network = "mainnet";
        string contract = "0xc09d1aa618ae8a4b54c5ac60efb394d38bf79d03";
        string tokenId = this.mapIdInput.text;

        string ownerOf = await ERC721.OwnerOf(chain, network, contract, tokenId);
        ownerOf = ownerOf.ToLower();
        if(this.account == ownerOf) {
            GlobalManager.instance.playMode = PlayMode.Owner;
        } else {
            GlobalManager.instance.playMode = PlayMode.Guest;
        }
        // Debug.LogError(ownerOf);
    }
    void SetLoading(bool isLoading)
    {
        this.loginBtn.SetActive(!isLoading);
        // this.loginVisitorBtn.SetActive(!isLoading);
        this.loading.SetActive(isLoading);
    }
    IEnumerator LoadData()
    {
        this.SetLoading(true);
        while(GlobalManager.instance.playMode == PlayMode.None)
        {
            yield return new WaitForSeconds(0.5f);
        }
        if(GlobalManager.instance.playMode == PlayMode.Guest) {
            loadingDesc.text = "Loading as Guest";
        } else if(GlobalManager.instance.playMode == PlayMode.Owner) {
            loadingDesc.text = "Loading as Owner";
        } else if(GlobalManager.instance.playMode == PlayMode.Visitor) {
            loadingDesc.text = "Loading as Visitor";
        }
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
                    this.SetLoading(false);
                    break;
                } else {
                    while(true) {
                        url = GlobalManager.instance.baseUrl + "world/getWorlds?user_id=" + this.account + "&user_name=" + this.userIdInput.text + "&map_id=" + this.mapIdInput.text;
                        Debug.LogError(url);
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
                            Debug.LogError(www.downloadHandler.text);
                            List<object> listData = Json.Deserialize(www.downloadHandler.text) as List<object>;
                            PlayerData.player_data = new PlayerData();
                            if(listData != null && listData.Count > 0) {
                                Dictionary<string, object> data = listData[0] as Dictionary<string, object>;
                                PlayerData.player_data.LoadSaveData(data);
                            } else {
                                PlayerData.player_data.FixData();
                            }
                            // SceneNav.GoTo("Game");
                            PhotonNetwork.LocalPlayer.NickName = this.userIdInput.text; //1
                            Debug.Log("PhotonNetwork.IsConnected! | Trying to Create/Join Room " + "Land" + this.mapIdInput.text);
                            RoomOptions roomOptions = new RoomOptions(); //2
                            TypedLobby typedLobby = new TypedLobby("Land" + this.mapIdInput.text, LobbyType.Default); //3
                            PhotonNetwork.JoinOrCreateRoom("Land" + this.mapIdInput.text , roomOptions, typedLobby); //4
                            this.SetLoading(false);
                            break;
                        }
                    }
                    break;
                }
            }
        }
    }

    public void LoadArena()
    {
        base.OnJoinedRoom();
        // 5
        if (PhotonNetwork.CurrentRoom.PlayerCount > 0)
        {
            PhotonNetwork.LoadLevel("Game");
        }
        else
        {
            // playerStatus.text = "Minimum 2 Players required to Load Arena!";
        }
    }

    // Photon Methods
    public override void OnConnected()
    {
        // 1
        base.OnConnected();
        Debug.LogError("connected to Photon!");
        // 2
        // connectionStatus.text = "Connected to Photon!";
        // connectionStatus.color = Color.green;
        // roomJoinUI.SetActive(true);
        // buttonLoadArena.SetActive(false);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        // 3
        // isConnecting = false;
        // controlPanel.SetActive(true);
        Debug.LogError("Disconnected. Please check your Internet connection.");
    }

    public override void OnJoinedRoom()
    {
        // Debug.LogError("heelo " + PhotonNetwork.CurrentRoom.Name);
        // 4
        if (PhotonNetwork.IsMasterClient)
        {
            this.LoadArena();
            // buttonLoadArena.SetActive(true);
            // buttonJoinRoom.SetActive(false);
            // playerStatus.text = "Your are Lobby Leader";
        }
    }

    public void OnWalletConnect()
    {
        if(GlobalManager.instance.isTest) {
            this.account = "0x0241d725f8bd03188f9f4c8ff92efbb6791c860f";
            // this.account = "0xfa455c51CF7605E6E4695D6426A81765DB9c44fE";
            this.mapIdInput.text = "2498";
            StartCoroutine(GetUserInfo());
        } else {
            Web3Connect();
            OnConnectedWallet();
        }
    }

    async private void OnConnectedWallet()
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
        StartCoroutine(GetSelectedMapId());
    }
    void ConnectToPhoton()
    {
        Debug.LogError("ConnectToPhoton");
        PhotonNetwork.OfflineMode = true;
        PhotonNetwork.ConnectUsingSettings(); //2
    }
}
