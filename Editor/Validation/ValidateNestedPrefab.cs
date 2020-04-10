using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

using System.IO;

namespace SNAPSPRODUCTION
{

    public class ValidateNestedPrefab : ScriptableObject
    {

        static bool HasNonPrefab(string prefabAssetPath)
        {
            bool result = false;

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabAssetPath);

            Renderer[] renderers = prefabRoot.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                if (PrefabUtility.GetPrefabAssetType(renderer.gameObject) == PrefabAssetType.NotAPrefab)
                {
                    result = true;
                }
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);

            return result;

        }


        static bool IsNestedPrefab(string assetPath)
        {
            bool result = false;

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

            Transform[] trans = prefabRoot.GetComponentsInChildren<Transform>();


            int subPrefabCount = 0;

            foreach (Transform tr in trans)
            {
                if (PrefabUtility.IsAnyPrefabInstanceRoot(tr.gameObject))
                {
                    result = true;
                    subPrefabCount++;
                }
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);

            return result;
        }


        [MenuItem("Tools/Snaps/Validation/Check whether Nested Prefab has only Prefabs, not models.")]
        static void CheckWhetherNestedPrefabHasSubPrefabsOnly()
        {

            int errCount = 0;

            string errorLogPath = Path.Combine(Path.GetTempPath(), "NestedPrefabErr.log");

            if (File.Exists(errorLogPath))
                File.Delete(errorLogPath);

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets" });

            ArrayList nestedPrefabList = new ArrayList();

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                if (!assetPath.ToLower().Contains(".prefab"))
                    continue;

                if (IsNestedPrefab(assetPath))
                {
                    if (!nestedPrefabList.Contains(assetPath))
                    {
                        nestedPrefabList.Add(assetPath);
                    }
                }

            }

            StreamWriter sw = new StreamWriter(errorLogPath);

            foreach (string path in nestedPrefabList)
            {
                if (HasNonPrefab(path))
                {
                    Debug.LogWarning("Nested prefab has models : " + path);
                    sw.WriteLine("Nested prefab has models : " + path);
                    errCount++;
                }
            }

            sw.Close();




            if (errCount != 0)
            {
                if (File.Exists(errorLogPath))
                    Application.OpenURL(errorLogPath);

                SceneView.lastActiveSceneView.ShowNotification(new GUIContent(errCount.ToString() + " Nested Prefabs need to be checked."));
            }
            else
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("No problems at the nested prefabs"));


        }

    }
}

#endif