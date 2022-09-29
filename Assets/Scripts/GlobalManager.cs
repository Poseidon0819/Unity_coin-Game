using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using MiniJSON;
using Piglet;
using SurvivalEngine;
using System.IO;
using Siccity.GLTFUtility;

public enum PlayMode
{
    None,
    Guest,
    Visitor,
    Owner
}
public class ImportedAsset
{
    public string id;
    public string asset_pack_id;
    public string model;
    public string thumbnail;
    public int type;
    public Dictionary<string, string> contents = new Dictionary<string, string>();
    public void SetData(Dictionary<string, object> data)
    {
        this.id = data["id"].ToString();
        this.model = data["model"].ToString();
        this.thumbnail = data["thumbnail"].ToString().Replace(GlobalManager.instance.assetBaseUrl, "");
        if(data.ContainsKey("asset_pack_id"))
            this.asset_pack_id = data["asset_pack_id"].ToString();
        if(data.ContainsKey("assetPackId"))
            this.asset_pack_id = data["assetPackId"].ToString();
        Dictionary<string, object> content = data["contents"] as Dictionary<string, object>;
        foreach(string name in content.Keys){
            this.contents.Add(name, content[name].ToString());
        }
    }
    public string GetModelName()
    {
        return this.contents[model];
    }
}
public class SceneProjectData
{
    public string id;
    public string thumbnail;
    public Dictionary<string, object> entities = new Dictionary<string, object>();
    public Dictionary<string, object> components = new Dictionary<string, object>();
    public Dictionary<string, ImportedAsset> assets = new Dictionary<string, ImportedAsset>();
}
public class GlobalManager : MonoBehaviour
{
    public static GlobalManager instance;
    public bool isTest;
    public string userId;
    public string mapId;
    public string mapData;
    public string baseUrl = "https://1a28-178-132-6-37.eu.ngrok.io/";
    public string assetBaseUrl;
    public string getAssetListUrl;
    public string builderServerUrl;
    public Image img;
    public List<ImportedAsset> assets = new List<ImportedAsset>();
    public List<Transform> saveingObjects = new List<Transform>();
    public Dictionary<string, SceneProjectData> sceneAssets = new Dictionary<string, SceneProjectData>();
    public PlayMode playMode;
    int importIdx = 0;
    private GltfImportTask _task;
    ImportedAsset currentLoadingAsset;
    void Start()
    {
        this.playMode = PlayMode.None;
        instance = this;
        DontDestroyOnLoad(this);
    }
    public string GetS3Name(string uriStr)
    {
        string name = uriStr.Replace(this.assetBaseUrl, "");
        string url = this.assetBaseUrl + this.currentLoadingAsset.contents[name];
        // Debug.LogError(url);
        return url;
    }
    public void ImportModel(ImportedAsset assetData)
    {
        this.currentLoadingAsset = assetData;
        string url = assetBaseUrl + assetData.contents[assetData.model];
        // Debug.LogError(url);
        _task = RuntimeGltfImporter.GetImportTask(url);
        _task.OnProgress = OnProgress;
        _task.OnCompleted = OnComplete;
    }
    void CompleteImportModels()
    {
        //create scene icons
        foreach(string id in sceneAssets.Keys) {
            ImportModelManager.instance.CreateSceneIcon().SetData(sceneAssets[id]);
        }
    }
    void FinalizeImportObject(GameObject importedModel, ImportedAsset asset)
    {
        // GameObject newObj = Instantiate(ImportModelManager.instance.userModelPrefab);
        // newObj.transform.parent = ImportModelManager.instance.transform;
        // newObj.transform.localPosition = Vector3.zero;
        // Buildable build = newObj.GetComponent<Buildable>();
        // build.building_character = PlayerCharacter.Get();
        // importedModel.transform.localPosition = Vector3.zero;
        
        importedModel.transform.parent = ImportModelManager.instance.transform;
        importedModel.transform.localPosition = Vector3.zero;
        ImportModelManager.instance.importedModels[asset.GetModelName()] = importedModel;
        MeshRenderer[] mrs = importedModel.transform.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < mrs.Length; i++)
        {
            mrs[i].gameObject.AddComponent<MeshCollider>();
        }
        if(asset.type == 1)
            ImportModelManager.instance.CreateModelIcon().SetData(asset);
    }
    private void OnComplete(GameObject importedModel)
    {
        this.FinalizeImportObject(importedModel, currentLoadingAsset);
        this.importIdx ++;
        currentLoadingAsset = null;
        while(this.assets.Count > importIdx && ImportModelManager.instance.importedModels.ContainsKey(this.assets[importIdx].contents[this.assets[importIdx].model])) {
            importIdx ++;
        }
        if(this.assets.Count > importIdx)
            this.ImportModel(this.assets[importIdx]);
        else {
            ImportModelManager.instance.refreshBtn.SetActive(true);
            CompleteImportModels();
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
        int i, j;
        var url = getAssetListUrl + GlobalManager.instance.userId;
        // Debug.LogError(url);
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Dictionary<string, object> data = Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
            List<object> listData = data["data"] as List<object>;
            this.assets.Clear();
            for(i = 0; i < listData.Count; i++) {
                Dictionary<string, object> dicData = listData[i] as Dictionary<string, object>;
                List<object> assetData = dicData["assets"] as List<object>;
                for(j = 0; j < assetData.Count; j++){
                    Dictionary<string, object> tmp = assetData[j] as Dictionary<string, object>;
                    if(tmp["model"].ToString() == "undefined")
                        continue;
                    ImportedAsset asset = new ImportedAsset();
                    asset.SetData(tmp);
                    asset.type = 1;
                    if(ImportModelManager.instance.importedModels.ContainsKey(asset.contents[asset.model]))
                        continue;
                    if(asset.model.Contains(".glb")){
                        yield return new WaitForSeconds(2f);
                        string filePath = $"{Application.persistentDataPath}/Files/" + asset.model;
                        // Debug.LogError(filePath);
                        url = GlobalManager.instance.assetBaseUrl + asset.contents[asset.model];
                        // Debug.LogError(url);
                        if(File.Exists(filePath)) {
                            this.ImportModelFromFile(filePath, asset);
                        } else {
                            www = UnityWebRequest.Get(url);
                            www.downloadHandler = new DownloadHandlerFile(filePath);
                            yield return www.SendWebRequest();

                            if (www.result != UnityWebRequest.Result.Success)
                            {
                                Debug.LogError(www.error);
                            }
                            else
                            {
                                this.ImportModelFromFile(filePath, asset);
                            }
                        }
                    } else {
                        this.assets.Add(asset);
                    }
                }
            }

            // Debug.LogError("Asset Cnt : " + assets.Count);
        }

        //scene load
        sceneAssets.Clear();
        url = builderServerUrl + "v1/projects/unity?ethAddress=" + GlobalManager.instance.userId;
        // Debug.LogError(url);
        www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            // Debug.LogError(www.downloadHandler.text);
            Dictionary<string, object> data = Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
            List<object> listData = (data["data"] as Dictionary<string, object>)["items"] as List<object>;
            for(i = 0; i < listData.Count; i++)
            {
                Dictionary<string, object> dicData = listData[i] as Dictionary<string, object>;
                SceneProjectData asset = new SceneProjectData();
                asset.id = dicData["id"].ToString();
                if(dicData["thumbnail"] != null)
                    asset.thumbnail = dicData["thumbnail"].ToString();
                sceneAssets[asset.id] = asset;
            }
        }
        foreach(string sceneId in sceneAssets.Keys)
        {
            url = builderServerUrl + "v1/projects/" + sceneId + "/manifest";
            // Debug.LogError(url);
            www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.LogError(www.error);
            }
            else {
                // Debug.LogError(www.downloadHandler.text);
                Dictionary<string, object> data = Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
                Dictionary<string, object> sceneData = data["scene"] as Dictionary<string, object>;
                sceneAssets[sceneId].entities = sceneData["entities"] as Dictionary<string, object>;
                sceneAssets[sceneId].components = sceneData["components"] as Dictionary<string, object>;
                Dictionary<string, object> assetData = sceneData["assets"] as Dictionary<string, object>;
                foreach(string id in assetData.Keys)
                {
                    Dictionary<string, object> tmp = assetData[id] as Dictionary<string, object>;
                    if(tmp["category"].ToString() == "ground")
                        continue;
                    ImportedAsset asset = new ImportedAsset();
                    asset.SetData(tmp);
                    asset.type = 2;
                    sceneAssets[sceneId].assets[id] = asset;
                    if(ImportModelManager.instance.importedModels.ContainsKey(asset.contents[asset.model]))
                        continue;
                    if(asset.model.Contains(".glb")) {
                        yield return new WaitForSeconds(2f);
                        string filePath = $"{Application.persistentDataPath}/Files/" + asset.model;
                        // Debug.LogError(filePath);
                        url = GlobalManager.instance.assetBaseUrl + asset.contents[asset.model];
                        // Debug.LogError(url);
                        if(File.Exists(filePath)) {
                            this.ImportModelFromFile(filePath, asset);
                        } else {
                            www = UnityWebRequest.Get(url);
                            www.downloadHandler = new DownloadHandlerFile(filePath);
                            yield return www.SendWebRequest();

                            if (www.result != UnityWebRequest.Result.Success) {
                                Debug.LogError(www.error);
                            }
                            else {
                                this.ImportModelFromFile(filePath, asset);
                            }
                        }
                    } else {
                        this.assets.Add(asset);
                    }
                }
            }
        }
        this.StartImportModel();
    }
    void ImportModelFromFile(string filePath, ImportedAsset asset)
    {
        GameObject model = Importer.LoadFromFile(filePath);
        FinalizeImportObject(model, asset);
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

    public void OnSetImage(string url, Image image) {
        StartCoroutine(SetImage(url, image));
    }
    public IEnumerator SetImage(string url, Image image) {
        yield return new WaitForSeconds(3f);
        Debug.LogError(url);
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        www.SetRequestHeader("Access-Control-Allow-Origin", "*");
        yield return www.SendWebRequest();
        if(www.downloadHandler.data != null && image != null) {
            // Debug.LogError("image + " + www.downloadHandler.data.Length);
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            if(texture != null) {
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                // thumbnail.transform.localScale = new Vector3(2256f / texture.width, 1421f / texture.height, 1);
            }
        }
    }
}
