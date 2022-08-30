using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImportModelManager : MonoBehaviour
{
    public static ImportModelManager instance;
    public GameObject userModelPrefab;
    public Dictionary<string, GameObject> importedModels = new Dictionary<string, GameObject>();
    public GameObject selectedModel;
    public string selectedModelName;
    public Dictionary<string, List<GameObject>> userModelPool = new Dictionary<string, List<GameObject>>();
    public List<GameObject> importedModelIcon;
    public GameObject refreshBtn;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        this.OnRefreshModel();
    }
    public void OnRefreshModel()
    {
        this.refreshBtn.SetActive(false);
        for(int i = 0; i < this.importedModelIcon.Count; i++)
        {
            this.importedModelIcon[i].SetActive(false);
        }
        StartCoroutine(GlobalManager.instance.OnImportAssets());
    }
    public void CheckSelectedModel()
    {
        if(this.selectedModel != null) {
            this.AddUserModelPool(this.selectedModelName, this.selectedModel);
            this.selectedModel = null;
        }
    }
    public GameObject GetUserModel(string modelName) {
        if(userModelPool.ContainsKey(modelName) && userModelPool[modelName].Count > 0) {
            GameObject obj = userModelPool[modelName][0];
            obj.SetActive(true);
            userModelPool[modelName].RemoveAt(0);
            return obj;
        }
        return Instantiate(ImportModelManager.instance.importedModels[modelName]);
    }
    public void AddUserModelPool(string modelName, GameObject obj) {
        if(!userModelPool.ContainsKey(modelName))
            userModelPool[modelName] = new List<GameObject>();
        userModelPool[modelName].Add(obj);
        obj.SetActive(false);
    }
}
