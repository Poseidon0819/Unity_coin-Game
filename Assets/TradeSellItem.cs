using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SurvivalEngine;
using MiniJSON;

public class TradeSellItem : MonoBehaviour
{
    public GameObject info;
    public GameObject input;
    public Image icon;
    public Dropdown itemDropDown;
    public InputField itemCntInput;
    public InputField seedCntInput;
    public Text itemCntText;
    public Text seedCntText;
    public InventoryItemData selectedItem;
    public string itemId;
    public string id;
    public void Init()
    {
        input.SetActive(true);
        info.SetActive(false);
        List<Dropdown.OptionData> itemNames = new List<Dropdown.OptionData>();
        InventoryData inventory = InventoryData.Get(InventoryType.Inventory, 0);
        foreach(InventoryItemData Invitem in inventory.items.Values) {
            ItemData item = Invitem.GetItem();
            itemNames.Add(new Dropdown.OptionData(item.title, item.icon));
        }
        this.itemDropDown.AddOptions(itemNames);
        this.OnSelectItem(0);
    }
    public void SetData(string itemCount, string seedCount, string itemName)
    {
        InventoryData inventory = InventoryData.Get(InventoryType.Inventory, 0);
        foreach(InventoryItemData Invitem in inventory.items.Values) {
            ItemData item = Invitem.GetItem();
            if(itemName == item.title) {
                icon.sprite = item.icon;
            }
        }
        itemCntText.text = itemCount;
        seedCntText.text = seedCount + " SEED";
        this.info.SetActive(true);
        this.input.SetActive(false);
    }
    public void OnConfirm()
    {
        if(itemCntInput.text == "" || seedCntInput.text == "")
            return;
        InventoryData inventory = InventoryData.Get(InventoryType.Inventory, 0);
        ItemData item = selectedItem.GetItem();
        Debug.LogError(selectedItem.quantity + " : " + int.Parse(itemCntInput.text));
        if(selectedItem.quantity >= int.Parse(itemCntInput.text))
        {
            StartCoroutine(OnSellItem());
        }
    }
    IEnumerator OnSellItem()
    {
        InventoryData inventory = InventoryData.Get(InventoryType.Inventory, 0);
        ItemData item = selectedItem.GetItem();
        string url = GlobalManager.instance.baseUrl + "trading/sellitem";
        Debug.LogError(url);
        WWWForm form = new WWWForm();
        form.AddField("from", GlobalManager.instance.userId);
        form.AddField("to", TradeManager.instance.selectedFriendAddress);
        form.AddField("item_id", selectedItem.item_id);
        form.AddField("item_amount", int.Parse(itemCntInput.text));
        form.AddField("item_name", item.title);
        form.AddField("item_price", int.Parse(seedCntInput.text));
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
            if(data != null && data.ContainsKey("_id"))
            {
                PlayerCharacter.Get().Inventory.UseItem(item, int.Parse(itemCntInput.text));
                TradeManager.instance.addBtn.SetActive(true);
                this.id = data["_id"].ToString();
                this.itemId = selectedItem.item_id;
                this.SetData(this.itemCntInput.text, this.seedCntInput.text, this.itemDropDown.captionText.text);
            }
        }
    }
    public void OnCancelInput()
    {
        TradeManager.instance.sellItems.Remove(this);
        TradeManager.instance.addBtn.SetActive(true);
        Destroy(this.gameObject);
    }
    IEnumerator CancelTrade()
    {
        string url = GlobalManager.instance.baseUrl + "trading/reject";
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
            InventoryData inventory = InventoryData.Get(InventoryType.Inventory, 0);
            ItemData itemData = ItemData.Get(itemId);
            PlayerCharacter.Get().Inventory.TakeItem(inventory, this.itemId, UniqueID.GenerateUniqueID(), int.Parse(itemCntInput.text.ToString()), itemData.durability);
            TradeManager.instance.sellItems.Remove(this);
            Destroy(this.gameObject);
        }
    }
    public void OnCancelTrade()
    {
        StartCoroutine(CancelTrade());
    }
    public void OnSelectItem(int index)
    {
        Debug.LogError("OnSelectItem");
        InventoryData inventory = InventoryData.Get(InventoryType.Inventory, 0);
        foreach(InventoryItemData Invitem in inventory.items.Values) {
            ItemData item = Invitem.GetItem();
            if(itemDropDown.captionText.text == item.title) {
                selectedItem = Invitem;
            }
        }
    }
}
