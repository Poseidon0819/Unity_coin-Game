using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using SurvivalEngine;
using MiniJSON;

public class TradeBuyItem : MonoBehaviour
{
    public Text itemCntText;
    public Text priceText;
    public Image icon;
    public string id;
    public string itemId;
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
        priceText.text = seedCount + " SEED";
    }
    public void OnBuyItem()
    {
        if(GlobalManager.instance.isTest) {
            StartCoroutine("BuyItem");
        } else {
            #if UNITY_WEBGL
            TradeManager.instance.contractManager.Transfer(TradeManager.instance.selectedFriendAddress, priceText.text.Replace("SEED", ""), delegate(){    
                StartCoroutine("BuyItem");
            });
            #endif
        }
    }
    IEnumerator BuyItem()
    {
        string url = GlobalManager.instance.baseUrl + "trading/accept";
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
            PlayerCharacter.Get().Inventory.TakeItem(inventory, this.itemId, UniqueID.GenerateUniqueID(), int.Parse(itemCntText.text.ToString()), itemData.durability);
            TradeManager.instance.buyItems.Remove(this);
            Destroy(this.gameObject);
        }
    }
}
