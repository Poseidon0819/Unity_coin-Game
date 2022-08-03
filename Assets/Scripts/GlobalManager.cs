using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
public class GlobalManager : MonoBehaviour
{
    public static GlobalManager instance;
    public string userId;
    public string mapId;
    public string mapData;
    public string baseUrl = "https://1a28-178-132-6-37.eu.ngrok.io/";
    public Image img;
    void Start()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }
}
