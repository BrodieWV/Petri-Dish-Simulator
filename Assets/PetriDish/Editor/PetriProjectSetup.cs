#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PetriDish.Editor
{
    public static class PetriProjectSetup
    {
        private const string SceneDirectory = "Assets/PetriDish/Scenes";
        private const string ScenePath = SceneDirectory + "/PetriDishVerticalSlice.unity";

        [MenuItem("Petri Dish/Setup Vertical Slice Project")]
        public static void Setup()
        {
            if (!Directory.Exists(SceneDirectory)) Directory.CreateDirectory(SceneDirectory);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            cameraObject.GetComponent<Camera>().backgroundColor = new Color(0.05f, 0.07f, 0.06f);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            PlayerSettings.productName = "Petri Dish Simulator";
            PlayerSettings.companyName = "BrodieWV Game Studio";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.brodiewv.petridishsimulator");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(ScenePath);
            Debug.Log("Petri Dish vertical slice project created. Enter Play Mode to run it.");
        }
    }
}
#endif
