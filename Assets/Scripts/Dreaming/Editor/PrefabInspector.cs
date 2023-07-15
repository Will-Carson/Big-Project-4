using UnityEngine;
using UnityEditor;
using System.Linq;
using Unity.Transforms;
using Unity.VisualScripting;
using Unity.NetCode;

public class PrefabInspector : EditorWindow
{
    [MenuItem("Window/Prefab Inspector")]
    public static void ShowWindow()
    {
        GetWindow<PrefabInspector>("Prefab Inspector");
    }

    private void OnGUI()
    {
        //GUILayout.Label("Prefabs in Assets/Encounters Folder", EditorStyles.boldLabel);

        //string prefabsPath = "Assets/Encounters/Prefabs";
        //string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { prefabsPath });

        //string productsPath = "Assets/Encounters/Products";
        //string[] productGUIDS = AssetDatabase.FindAssets("t:Prefab", new[] { productsPath });

        //foreach (string prefabGUID in prefabGUIDs)
        //{
        //    // display "new" prefabs (prefabs without products)
        //}

        if (GUILayout.Button("Cook encounters"))
        {
            CookEncounters();
        }
    }

    private static void CookEncounters()
    {
        // Get all the prefabs in the Assets/Encounters folder
        string prefabsPath = "Assets/Encounters/Prefabs";
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { prefabsPath });

        string productsPath = "Assets/Encounters/Products";
        string[] productGUIDS = AssetDatabase.FindAssets("t:Prefab", new[] { productsPath });



        foreach (string prefabGUID in prefabGUIDs)
        {
            // Check if prefabGUID is in productGUIDS
            // If not, build the product prefab from the prefab

            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUID);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            var product = new GameObject(prefab.name);
            //product.AddComponent<GhostAuthoringComponent>();
            var encounterAuthoring = product.AddComponent<EncounterAuthoring>();
            encounterAuthoring.encounter = new EncounterBuffer[prefab.transform.childCount];

            var i = 0;
            foreach (Transform child in prefab.transform)
            {
                encounterAuthoring.encounter[i] = new EncounterBuffer
                {
                    prefab = child.gameObject.GetPrefabDefinition().GameObject(),
                    position = child.position,
                    rotation = child.rotation,
                };

                i++;
            }


            PrefabUtility.SaveAsPrefabAsset(product, productsPath + "/" + product.name + ".prefab");
            DestroyImmediate(product);
        }
    }
}