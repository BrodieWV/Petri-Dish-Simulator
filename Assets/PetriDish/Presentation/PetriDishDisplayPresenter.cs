using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
        [Header("Dish inspection")]
        [SerializeField, Min(0f)] private float yawLimit = 55f;
        [SerializeField] private Vector2 pitchLimits = new Vector2(-18f, 32f);
        [SerializeField] private Vector2 zoomLimits = new Vector2(0.78f, 1.18f);
        [SerializeField, Min(0.01f)] private float orbitDegreesPerPixel = 0.16f;
        [SerializeField, Min(0.001f)] private float mouseWheelZoomStep = 0.08f;
        [SerializeField, Min(0.0001f)] private float pinchZoomPerPixel = 0.003f;

        private RenderTexture activeRenderTexture;
        private Quaternion defaultPivotRotation = Quaternion.identity;
        private bool defaultViewCaptured;
        private float yawDegrees;
        private float pitchDegrees;
        private float zoom = 1f;
        private bool mouseDragging;
        private Vector2 previousMousePosition;
        private int primaryTouchId = -1;
        private int secondaryTouchId = -1;
        private readonly List<RaycastResult> pointerRaycastResults = new List<RaycastResult>(8);
        private PointerEventData pointerEventData;
        private EventSystem pointerEventSystem;

        public Transform RotationPivot => rotationPivot;
        public Camera DisplayCamera => displayCamera;
        public RawImage Output => output;
        public RenderTexture ActiveRenderTexture => activeRenderTexture;
        public float FramingScale => framingScale;
        public float VerticalFramingOffset => verticalFramingOffset;
        public float YawDegrees => yawDegrees;
        public float PitchDegrees => pitchDegrees;
        public float Zoom => zoom;
        public Vector2 PitchLimits => pitchLimits;
        public Vector2 ZoomLimits => zoomLimits;

        public void ConfigureRig(Transform pivot, Camera camera, Light[] lights)
        {
            rotationPivot = pivot;
            displayCamera = camera;
            presentationLights = lights;
            CaptureDefaultView(true);
            ApplyView();
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

        /// <summary>
        /// Applies a constrained presentation-only orbit delta in screen pixels.
        /// Positive X rotates right; positive Y tips the near edge upward.
        /// </summary>
        public void OrbitBy(Vector2 screenDelta)
        {
            yawDegrees = Mathf.Clamp(
                yawDegrees + screenDelta.x * orbitDegreesPerPixel,
                -Mathf.Abs(yawLimit),
                Mathf.Abs(yawLimit));
            pitchDegrees = Mathf.Clamp(
                pitchDegrees - screenDelta.y * orbitDegreesPerPixel,
                Mathf.Min(pitchLimits.x, pitchLimits.y),
                Mathf.Max(pitchLimits.x, pitchLimits.y));
            ApplyView();
        }

        /// <summary>Adjusts camera framing without moving or mutating the dish model.</summary>
        public void ZoomBy(float amount)
        {
            zoom = Mathf.Clamp(
                zoom + amount,
                Mathf.Min(zoomLimits.x, zoomLimits.y),
                Mathf.Max(zoomLimits.x, zoomLimits.y));
            if (displayCamera != null && rotationPivot != null)
                FrameCamera();
        }

        /// <summary>Restores the product-authored pivot rotation and framing.</summary>
        public void ResetView()
        {
            yawDegrees = 0f;
            pitchDegrees = 0f;
            zoom = 1f;
            ApplyView();
            if (displayCamera != null && rotationPivot != null)
                FrameCamera();
        }

        /// <summary>
        /// Returns true only when the point is inside the dish output and is not covered by
        /// another interactive UI graphic. This keeps buttons isolated from dish gestures.
        /// </summary>
        public bool CanBeginInteraction(Vector2 screenPosition)
        {
            if (output == null || !output.isActiveAndEnabled ||
                !RectTransformUtility.RectangleContainsScreenPoint(
                    output.rectTransform,
                    screenPosition,
                    OutputEventCamera()))
                return false;

            EventSystem eventSystem = EventSystem.current;
            if (HasBlockingGraphicAboveOutput(screenPosition))
                return false;
            if (eventSystem == null)
                return true;

            if (pointerEventData == null || pointerEventSystem != eventSystem)
            {
                pointerEventData = new PointerEventData(eventSystem);
                pointerEventSystem = eventSystem;
            }
            pointerEventData.position = screenPosition;
            pointerRaycastResults.Clear();
            eventSystem.RaycastAll(pointerEventData, pointerRaycastResults);
            bool reachedOutput = false;
            for (int i = 0; i < pointerRaycastResults.Count; i++)
            {
                Transform hit = pointerRaycastResults[i].gameObject.transform;
                if (hit == output.transform || hit.IsChildOf(output.transform))
                {
                    reachedOutput = true;
                    continue;
                }
                if (output.transform.IsChildOf(hit))
                    continue;
                if (reachedOutput)
                    break;
                GameObject hitObject = pointerRaycastResults[i].gameObject;
                Graphic blockingGraphic = hitObject.GetComponent<Graphic>();
                if ((blockingGraphic != null && blockingGraphic.raycastTarget) ||
                    hitObject.GetComponentInParent<Selectable>() != null ||
                    ExecuteEvents.GetEventHandler<IPointerClickHandler>(hitObject) != null ||
                    ExecuteEvents.GetEventHandler<IBeginDragHandler>(hitObject) != null ||
                    ExecuteEvents.GetEventHandler<IScrollHandler>(hitObject) != null)
                    return false;
            }
            return true;
        }

        private bool HasBlockingGraphicAboveOutput(Vector2 screenPosition)
        {
            Canvas canvas = output.canvas;
            if (canvas == null)
                return false;

            Graphic[] graphics = canvas.GetComponentsInChildren<Graphic>(false);
            Camera eventCamera = OutputEventCamera();
            bool reachedOutput = false;
            for (int i = 0; i < graphics.Length; i++)
            {
                Graphic graphic = graphics[i];
                if (graphic == output)
                {
                    reachedOutput = true;
                    continue;
                }
                if (!reachedOutput || graphic == null || !graphic.raycastTarget || !graphic.isActiveAndEnabled)
                    continue;

                Transform graphicTransform = graphic.transform;
                if (graphicTransform.IsChildOf(output.transform) || output.transform.IsChildOf(graphicTransform))
                    continue;

                if (RectTransformUtility.RectangleContainsScreenPoint(
                    graphic.rectTransform,
                    screenPosition,
                    eventCamera))
                    return true;
            }
            return false;
        }

        private void Awake()
        {
            CaptureDefaultView(false);
            SanitizeInteractionSettings();
            ApplyView();
        }

        private void OnValidate() => SanitizeInteractionSettings();

        private void OnEnable()
        {
            CaptureDefaultView(false);
            SanitizeInteractionSettings();
            if (!global::UnityEngine.Application.isPlaying)
                return;
            CreateRenderTexture();
        }

        private void Update()
        {
            if (!global::UnityEngine.Application.isPlaying)
                return;
            ProcessTouchInput();
            if (Input.touchCount == 0)
                ProcessMouseInput();
            else
                mouseDragging = false;
        }

        private void OnDisable()
        {
            CancelGesture();
            ReleaseRenderTexture();
        }
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
            float safeZoom = Mathf.Clamp(
                zoom,
                Mathf.Min(zoomLimits.x, zoomLimits.y),
                Mathf.Max(zoomLimits.x, zoomLimits.y));
            float distance = radius * framingPadding / (Mathf.Sin(halfFov) * framingScale * safeZoom);
            Vector3 direction = new Vector3(0f, 0.78f, -1f).normalized;
            Vector3 target = bounds.center - Vector3.up * radius * verticalFramingOffset;
            displayCamera.transform.position = target + direction * distance;
            displayCamera.transform.rotation = Quaternion.LookRotation(target - displayCamera.transform.position, Vector3.up);
            displayCamera.nearClipPlane = Mathf.Max(0.01f, distance - radius * 2f);
            displayCamera.farClipPlane = distance + radius * 3f;
        }

        private void CaptureDefaultView(bool force)
        {
            if (rotationPivot == null || (defaultViewCaptured && !force))
                return;
            defaultPivotRotation = rotationPivot.localRotation;
            defaultViewCaptured = true;
            yawDegrees = 0f;
            pitchDegrees = 0f;
            zoom = 1f;
        }

        private void ApplyView()
        {
            if (rotationPivot == null)
                return;
            if (!defaultViewCaptured)
                CaptureDefaultView(false);
            rotationPivot.localRotation = defaultPivotRotation * Quaternion.Euler(pitchDegrees, yawDegrees, 0f);
        }

        private void SanitizeInteractionSettings()
        {
            yawLimit = Mathf.Max(0f, yawLimit);
            if (pitchLimits.x > pitchLimits.y)
                pitchLimits = new Vector2(pitchLimits.y, pitchLimits.x);
            zoomLimits.x = Mathf.Max(0.1f, zoomLimits.x);
            zoomLimits.y = Mathf.Max(zoomLimits.x, zoomLimits.y);
            zoom = Mathf.Clamp(zoom, zoomLimits.x, zoomLimits.y);
        }

        private void ProcessMouseInput()
        {
            Vector2 current = Input.mousePosition;
            if (Input.GetMouseButtonDown(0))
            {
                mouseDragging = CanBeginInteraction(current);
                previousMousePosition = current;
            }
            else if (Input.GetMouseButton(0) && mouseDragging)
            {
                OrbitBy(current - previousMousePosition);
                previousMousePosition = current;
            }
            if (Input.GetMouseButtonUp(0))
                mouseDragging = false;

            float wheel = Input.mouseScrollDelta.y;
            if (!Mathf.Approximately(wheel, 0f) && CanBeginInteraction(current))
                ZoomBy(wheel * mouseWheelZoomStep);
        }

        private void ProcessTouchInput()
        {
            if (Input.touchCount == 0)
            {
                primaryTouchId = -1;
                secondaryTouchId = -1;
                return;
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase != TouchPhase.Began || !CanBeginInteraction(touch.position))
                    continue;
                if (primaryTouchId < 0)
                    primaryTouchId = touch.fingerId;
                else if (secondaryTouchId < 0 && touch.fingerId != primaryTouchId)
                    secondaryTouchId = touch.fingerId;
            }

            bool hasPrimary = TryGetTouch(primaryTouchId, out Touch primary);
            bool hasSecondary = TryGetTouch(secondaryTouchId, out Touch secondary);
            if (hasPrimary && hasSecondary)
            {
                if (IsFinished(primary) || IsFinished(secondary))
                {
                    CancelTouchGesture();
                    return;
                }
                Vector2 previousPrimary = primary.position - primary.deltaPosition;
                Vector2 previousSecondary = secondary.position - secondary.deltaPosition;
                float distanceDelta = Vector2.Distance(primary.position, secondary.position) -
                                      Vector2.Distance(previousPrimary, previousSecondary);
                if (!Mathf.Approximately(distanceDelta, 0f))
                    ZoomBy(distanceDelta * pinchZoomPerPixel);
                return;
            }

            if (!hasPrimary)
            {
                primaryTouchId = -1;
                secondaryTouchId = -1;
                return;
            }
            if (IsFinished(primary))
            {
                CancelTouchGesture();
                return;
            }
            if (primary.phase == TouchPhase.Moved)
                OrbitBy(primary.deltaPosition);
        }

        private static bool IsFinished(Touch touch) =>
            touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;

        private static bool TryGetTouch(int fingerId, out Touch result)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.fingerId != fingerId)
                    continue;
                result = touch;
                return true;
            }
            result = default;
            return false;
        }

        private Camera OutputEventCamera()
        {
            Canvas canvas = output != null ? output.canvas : null;
            return canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
        }

        private void CancelTouchGesture()
        {
            primaryTouchId = -1;
            secondaryTouchId = -1;
        }

        private void CancelGesture()
        {
            mouseDragging = false;
            CancelTouchGesture();
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
