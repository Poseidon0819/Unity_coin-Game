using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradeFriendItem : MonoBehaviour
{
    public Text noText;
    public Text addressText;
    public string walletAddress;
    public Toggle toggle;
    public void SetData(int no, string address, string userName)
    {
        addressText.text = userName;
        walletAddress = address;
        noText.text = no.ToString();
    }
    public void OnSelect()
    {
        if(toggle.isOn) {
            TradeManager.instance.OnSelectFriend(this.walletAddress);
        }
    }
}
