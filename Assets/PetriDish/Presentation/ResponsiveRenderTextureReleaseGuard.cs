using UnityEngine;
using UnityEngine.SceneManagement;

namespace PetriDish.Presentation
{
    /// <summary>
    /// Detaches the responsive dish camera from its RenderTexture before Unity
    /// releases scene resources. This prevents Unity from warning that a camera
    /// still references a RenderTexture being released.
    /// </summary>
    internal static class ResponsiveRenderTextureReleaseGuard
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            global::UnityEngine.Application.quitting -= DetachResponsiveDishCamera;
            global::UnityEngine.Application.quitting += DetachResponsiveDishCamera;
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            DetachResponsiveDishCamera();
        }

        private static void DetachResponsiveDishCamera()
        {
            GameObject cameraObject = GameObject.Find("ResponsiveDishCamera");
            if (cameraObject == null)
                return;

            Camera cameraComponent = cameraObject.GetComponent<Camera>();
            if (cameraComponent != null)
                cameraComponent.targetTexture = null;
        }
    }
}
