using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class InvitedFriendItem : MonoBehaviour
{
    public string id;
    public Text userIdText;
    public string walletAddress;
    public void SetData(Dictionary<string, object> data)
    {
        Dictionary<string, object> fromUser = data["from_user"] as Dictionary<string, object>;
        userIdText.text = fromUser["user_name"].ToString();
        walletAddress = fromUser["wallet_address"].ToString();
        id = data["_id"].ToString();
    }
    public void OnAccept()
    {
        StartCoroutine("AcceptRequest");
    }
    public void OnReject()
    {
        StartCoroutine("RejectRequest");
    }
    
    IEnumerator AcceptRequest()
    {
        string url = GlobalManager.instance.baseUrl + "friend/accept";
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
            // FriendManager.instance.invitedItems.Remove(this);
            // Destroy(this.gameObject);
            FriendManager.instance.OnEnable();            
        }
    }
    
    IEnumerator RejectRequest()
    {
        string url = GlobalManager.instance.baseUrl + "friend/reject";
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
            // FriendManager.instance.invitedItems.Remove(this);
            // Destroy(this.gameObject);
            FriendManager.instance.OnEnable();
        }
    }
}
