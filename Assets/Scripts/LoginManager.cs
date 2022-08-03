using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using MiniJSON;
using SurvivalEngine;

public class LoginManager : MonoBehaviour
{
    // Start is called before the first frame update
    public InputField userIdInput;
    public InputField mapIdInput;
    public GameObject loginBtn;
    public GameObject loading;
    public void OnLogin()
    {
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
                        url = GlobalManager.instance.baseUrl + "world?user_id=" + this.userIdInput.text + "&map_id=" + this.mapIdInput.text;
                        GlobalManager.instance.userId = this.userIdInput.text;
                        GlobalManager.instance.mapId = this.mapIdInput.text;
                        GlobalManager.instance.mapData = www.downloadHandler.text;

                        www = UnityWebRequest.Get(url);
                        yield return www.SendWebRequest();

                        if (www.result != UnityWebRequest.Result.Success)
                        {
                            Debug.LogError(www.error);
                            Dictionary<string, object> data = new Dictionary<string, object>();
                            PlayerData.player_data = new PlayerData();
                            PlayerData.player_data.FixData();
                            SceneNav.GoTo("Game");
                            break;
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
}
