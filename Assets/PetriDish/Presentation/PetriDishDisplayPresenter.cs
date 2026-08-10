using UnityEngine;
using UnityEngine.UI;

namespace PetriDish.Presentation
{
    /// <summary>
    /// Owns the isolated camera and RenderTexture used to present the shared 3D dish.
    /// The model sits below RotationPivot so future orbit controls do not move the camera.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PetriDishDisplayPresenter : MonoBehaviour
    {
        [SerializeField] private Transform rotationPivot;
        [SerializeField] private Camera displayCamera;
        [SerializeField] private Light[] presentationLights;
        [SerializeField] private RawImage output;
        [SerializeField, Min(256)] private int renderTextureSize = 768;
        [SerializeField, Min(1f)] private float framingPadding = 1.18f;
        [SerializeField, Min(1f)] private float framingScale = 1f;
        [SerializeField, Range(-0.25f, 0.25f)] private float verticalFramingOffset;

        private RenderTexture activeRenderTexture;

        public Transform RotationPivot => rotationPivot;
        public Camera DisplayCamera => displayCamera;
        public RawImage Output => output;
        public RenderTexture ActiveRenderTexture => activeRenderTexture;
        public float FramingScale => framingScale;
        public float VerticalFramingOffset => verticalFramingOffset;

        public void ConfigureRig(Transform pivot, Camera camera, Light[] lights)
        {
            rotationPivot = pivot;
            displayCamera = camera;
            presentationLights = lights;
        }

        public void ConfigureOutput(RawImage target)
        {
            if (output != null && output.texture == activeRenderTexture)
                output.texture = null;
            output = target;
            if (output != null && activeRenderTexture != null)
                output.texture = activeRenderTexture;
        }

        public void ConfigureFraming(float scale, float verticalOffset)
        {
            framingScale = Mathf.Max(1f, scale);
            verticalFramingOffset = Mathf.Clamp(verticalOffset, -0.25f, 0.25f);
            if (activeRenderTexture != null)
                FrameCamera();
        }

        private void OnEnable()
        {
            if (!global::UnityEngine.Application.isPlaying)
                return;
            CreateRenderTexture();
        }

        private void OnDisable() => ReleaseRenderTexture();
        private void OnDestroy() => ReleaseRenderTexture();

        public void ReleaseRenderTexture()
        {
            if (displayCamera != null && displayCamera.targetTexture == activeRenderTexture)
                displayCamera.targetTexture = null;
            if (output != null && output.texture == activeRenderTexture)
                output.texture = null;
            if (displayCamera != null)
                displayCamera.enabled = false;
            SetLightsEnabled(false);

            if (activeRenderTexture == null)
                return;
            activeRenderTexture.Release();
            Destroy(activeRenderTexture);
            activeRenderTexture = null;
        }

        private void CreateRenderTexture()
        {
            ReleaseRenderTexture();
            if (rotationPivot == null || displayCamera == null || output == null)
            {
                Debug.LogError("PetriDishDisplayPresenter requires a rotation pivot, display camera, and RawImage output.", this);
                return;
            }

            FrameCamera();
            activeRenderTexture = new RenderTexture(renderTextureSize, renderTextureSize, 24, RenderTextureFormat.ARGB32)
            {
                name = "LaboratoryHubDishRenderTexture",
                antiAliasing = 4,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            activeRenderTexture.Create();
            displayCamera.targetTexture = activeRenderTexture;
            output.texture = activeRenderTexture;
            displayCamera.enabled = true;
            SetLightsEnabled(true);
        }

        private void FrameCamera()
        {
            Renderer[] renderers = rotationPivot.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            float radius = Mathf.Max(bounds.extents.magnitude, 0.1f);
            float halfFov = Mathf.Max(1f, displayCamera.fieldOfView * 0.5f) * Mathf.Deg2Rad;
            float distance = radius * framingPadding / (Mathf.Sin(halfFov) * framingScale);
            Vector3 direction = new Vector3(0f, 0.78f, -1f).normalized;
            Vector3 target = bounds.center - Vector3.up * radius * verticalFramingOffset;
            displayCamera.transform.position = target + direction * distance;
            displayCamera.transform.rotation = Quaternion.LookRotation(target - displayCamera.transform.position, Vector3.up);
            displayCamera.nearClipPlane = Mathf.Max(0.01f, distance - radius * 2f);
            displayCamera.farClipPlane = distance + radius * 3f;
        }

        private void SetLightsEnabled(bool value)
        {
            if (presentationLights == null)
                return;
            foreach (Light presentationLight in presentationLights)
                if (presentationLight != null)
                    presentationLight.enabled = value;
        }
    }
}
