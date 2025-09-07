using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[System.Serializable]
public class MissingScriptsFinder : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts")]
    public static void ShowWindow()
    {
        GetWindow<MissingScriptsFinder>("Missing Scripts Finder");
    }

    void OnGUI()
    {
        GUILayout.Label("Missing Scripts Finder", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Find Missing Scripts in Scene"))
        {
            FindMissingScriptsInScene();
        }
        
        if (GUILayout.Button("Find Missing Scripts in Project"))
        {
            FindMissingScriptsInProject();
        }
        
        if (GUILayout.Button("Refresh and Reimport All"))
        {
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset("Assets", ImportAssetOptions.ImportRecursive);
            Debug.Log("Assets refreshed and reimported!");
        }
    }

    private void FindMissingScriptsInScene()
    {
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int missingCount = 0;

        foreach (GameObject go in allObjects)
        {
            Component[] components = go.GetComponents<Component>();
            
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    missingCount++;
                    Debug.LogError($"Missing script on GameObject: {GetFullPath(go.transform)} at component index {i}", go);
                }
            }
        }

        if (missingCount == 0)
        {
            Debug.Log("No missing scripts found in the current scene!");
        }
        else
        {
            Debug.LogWarning($"Found {missingCount} missing script references in the scene!");
        }
    }

    private void FindMissingScriptsInProject()
    {
        string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab");
        int missingCount = 0;

        foreach (string prefabPath in prefabPaths)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(prefabPath);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            
            if (prefab != null)
            {
                Component[] components = prefab.GetComponentsInChildren<Component>(true);
                
                foreach (Component comp in components)
                {
                    if (comp == null)
                    {
                        missingCount++;
                        Debug.LogError($"Missing script in prefab: {assetPath}", prefab);
                    }
                }
            }
        }

        if (missingCount == 0)
        {
            Debug.Log("No missing scripts found in project prefabs!");
        }
        else
        {
            Debug.LogWarning($"Found {missingCount} missing script references in project prefabs!");
        }
    }

    private string GetFullPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}
#endif
