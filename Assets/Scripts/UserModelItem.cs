using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UserModelItem : MonoBehaviour
{
    public string modelName;
    public Image thumbnail;
    public void SetData(ImportedAsset asset)
    {
        this.modelName = asset.contents[asset.model];
        string url = GlobalManager.instance.assetBaseUrl + asset.thumbnail;
        Debug.LogError(url);
        StartCoroutine(setImage(url));
    }

    IEnumerator setImage(string url) {
        yield return new WaitForSeconds(3f);
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        www.SetRequestHeader("Access-Control-Allow-Origin", "*");
        yield return www.SendWebRequest();
        if(www.downloadHandler.data != null && thumbnail != null) {
            Debug.LogError("image + " + www.downloadHandler.data.Length);
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            if(texture != null) {
                thumbnail.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                // thumbnail.transform.localScale = new Vector3(2256f / texture.width, 1421f / texture.height, 1);
            }
        }
    }
    public void OnSelect()
    {
        GameObject obj = ImportModelManager.instance.GetUserModel(modelName);
        ImportModelManager.instance.CheckSelectedModel();
        ImportModelManager.instance.selectedModel = obj;
        ImportModelManager.instance.selectedModelName = modelName;
        Buildable build = obj.GetComponent<Buildable>();
        build.building_mode = true;
        build.position_set = false;
        Debug.LogError("OnClick");
    }
}
