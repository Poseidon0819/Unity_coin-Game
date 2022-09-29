using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImportModelManager : MonoBehaviour
{
    public static ImportModelManager instance;
    public Dictionary<string, GameObject> importedModels = new Dictionary<string, GameObject>();
    public Dictionary<string, List<GameObject>> userModelPool = new Dictionary<string, List<GameObject>>();
    public List<GameObject> importedModelIcons;
    public List<GameObject> importedSceneIcons;
    public Transform modelIconContainer;
    public Transform SceneIconContainer;
    public GameObject importedModelIconPrefab;
    public GameObject selectedModel;
    public GameObject userModelPrefab;
    public GameObject refreshBtn;
    public GameObject controlPanel;
    public InputField scaleInput;
    public InputField posYInput;
    public InputField rotXInput;
    public InputField rotYInput;
    public InputField rotZInput;
    public string selectedModelName;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        this.OnRefreshModel();
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            foreach(string keys in importedModels.Keys) {
                Debug.LogError(keys);
            }
        }
        if(this.selectedModel != null) {
            this.controlPanel.SetActive(true);
        } else {
            this.scaleInput.text = "1";
            this.posYInput.text = "0";
            this.rotXInput.text = "0";
            this.rotYInput.text = "0";
            this.rotZInput.text = "0";
            this.controlPanel.SetActive(false);
        }
    }
    public UserModelItem CreateModelIcon()
    {
        GameObject obj = Instantiate(importedModelIconPrefab, modelIconContainer);
        importedModelIcons.Add(obj);
        return obj.GetComponent<UserModelItem>();
    }
    public UserModelItem CreateSceneIcon()
    {
        GameObject obj = Instantiate(importedModelIconPrefab, SceneIconContainer);
        importedSceneIcons.Add(obj);
        return obj.GetComponent<UserModelItem>();
    }
    public void OnRefreshModel()
    {
        this.refreshBtn.SetActive(false);
        for(int i = 0; i < this.importedModelIcons.Count; i++)
        {
            Destroy(this.importedModelIcons[i].gameObject);
        }
        this.importedModelIcons.Clear();
        for(int i = 0; i < this.importedSceneIcons.Count; i++)
        {
            Destroy(this.importedSceneIcons[i].gameObject);
        }
        this.importedSceneIcons.Clear();
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
        // if(userModelPool.ContainsKey(modelName) && userModelPool[modelName].Count > 0) {
        //     GameObject obj = userModelPool[modelName][0];
        //     obj.SetActive(true);
        //     userModelPool[modelName].RemoveAt(0);
        //     return obj;
        // }
        if(!importedModels.ContainsKey(modelName))
            return null;
        return Instantiate(importedModels[modelName]);
    }
    public void AddUserModelPool(string modelName, GameObject obj) {
        // if(!userModelPool.ContainsKey(modelName))
        //     userModelPool[modelName] = new List<GameObject>();
        // MeshRenderer[] mrs = obj.transform.GetComponentsInChildren<MeshRenderer>(true);
        // for (int i = 0; i < mrs.Length; i++)
        // {
        //     mrs[i].transform.localScale = Vector3.one;
        //     mrs[i].transform.localPosition = Vector3.zero;
        //     mrs[i].transform.localEulerAngles = Vector3.zero;
        // }
        // userModelPool[modelName].Add(obj);
        // obj.SetActive(false);
        Destroy(obj);
    }
    public void OnChangeScale()
    {
        if(selectedModel == null)
            return;
        MeshRenderer[] mrs = selectedModel.transform.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < mrs.Length; i++)
        {
            mrs[i].transform.localScale = new Vector3(float.Parse(scaleInput.text), float.Parse(scaleInput.text), float.Parse(scaleInput.text));
        }
    }
    public void OnChangePosY()
    {
        if(selectedModel == null)
            return;
        MeshRenderer[] mrs = selectedModel.transform.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < mrs.Length; i++)
        {
            mrs[i].transform.localPosition = new Vector3(0, float.Parse(posYInput.text), 0);
        }
    }
    public void OnChangeRot()
    {
        if(selectedModel == null)
            return;
        MeshRenderer[] mrs = selectedModel.transform.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < mrs.Length; i++)
        {
            mrs[i].transform.localEulerAngles = new Vector3(float.Parse(rotXInput.text), float.Parse(rotYInput.text), float.Parse(rotZInput.text));
        }        
    }
}
