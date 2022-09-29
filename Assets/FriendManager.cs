using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using MiniJSON;

public class FriendManager : MonoBehaviour
{
    public static FriendManager instance;
    public Transform requestScroll;
    public Transform invitedScroll;
    public Transform friendsScroll;
    public GameObject requestItemPrefab;
    public GameObject invitedItemPrefab;
    public GameObject friendPrefab;
    public List<RequestFriendItem> requestItems = new List<RequestFriendItem> ();
    public List<InvitedFriendItem> invitedItems = new List<InvitedFriendItem> ();
    public List<FriendListItem> friendItems = new List<FriendListItem>();
    public List<string> friendList = new List<string>();
    void Start()
    {
        instance = this;
    }
    public void OnEnable()
    {
        StartCoroutine("GetFriend");
        StartCoroutine("GetFriendReuests");
        StartCoroutine("GetFriendInvited");
    }
    IEnumerator GetFriend()
    {
        string url = GlobalManager.instance.baseUrl + "user/getInfo?id=" + GlobalManager.instance.userId;
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
            Dictionary<string, object> dicData = Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
            List<object> data = dicData["friends"] as List<object>;
            ClearFriendList();
            int j = 0;
            for(int i = 0; i < data.Count; i++)
            {
                Dictionary<string, object> friendData = data[i] as Dictionary<string, object>;
                if(friendData["wallet_address"].ToString() == GlobalManager.instance.userId)
                    continue;
                j++;
                GameObject obj = Instantiate(friendPrefab, friendsScroll);
                FriendListItem item = obj.GetComponent<FriendListItem>();
                item.SetData(j, friendData["wallet_address"].ToString(), friendData["user_name"].ToString());
                this.friendList.Add(friendData["wallet_address"].ToString());
                this.friendItems.Add(item);
            }
        }
    }
    void ClearFriendList()
    {
        for(int i = 0; i < friendItems.Count; i++){
            GameObject.Destroy(friendItems[i].gameObject);
        }
        friendItems.Clear();
        friendList.Clear();
    }
    void ClearRequestItems()
    {
        for(int i = 0; i < this.requestItems.Count; i++)
        {
            Destroy(this.requestItems[i].gameObject);
        }
        this.requestItems.Clear();
    }
    void ClearInvitedItems()
    {
        for(int i = 0; i < this.invitedItems.Count; i++)
        {
            Destroy(this.invitedItems[i].gameObject);
        }
        this.invitedItems.Clear();
    }
    
    public IEnumerator GetFriendInvited()
    {
        this.ClearInvitedItems();
        string url = GlobalManager.instance.baseUrl + "friend/getRequestorList?user_id=" + GlobalManager.instance.userId;
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
            List<object> listData = Json.Deserialize(www.downloadHandler.text) as List<object>;
            for(int i = 0; i < listData.Count; i++)
            {
                Dictionary<string, object> data = listData[i] as Dictionary<string, object>;
                GameObject obj = Instantiate(invitedItemPrefab, invitedScroll);
                InvitedFriendItem item = obj.GetComponent<InvitedFriendItem>();
                item.SetData(data);
                invitedItems.Add(item);
            }
        }
    }
    public IEnumerator GetFriendReuests()
    {
        this.ClearRequestItems();
        string url = GlobalManager.instance.baseUrl + "friend/getInviteeList?user_id=" + GlobalManager.instance.userId;
        List<string> requestUsers = new List<string>();
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
            List<object> data = Json.Deserialize(www.downloadHandler.text) as List<object>;
            int j = 0;
            for(int i = 0; i < data.Count; i++)
            {
                Dictionary<string, object> dicData = data[i] as Dictionary<string, object>;
                Dictionary<string, object> toUserData = dicData["to_user"] as Dictionary<string, object>;
                if(toUserData["wallet_address"] == null || toUserData["wallet_address"].ToString() == GlobalManager.instance.userId)
                    continue;
                j++;
                GameObject obj = Instantiate(requestItemPrefab, requestScroll);
                RequestFriendItem item = obj.GetComponent<RequestFriendItem>();
                item.SetData(toUserData["user_name"].ToString(), toUserData["wallet_address"].ToString(), false);
                requestUsers.Add(toUserData["wallet_address"].ToString());
                item.id = dicData["_id"].ToString();
                this.requestItems.Add(item);
            }
        }
        
        url = GlobalManager.instance.baseUrl + "user/getAll";
        Debug.LogError(url);
        www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.LogError(www.downloadHandler.text);
            List<object> data = Json.Deserialize(www.downloadHandler.text) as List<object>;
            for(int i = 0; i < data.Count; i++)
            {
                Dictionary<string, object> dicData = data[i] as Dictionary<string, object>;
                if(dicData["wallet_address"].ToString() == GlobalManager.instance.userId
                 || requestUsers.Contains(dicData["wallet_address"].ToString())
                 || this.friendList.Contains(dicData["wallet_address"].ToString()))
                    continue;
                GameObject obj = Instantiate(requestItemPrefab, requestScroll);
                RequestFriendItem item = obj.GetComponent<RequestFriendItem>();
                item.SetData(dicData["user_name"].ToString(), dicData["wallet_address"].ToString(), true);
                this.requestItems.Add(item);
            }
        }
    }
}
