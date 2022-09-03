using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ERC721OwnerOfExample : MonoBehaviour
{
    async void Start()
    {
        string chain = "ethereum";
        string network = "mainnet";
        string contract = "0xc09d1aa618ae8a4b54c5ac60efb394d38bf79d03";
        string tokenId = "1";

        string ownerOf = await ERC721.OwnerOf(chain, network, contract, tokenId);
        Debug.LogError(ownerOf);
    }
}
