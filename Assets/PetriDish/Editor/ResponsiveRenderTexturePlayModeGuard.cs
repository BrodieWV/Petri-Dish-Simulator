#if UNITY_EDITOR
using System.Reflection;
using PetriDish.Presentation;
using UnityEditor;
using UnityEngine;

namespace PetriDish.Editor
{
    /// <summary>
    /// Ensures the responsive dish camera is detached from its RenderTexture
    /// before Unity begins tearing down Play Mode objects.
    /// </summary>
    [InitializeOnLoad]
    internal static class ResponsiveRenderTexturePlayModeGuard
    {
        static ResponsiveRenderTexturePlayModeGuard()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
                return;

            ResponsiveUIRuntimeFixes runtimeFixes =
                Object.FindFirstObjectByType<ResponsiveUIRuntimeFixes>();

            if (runtimeFixes == null)
                return;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo cameraField = typeof(ResponsiveUIRuntimeFixes).GetField("dishCamera", flags);
            FieldInfo textureField = typeof(ResponsiveUIRuntimeFixes).GetField("dishRenderTexture", flags);

            Camera camera = cameraField?.GetValue(runtimeFixes) as Camera;
            if (camera != null)
                camera.targetTexture = null;

            // Prevent ResponsiveUIRuntimeFixes.OnDestroy from explicitly releasing
            // a texture that Unity is already tearing down during Play Mode exit.
            textureField?.SetValue(runtimeFixes, null);
        }
    }
}
#endif
