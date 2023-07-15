using UnityEngine;
using UnityEditor;
using System.Linq;
using Unity.Transforms;
using Unity.VisualScripting;
using Unity.NetCode;

public class DelayedSpawnCooker : EditorWindow
{
    [MenuItem("Helpers/Delayed Spawn Cooker")]
    public static void CookDelayedSpawners()
    {
        string prefabsPath = "Assets/Delayed Spawning/Prefabs";
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { prefabsPath });

        string productsPath = "Assets/Delayed Spawning/Products";

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

            PrefabUtility.SaveAsPrefabAsset(product, $"{productsPath}/{product.name}.prefab");
            DestroyImmediate(product);
        }
    }
}