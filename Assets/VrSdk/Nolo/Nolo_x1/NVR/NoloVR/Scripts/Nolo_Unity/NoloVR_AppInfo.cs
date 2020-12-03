using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoloVR_AppInfo : MonoBehaviour {
    public string appKey;
    void Start() {
        NoloVR_Playform.GetInstance().Authentication(appKey);
    }
}
