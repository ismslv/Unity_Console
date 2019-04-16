using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FMLHT {

public class ConsoleEditor : Editor
{
    [MenuItem("FMLHT/Console/Add to scene")]
    public static void AddPrefab() {
        if (Editor.FindObjectOfType<Console>() == null) {
            UnityEngine.Object prefab = Resources.Load("Console");
            var newObj = PrefabUtility.InstantiatePrefab(prefab);
            GameObject obj = (GameObject)newObj;
            obj.name = "Console";
            var core = GameObject.Find("Core");
            if (core == null) {
                core = new GameObject();
                core.name = "Core";
            }
            obj.transform.SetParent(core.transform);
        } else {
            Debug.Log("There is already one Console in this scene!");
        }
    }
}

}