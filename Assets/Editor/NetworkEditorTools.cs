using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class NetworkEditorTools
{
    [MenuItem("Tools/Set Scene IDs")]
    public static void SetSceneIDs() {
        int currentId = -1;
        NetworkedObject[] objects = GameObject.FindObjectsOfType<NetworkedObject>();
        foreach (NetworkedObject netObj in objects) {
            netObj.sceneId = currentId;
            currentId--;
            EditorUtility.SetDirty(netObj);
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
}
