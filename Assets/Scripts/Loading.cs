using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loading : MonoBehaviour
{
    public float rotateSpeed;
    void Update() {
        this.transform.eulerAngles += new Vector3(0, 0, rotateSpeed * Time.deltaTime);
    }
}
