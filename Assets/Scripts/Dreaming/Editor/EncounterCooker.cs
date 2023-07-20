using UnityEngine;
using UnityEditor;
using System.Linq;
using Unity.Transforms;
using Unity.VisualScripting;
using Unity.NetCode;
using System.Collections.Generic;

public class EncounterCooker : EditorWindow
{
    [MenuItem("Helpers/Encounter cooker")]
    public static void CookDelayedSpawners()
    {
        string prefabsPath = "Assets/Encounters/Prefabs";
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { prefabsPath });

        string productsPath = "Assets/Encounters/Products";

        var encounters = FindObjectOfType<EncountersAuthoring>();
        encounters.encounters = new List<EncounterAuthoring>();



        foreach (string prefabGUID in prefabGUIDs)
        {
            // Check if prefabGUID is in productGUIDS
            // If not, build the product prefab from the prefab

            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUID);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            var product = new GameObject(prefab.name);
            var delayedSpawner = product.AddComponent<DelayedSpawnerAuthoring>();
            delayedSpawner.spawnables = new DelayedSpawnBuffer[prefab.transform.childCount];

            var i = 0;
            foreach (Transform child in prefab.transform)
            {
                delayedSpawner.spawnables[i] = new DelayedSpawnBuffer
                {
                    prefab = child.gameObject.GetPrefabDefinition().GameObject(),
                    position = child.position,
                    rotation = child.rotation,
                };

                i++;
            }

            var savedProduct = PrefabUtility.SaveAsPrefabAsset(product, $"{productsPath}/{product.name}.prefab");
            DestroyImmediate(product);

            if (prefab.GetComponent<EncounterComponent>() == null) Debug.Log(prefab.name);
            var encounter = prefab.GetComponent<EncounterComponent>().encounter;
            encounters.encounters.Add(new EncounterAuthoring
            {
                tags = encounter.tags,
                weight = encounter.weight,
                prefab = savedProduct,
            });
        }
    }

    [MenuItem("Helpers/Thingamajig")]
    public static void Thingamajig()
    {
        Debug.Log("Thingamajig");

        // Get all prefabs
        // Check each for a marker component
        // If it has it, iterate over all chidren of the prefab.
        // If the child is a prefab, put it in the set
        // If a child is not a prefab, save it as one and put it 

        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");

        foreach (string prefabGUID in prefabGUIDs)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUID);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (!prefab.TryGetComponent<ComplexColliderMarker>(out var m))
            {
                continue;
            }

            foreach (Transform child in prefab.transform)
            {

                if (child.gameObject.GetPrefabDefinition() != null)
                {
                    Debug.Log($"{child.name} from parent {prefab.name}");
                }
            }
        }
    }
}
