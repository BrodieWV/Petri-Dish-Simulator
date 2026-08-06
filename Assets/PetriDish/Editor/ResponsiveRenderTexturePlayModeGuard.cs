#if UNITY_EDITOR
using System.Reflection;
using PetriDish.Presentation;
using UnityEditor;
using UnityEngine;

namespace PetriDish.Editor
{
    /// <summary>
    /// Detaches all runtime-created dish cameras from their RenderTextures before
    /// Unity tears down Play Mode objects. Hidden runtime objects are included.
    /// </summary>
    [InitializeOnLoad]
    internal static class ResponsiveRenderTexturePlayModeGuard
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

        static ResponsiveRenderTexturePlayModeGuard()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
                return;

            // FindFirstObjectByType does not reliably return hidden runtime helpers.
            // Resources.FindObjectsOfTypeAll includes HideAndDontSave objects.
            ResponsiveUIRuntimeFixes[] runtimeFixes =
                Resources.FindObjectsOfTypeAll<ResponsiveUIRuntimeFixes>();

            FieldInfo cameraField = typeof(ResponsiveUIRuntimeFixes).GetField("dishCamera", Flags);
            FieldInfo textureField = typeof(ResponsiveUIRuntimeFixes).GetField("dishRenderTexture", Flags);
            FieldInfo imageField = typeof(ResponsiveUIRuntimeFixes).GetField("dishCameraImage", Flags);

            foreach (ResponsiveUIRuntimeFixes runtimeFix in runtimeFixes)
            {
                if (runtimeFix == null)
                    continue;

                Camera camera = cameraField?.GetValue(runtimeFix) as Camera;
                RenderTexture texture = textureField?.GetValue(runtimeFix) as RenderTexture;
                UnityEngine.UI.RawImage image = imageField?.GetValue(runtimeFix) as UnityEngine.UI.RawImage;

                if (camera != null)
                    camera.targetTexture = null;

                if (image != null && image.texture == texture)
                    image.texture = null;

                // Prevent runtime OnDestroy from releasing a texture after Unity has
                // already begun Play Mode teardown.
                textureField?.SetValue(runtimeFix, null);
            }

            // Defensive sweep for duplicate or stale runtime cameras.
            Camera[] cameras = Resources.FindObjectsOfTypeAll<Camera>();
            foreach (Camera camera in cameras)
            {
                if (camera == null || camera.name != "ResponsiveDishCamera")
                    continue;
                camera.targetTexture = null;
            }
        }
    }
}
#endif
