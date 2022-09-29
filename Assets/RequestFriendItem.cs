using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using MiniJSON;

public class RequestFriendItem : MonoBehaviour
{
    public Text address;
    string walletAddress;
    public string id;
    public GameObject requestBtn;
    public GameObject cancelBtn;
    public void SetData(string userName, string userAddress, bool is_request)
    {
        this.address.text = userName;
        walletAddress = userAddress;
        this.SetStae(is_request);
    }
    void SetStae(bool is_request)
    {
        requestBtn.SetActive(false);
        cancelBtn.SetActive(false);
        if(is_request)
            requestBtn.SetActive(true);
        else
            cancelBtn.SetActive(true);        
    }
    public void OnRequest()
    {
        StartCoroutine("RequestFriend");
    }
    public void OnCancel()
    {
        StartCoroutine("CancelRequest");
    }
    IEnumerator RequestFriend()
    {
        string url = GlobalManager.instance.baseUrl + "friend/request";
        List<string> requestUsers = new List<string>();
        Debug.LogError(url);
        WWWForm form = new WWWForm();
        form.AddField("from", GlobalManager.instance.userId);
        form.AddField("to", this.walletAddress);
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.LogError(www.downloadHandler.text);
            Dictionary<string, object> data = Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
            // data = data["data"] as Dictionary<string, object>;
            id = data["_id"].ToString();
            SetStae(false);
        }
    }
    IEnumerator CancelRequest()
    {
        string url = GlobalManager.instance.baseUrl + "friend/reject";
        List<string> requestUsers = new List<string>();
        Debug.LogError(url);
        WWWForm form = new WWWForm();
        form.AddField("id", this.id);
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.LogError(www.downloadHandler.text);
            Dictionary<string, object> data = Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
            // if(data.ContainsKey("data") && data["data"].ToString() == "Sucessfully canceled")
            SetStae(true);
        }
    }
}
