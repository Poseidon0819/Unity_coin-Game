using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using MiniJSON;

public class TradeManager : MonoBehaviour
{
    public static TradeManager instance;
    public GameObject friendPrefab;
    public GameObject sellItemPrefab;
    public GameObject buyItemprefab;
    public GameObject addBtn;
    public GameObject sellItemContent;
    public GameObject buyItemContent;
    public Transform friendsScroll;
    public Transform sellItemsScroll;
    public Transform buyItemsScroll;
    public List<TradeBuyItem> buyItems = new List<TradeBuyItem>();
    public List<TradeSellItem> sellItems = new List<TradeSellItem>();
    public List<TradeFriendItem> friendItems = new List<TradeFriendItem>();
    public InputField transferCntText;
    public string selectedFriendAddress;
#if UNITY_WEBGL
    public WebGLTransfer20 contractManager;
#endif
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }
    void OnEnable()
    {
        StartCoroutine("GetFriend");
    }
    void ClearFriendList()
    {
        for(int i = 0; i < friendItems.Count; i++){
            GameObject.Destroy(friendItems[i].gameObject);
        }
        friendItems.Clear();
    }
    void ClearSellItems()
    {
        for(int i = 0; i < sellItems.Count; i++){
            GameObject.Destroy(sellItems[i].gameObject);
        }
        sellItems.Clear();
    }
    void ClearBuyItems()
    {
        for(int i = 0; i < buyItems.Count; i++){
            GameObject.Destroy(buyItems[i].gameObject);
        }
        buyItems.Clear();
    }
    public void OnSendSeed()
    {
#if UNITY_WEBGL
        this.contractManager.Transfer(this.selectedFriendAddress, transferCntText.text);
#endif
    }
    public void OnSelectFriend(string address)
    {
        selectedFriendAddress = address;
        StartCoroutine(GetSellItems(address));
        StartCoroutine(GetBuyItems(address));
    }
    IEnumerator GetBuyItems(string toAddress)
    {
        this.ClearBuyItems();
        string url = GlobalManager.instance.baseUrl + "trading/getItems?from=" + toAddress + "&to=" + GlobalManager.instance.userId;
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
            SetVisibleContents(true);
            for(int i = 0; i < listData.Count; i++)
            {
                Dictionary<string, object> data = listData[i] as Dictionary<string, object>;
                if(data["status"].ToString() != "0"){
                    continue;
                }
                GameObject obj = Instantiate(buyItemprefab, buyItemsScroll);
                TradeBuyItem item = obj.GetComponent<TradeBuyItem>();
                item.id = data["_id"].ToString();
                item.itemId = data["item_id"].ToString();
                item.SetData(data["item_amount"].ToString(), data["item_price"].ToString(), data["item_name"].ToString());
                obj.transform.SetSiblingIndex(this.buyItems.Count);
                this.buyItems.Add(item);
            }
        }
    }
    IEnumerator GetSellItems(string toAddress)
    {
        this.ClearSellItems();
        string url = GlobalManager.instance.baseUrl + "trading/getItems?from=" + GlobalManager.instance.userId + "&to=" + toAddress;
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
                if(data["status"].ToString() != "0"){
                    continue;
                }
                GameObject obj = Instantiate(sellItemPrefab, sellItemsScroll);
                TradeSellItem item = obj.GetComponent<TradeSellItem>();
                item.id = data["_id"].ToString();
                item.itemId = data["item_id"].ToString();
                item.SetData(data["item_amount"].ToString(), data["item_price"].ToString(), data["item_name"].ToString());
                obj.transform.SetSiblingIndex(this.sellItems.Count);
                this.sellItems.Add(item);
            }
        }
    }
    void SetVisibleContents(bool flag)
    {
        this.buyItemContent.SetActive(flag);
        this.sellItemContent.SetActive(flag);
    }
    IEnumerator GetFriend()
    {
        this.SetVisibleContents(false);
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
                TradeFriendItem item = obj.GetComponent<TradeFriendItem>();
                item.SetData(j, friendData["wallet_address"].ToString(), friendData["user_name"].ToString());
                item.GetComponent<Toggle>().group = friendsScroll.GetComponent<ToggleGroup>();
                this.friendItems.Add(item);
            }
        }
    }
    public void OnExit()
    {
        this.gameObject.SetActive(false);
    }

    public void AddSellItem()
    {
        GameObject obj = Instantiate(sellItemPrefab, sellItemsScroll);
        TradeSellItem item = obj.GetComponent<TradeSellItem>();
        item.Init();
        obj.transform.SetSiblingIndex(this.sellItems.Count);
        this.sellItems.Add(item);
        this.addBtn.SetActive(false);
    }
}
