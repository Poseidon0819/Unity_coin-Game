using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UserModelItem : MonoBehaviour
{
    public string itemId;
    public Image thumbnail;
    public int type;
    public void SetData(ImportedAsset asset)
    {
        this.itemId = asset.contents[asset.model];
        this.type = asset.type;
        string url = GlobalManager.instance.assetBaseUrl + asset.thumbnail;
        GlobalManager.instance.OnSetImage(url, thumbnail);
    }
    public void SetData(SceneProjectData sceneData)
    {
        this.itemId = sceneData.id;
        if(sceneData.thumbnail != null) {
            string url = GlobalManager.instance.builderServerUrl + "v1/projects/" + sceneData.id + "/media/" + sceneData.thumbnail;
            GlobalManager.instance.OnSetImage(url, thumbnail);
            this.type = 2;
        }
    }
    public void OnSelect()
    {
        GameObject newObj = Instantiate(ImportModelManager.instance.userModelPrefab);
        newObj.transform.parent = ImportModelManager.instance.transform;
        newObj.transform.localPosition = Vector3.zero;
        Buildable build = newObj.GetComponent<Buildable>();
        build.building_character = PlayerCharacter.Get();
        UserModelObjectItem item = newObj.GetComponent<UserModelObjectItem>();

        if(type == 1) {
            GameObject obj = ImportModelManager.instance.GetUserModel(itemId);
            obj.transform.parent = newObj.transform;
            obj.transform.localPosition = Vector3.zero;
            item.userModels.Add(obj);
        } else if (type == 2) {
            SceneProjectData sceneData = GlobalManager.instance.sceneAssets[itemId];
            foreach(object entity in sceneData.entities.Values) {
                Dictionary<string, object> dicEntity = entity as Dictionary<string, object>;
                if(dicEntity["name"].ToString().Contains("entity"))
                    continue;
                GameObject obj = null;
                List<object> components = dicEntity["components"] as List<object>;
                Vector3 scale = Vector3.one, position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;
                for(int i = 0; i < components.Count; i++) {
                    Dictionary<string, object> dicComponent = sceneData.components[components[i].ToString()] as Dictionary<string, object>;
                    Dictionary<string, object> componentData = dicComponent["data"] as Dictionary<string, object>;
                    if(dicComponent["type"].ToString() == "Transform") {
                        Dictionary<string, object> dicPosition = componentData["position"] as Dictionary<string, object>;
                        Dictionary<string, object> dicRotation = componentData["rotation"] as Dictionary<string, object>;
                        Dictionary<string, object> dicScale = componentData["scale"] as Dictionary<string, object>;
                        scale = new Vector3(float.Parse(dicScale["x"].ToString()), float.Parse(dicScale["y"].ToString()), float.Parse(dicScale["z"].ToString()));
                        position = new Vector3(float.Parse(dicPosition["x"].ToString()), float.Parse(dicPosition["y"].ToString()), float.Parse(dicPosition["z"].ToString()));
                        rotation = new Quaternion(float.Parse(dicRotation["x"].ToString()), float.Parse(dicRotation["y"].ToString()), float.Parse(dicRotation["z"].ToString()), float.Parse(dicRotation["w"].ToString()));
                    } else if(dicComponent["type"].ToString() == "GLTFShape") {
                        ImportedAsset modelAsset = sceneData.assets[componentData["assetId"].ToString()];
                        obj = ImportModelManager.instance.GetUserModel(modelAsset.GetModelName());
                    }
                }
                if(obj == null)
                    continue;
                obj.transform.parent = newObj.transform;
                obj.transform.localRotation = rotation;
                obj.transform.localScale = scale;
                obj.transform.localPosition = position;
                item.userModels.Add(obj);
            }
        }

        ImportModelManager.instance.CheckSelectedModel();
        ImportModelManager.instance.selectedModel = newObj;
        ImportModelManager.instance.selectedModelName = itemId;
        build.building_mode = true;
        build.position_set = false;
    }
}
