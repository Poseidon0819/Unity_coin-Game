using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using MiniJSON;
using Piglet;
using SurvivalEngine;
public class ImportedAsset
{
    public string id;
    public string asset_pack_id;
    public string model;
    public string thumbnail;
    public Dictionary<string, string> contents = new Dictionary<string, string>();
}
public class GlobalManager : MonoBehaviour
{
    public static GlobalManager instance;
    public string userId;
    public string mapId;
    public string mapData;
    public string baseUrl = "https://1a28-178-132-6-37.eu.ngrok.io/";
    public string assetBaseUrl;
    public string getAssetListUrl;
    public Image img;
    public List<ImportedAsset> assets = new List<ImportedAsset>();
    private GameObject _model;
    private GltfImportTask _task;
    ImportedAsset currentLoadingAsset;
    void Start()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }
    public string GetS3Name(string uriStr)
    {
        string name = uriStr.Replace(this.assetBaseUrl, "");
        string url = this.assetBaseUrl + this.currentLoadingAsset.contents[name];
        Debug.LogError(url);
        return url;
    }
    public void ImportModel(ImportedAsset assetData)
    {
        this.currentLoadingAsset = assetData;
        string url = assetBaseUrl + assetData.contents[assetData.model];
        Debug.LogError(url);
        _task = RuntimeGltfImporter.GetImportTask(url);
        _task.OnProgress = OnProgress;
        _task.OnCompleted = OnComplete;
    }
    private void OnComplete(GameObject importedModel)
    {
        GameObject newObj = Instantiate(ImportModelManager.instance.userModelPrefab);
        Buildable build = newObj.GetComponent<Buildable>();
        build.building_character = PlayerCharacter.Get();
        newObj.SetActive(false);
        importedModel.transform.parent = newObj.transform;
        importedModel.transform.localPosition = Vector3.zero;
        _model = importedModel;
        Debug.Log("Success!");
    }
    private void OnProgress(GltfImportStep step, int completed, int total)
    {
        Debug.LogFormat("{0}: {1}/{2}", step, completed, total);
    }
    void Update()
    {
        // advance execution of glTF import task
        if(_task != null) {
            _task.MoveNext();
        }

        // spin model about y-axis
        if (_model != null)
            _model.transform.Rotate(0, 1, 0);
    }
    public IEnumerator OnImportAssets()
    {
        var url = getAssetListUrl + "0xB34Ea9c0516Ccf82869d226D631caF62D6aE55E6";
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
            Dictionary<string, object> data = Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
            List<object> listData = data["data"] as List<object>;
            int i, j;
            for(i = 0; i < listData.Count; i++) {
                Dictionary<string, object> dicData = listData[i] as Dictionary<string, object>;
                List<object> assetData = dicData["assets"] as List<object>;
                for(j = 0; j < assetData.Count; j++){
                    Dictionary<string, object> tmp = assetData[j] as Dictionary<string, object>;
                    ImportedAsset asset = new ImportedAsset();
                    asset.id = tmp["id"].ToString();
                    asset.model = tmp["model"].ToString();
                    if(asset.model == "undefined")
                        continue;
                    asset.thumbnail = tmp["thumbnail"].ToString();
                    asset.asset_pack_id = tmp["asset_pack_id"].ToString();
                    Dictionary<string, object> content = tmp["contents"] as Dictionary<string, object>;
                    foreach(string name in content.Keys){
                        asset.contents.Add(name, content[name].ToString());
                    }
                    this.assets.Add(asset);
                    ImportModel(asset);
                }
            }
        }
    }
}
