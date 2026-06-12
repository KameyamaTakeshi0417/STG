using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Alpha.Core.Utils;
using Alpha.Core;

namespace Alpha.EditorScripts
{
    public class BootStrapSetupEditor : EditorWindow
    {
        [MenuItem("Tools/Alpha/Revenge BootStrap (Auto Setup)")]
        public static void SetupBootStrap()
        {
            if (!EditorUtility.DisplayDialog("BootStrap Setup", "BootStrapシーンを自動構築し、既存のグローバルマネージャーを移行しますか？\n（現在のシーンは自動保存されます）", "Yes", "Cancel"))
            {
                return;
            }

            // 1. Save current scene if dirty
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            string tutorialScenePath = "Assets/Scenes/alphaScene/TutorialStage_Alpha.unity";
            string titleScenePath = "Assets/Scenes/alphaScene/Title_Alpha.unity";
            string bootstrapScenePath = "Assets/Scenes/alphaScene/BootStrap_Alpha.unity";
            string prefabPath = "Assets/Prefabs/GlobalSystem_Alpha.prefab";

            // 2. Open Tutorial Scene to extract configured managers
            Scene tutorialScene = EditorSceneManager.OpenScene(tutorialScenePath, OpenSceneMode.Single);
            
            // Find configured managers
            var oldPool = FindObjectOfType<Alpha_ObjectPoolManager>(true);
            var oldCursor = FindObjectOfType<CursorManager_Alpha>(true);
            var oldSave = FindObjectOfType<SaveManager_Alpha>(true);

            if (oldPool == null && oldCursor == null)
            {
                Debug.LogWarning("マネージャーがTutorialStageに見つかりませんでした。TitleSceneを探します。");
                EditorSceneManager.OpenScene(titleScenePath, OpenSceneMode.Single);
                oldPool = FindObjectOfType<Alpha_ObjectPoolManager>(true);
                oldCursor = FindObjectOfType<CursorManager_Alpha>(true);
                oldSave = FindObjectOfType<SaveManager_Alpha>(true);
            }

            // Create a temporary object to hold the global components
            GameObject globalGo = new GameObject("GlobalSystem_Alpha");

            // Copy components (using reflection or UnityEditorUtility)
            if (oldPool != null) UnityEditorInternal.ComponentUtility.CopyComponent(oldPool);
            if (oldPool != null) UnityEditorInternal.ComponentUtility.PasteComponentAsNew(globalGo);
            
            if (oldCursor != null) UnityEditorInternal.ComponentUtility.CopyComponent(oldCursor);
            if (oldCursor != null) UnityEditorInternal.ComponentUtility.PasteComponentAsNew(globalGo);

            if (oldSave != null) UnityEditorInternal.ComponentUtility.CopyComponent(oldSave);
            if (oldSave != null) UnityEditorInternal.ComponentUtility.PasteComponentAsNew(globalGo);

            // If we couldn't find them, just add default ones
            if (globalGo.GetComponent<Alpha_ObjectPoolManager>() == null) globalGo.AddComponent<Alpha_ObjectPoolManager>();
            if (globalGo.GetComponent<CursorManager_Alpha>() == null) globalGo.AddComponent<CursorManager_Alpha>();
            if (globalGo.GetComponent<SaveManager_Alpha>() == null) globalGo.AddComponent<SaveManager_Alpha>();

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            // Save as Prefab
            GameObject globalPrefab = PrefabUtility.SaveAsPrefabAsset(globalGo, prefabPath);
            DestroyImmediate(globalGo);

            // 3. Remove old managers from Tutorial and Title to prevent duplicates
            RemoveManagersFromScene(tutorialScenePath);
            RemoveManagersFromScene(titleScenePath);

            // 4. Create BootStrap Scene
            Scene bootstrapScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Add Camera (optional but good for safety)
            GameObject cam = new GameObject("Main Camera");
            cam.AddComponent<Camera>().backgroundColor = Color.black;

            // Instantiate Global Prefab
            PrefabUtility.InstantiatePrefab(globalPrefab);

            // Add BootStrapManager
            GameObject bootstrapMgrGo = new GameObject("BootStrapManager");
            bootstrapMgrGo.AddComponent<BootStrapManager_Alpha>();

            // Save Scene
            EditorSceneManager.SaveScene(bootstrapScene, bootstrapScenePath);

            // 5. Update Build Settings
            var originalScenes = EditorBuildSettings.scenes;
            var newScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            
            // Add BootStrap as index 0
            newScenes.Add(new EditorBuildSettingsScene(bootstrapScenePath, true));
            
            // Add Title
            newScenes.Add(new EditorBuildSettingsScene(titleScenePath, true));
            
            // Add Tutorial
            newScenes.Add(new EditorBuildSettingsScene(tutorialScenePath, true));

            EditorBuildSettings.scenes = newScenes.ToArray();

            Debug.Log("<color=green>[Success] BootStrapシーンの自動構築、マネージャーの移行、BuildSettingsの設定が完了しました！</color>");
            EditorUtility.DisplayDialog("Setup Complete", "BootStrapの構築が完了しました。\nBootStrapシーンからPlayボタンを押して動作確認してください。", "OK");
        }

        private static void RemoveManagersFromScene(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            var pool = FindObjectOfType<Alpha_ObjectPoolManager>(true);
            if (pool != null) DestroyImmediate(pool);
            
            var cursor = FindObjectOfType<CursorManager_Alpha>(true);
            if (cursor != null) DestroyImmediate(cursor);

            var save = FindObjectOfType<SaveManager_Alpha>(true);
            if (save != null) DestroyImmediate(save);

            EditorSceneManager.SaveScene(scene);
        }
    }
}
