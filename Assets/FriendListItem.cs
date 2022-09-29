using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendListItem : MonoBehaviour
{
    public Text noText;
    public Text addressText;
    public string walletAddress;
    public void SetData(int no, string address, string userName)
    {
        addressText.text = userName;
        walletAddress = address;
        noText.text = no.ToString();
    }
}
