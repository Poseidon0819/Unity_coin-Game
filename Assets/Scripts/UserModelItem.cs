using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalEngine;

public class UserModelItem : MonoBehaviour
{
    public string modelName;
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
