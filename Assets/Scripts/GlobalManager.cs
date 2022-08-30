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
    int importIdx = 0;
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
        newObj.transform.parent = ImportModelManager.instance.transform;
        newObj.transform.localPosition = Vector3.zero;
        Buildable build = newObj.GetComponent<Buildable>();
        build.building_character = PlayerCharacter.Get();
        importedModel.transform.parent = newObj.transform;
        importedModel.transform.localPosition = Vector3.zero;
        _model = importedModel;
        ImportModelManager.instance.importedModels[importedModel.name] = newObj;
        Debug.LogError("Success!");
        if(importIdx < ImportModelManager.instance.importedModelIcon.Count) {
            ImportModelManager.instance.importedModelIcon[importIdx].SetActive(true);
            ImportModelManager.instance.importedModelIcon[importIdx].GetComponent<UserModelItem>().SetData(currentLoadingAsset);
            this.importIdx ++;
            currentLoadingAsset = null;
            if(this.assets.Count > importIdx)
                this.ImportModel(this.assets[importIdx]);
            else
                ImportModelManager.instance.refreshBtn.SetActive(true);
        } else {
            ImportModelManager.instance.refreshBtn.SetActive(true);
        }
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
        // if (_model != null)
            // _model.transform.Rotate(0, 1, 0);
    }
    public IEnumerator OnImportAssets()
    {
        var url = getAssetListUrl + GlobalManager.instance.userId;
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
            this.assets.Clear();
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
                }
            }
            Debug.LogError("Asset Cnt : " + assets.Count);
            this.StartImportModel();
        }
    }

    public void StartImportModel()
    {
        importIdx = 0;
        if(assets.Count > 0) {
            this.ImportModel(assets[importIdx]);
        } else {
            ImportModelManager.instance.refreshBtn.SetActive(true);
        }
    }
}
