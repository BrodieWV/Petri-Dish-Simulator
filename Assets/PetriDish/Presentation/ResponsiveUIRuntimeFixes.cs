using System;
using System.Collections;
using System.Reflection;
using PetriDish.Application;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PetriDish.Presentation
{
    /// <summary>
    /// Runtime repair layer for the scene-generated responsive interface.
    /// Keeps the simulation texture as the authoritative colony source while
    /// showing the actual 3D dish through a dedicated camera and RenderTexture.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ResponsiveUIRuntimeFixes : MonoBehaviour
    {
        private const float MinimumTemperature = 8f;
        private const float MaximumTemperature = 42f;
        private const float TemperatureStep = 0.5f;

        private PetriDishResponsiveUIBinder binder;
        private ExperimentController controller;
        private Button moistureDisplayButton;
        private Button temperatureLegacyButton;
        private Slider temperatureSlider;
        private Text temperatureReadout;
        private Button temperatureMinusButton;
        private Button temperaturePlusButton;
        private Camera dishCamera;
        private RenderTexture dishRenderTexture;
        private RawImage dishCameraImage;
        private bool ready;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallLoader()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GameObject loader = new GameObject("ResponsiveUIRuntimeFixesLoader");
            loader.hideFlags = HideFlags.HideAndDontSave;
            loader.AddComponent<ResponsiveUIRuntimeFixes>();
        }

        private IEnumerator Start()
        {
            for (int frame = 0; frame < 180; frame++)
            {
                binder = FindFirstObjectByType<PetriDishResponsiveUIBinder>();
                if (binder != null && binder.IsInitialized)
                {
                    controller = GetPrivateField<ExperimentController>(binder, "controller");
                    if (controller != null)
                        break;
                }
                yield return null;
            }

            if (binder == null || controller == null)
            {
                Destroy(gameObject);
                yield break;
            }

            FixMoistureDisplay();
            BuildTemperatureController();
            yield return BuildThreeDimensionalViewport();
            ready = true;
        }

        private void Update()
        {
            if (!ready || controller == null || controller.Simulation == null)
                return;

            float target = Mathf.Clamp(
                controller.Simulation.TargetTemperature,
                MinimumTemperature,
                MaximumTemperature);

            if (temperatureSlider != null)
                temperatureSlider.SetValueWithoutNotify(target);
            if (temperatureReadout != null)
                temperatureReadout.text = string.Format("TARGET  {0:0.0} °C", target);

            if (temperatureMinusButton != null)
                temperatureMinusButton.interactable = target > MinimumTemperature + 0.001f;
            if (temperaturePlusButton != null)
                temperaturePlusButton.interactable = target < MaximumTemperature - 0.001f;
        }

        private void FixMoistureDisplay()
        {
            moistureDisplayButton = FindComponentByName<Button>(binder.transform, "MoistureButton");
            if (moistureDisplayButton == null)
                return;

            // The metric display is read-only. Only AddMoistureButton may intervene.
            moistureDisplayButton.onClick.RemoveAllListeners();
            moistureDisplayButton.enabled = false;
            Text[] labels = moistureDisplayButton.GetComponentsInChildren<Text>(true);
            foreach (Text label in labels)
                label.raycastTarget = false;
        }

        private void BuildTemperatureController()
        {
            temperatureLegacyButton = FindComponentByName<Button>(binder.transform, "TemperatureButton");
            if (temperatureLegacyButton == null)
                return;

            temperatureLegacyButton.onClick.RemoveAllListeners();
            temperatureLegacyButton.enabled = false;

            Transform old = temperatureLegacyButton.transform.Find("ScientificTemperatureController");
            if (old != null)
                Destroy(old.gameObject);

            RectTransform host = temperatureLegacyButton.GetComponent<RectTransform>();
            Text legacyLabel = temperatureLegacyButton.GetComponentInChildren<Text>(true);
            if (legacyLabel != null)
            {
                temperatureReadout = legacyLabel;
                temperatureReadout.alignment = TextAnchor.UpperCenter;
                temperatureReadout.fontSize = Mathf.Max(10, temperatureReadout.fontSize);
                temperatureReadout.raycastTarget = false;
                RectTransform labelRect = temperatureReadout.rectTransform;
                labelRect.anchorMin = new Vector2(0f, 0.52f);
                labelRect.anchorMax = new Vector2(1f, 1f);
                labelRect.offsetMin = new Vector2(4f, 0f);
                labelRect.offsetMax = new Vector2(-4f, -1f);
            }

            GameObject controls = CreateUiObject("ScientificTemperatureController", host);
            RectTransform controlsRect = controls.GetComponent<RectTransform>();
            controlsRect.anchorMin = new Vector2(0f, 0f);
            controlsRect.anchorMax = new Vector2(1f, 0.56f);
            controlsRect.offsetMin = new Vector2(4f, 3f);
            controlsRect.offsetMax = new Vector2(-4f, -1f);

            temperatureMinusButton = CreateInstrumentButton(
                controls.transform,
                "TemperatureMinus",
                "−",
                new Vector2(0f, 0f),
                new Vector2(0.18f, 1f));
            temperaturePlusButton = CreateInstrumentButton(
                controls.transform,
                "TemperaturePlus",
                "+",
                new Vector2(0.82f, 0f),
                new Vector2(1f, 1f));

            temperatureSlider = CreateInstrumentSlider(controls.transform);
            temperatureSlider.minValue = MinimumTemperature;
            temperatureSlider.maxValue = MaximumTemperature;
            temperatureSlider.wholeNumbers = false;
            temperatureSlider.SetValueWithoutNotify(
                Mathf.Clamp(controller.Simulation.TargetTemperature, MinimumTemperature, MaximumTemperature));

            temperatureMinusButton.onClick.AddListener(() => AdjustTemperature(-TemperatureStep));
            temperaturePlusButton.onClick.AddListener(() => AdjustTemperature(TemperatureStep));
            temperatureSlider.onValueChanged.AddListener(OnTemperatureSliderChanged);
        }

        private void AdjustTemperature(float delta)
        {
            if (controller == null || controller.Simulation == null)
                return;

            float next = Mathf.Clamp(
                Mathf.Round((controller.Simulation.TargetTemperature + delta) / TemperatureStep) * TemperatureStep,
                MinimumTemperature,
                MaximumTemperature);
            controller.SetTemperature(next);
        }

        private void OnTemperatureSliderChanged(float value)
        {
            if (controller == null || controller.Simulation == null)
                return;

            float stepped = Mathf.Clamp(
                Mathf.Round(value / TemperatureStep) * TemperatureStep,
                MinimumTemperature,
                MaximumTemperature);
            temperatureSlider.SetValueWithoutNotify(stepped);
            controller.SetTemperature(stepped);
        }

        private IEnumerator BuildThreeDimensionalViewport()
        {
            RectTransform renderTarget = FindComponentByName<RectTransform>(binder.transform, "DishRenderTarget");
            if (renderTarget == null)
                yield break;

            ColonySurfacePresenter presenter = FindFirstObjectByType<ColonySurfacePresenter>();
            if (presenter == null)
                yield break;

            Renderer colonyRenderer = presenter.GetComponent<Renderer>();
            if (colonyRenderer == null)
                colonyRenderer = presenter.GetComponentInChildren<Renderer>(true);
            if (colonyRenderer == null)
                yield break;

            // Allow the presenter to receive and bind the first simulation texture.
            yield return null;
            yield return new WaitForEndOfFrame();

            Bounds dishBounds = CollectDishBounds(colonyRenderer);
            CreateDishCamera(dishBounds);
            CreateViewportImage(renderTarget);

            // Hide the old flat simulation image without disabling its DishRenderer.
            // The new transparent hit area forwards taps to the binder.
            DishRenderer simulationTextureSource = binder.ColonyTextureSource;
            if (simulationTextureSource != null)
            {
                RawImage flatImage = simulationTextureSource.GetComponent<RawImage>();
                if (flatImage != null)
                {
                    Color hidden = flatImage.color;
                    hidden.a = 0f;
                    flatImage.color = hidden;
                    flatImage.raycastTarget = false;
                }
            }
        }

        private Bounds CollectDishBounds(Renderer colonyRenderer)
        {
            Bounds bounds = colonyRenderer.bounds;
            float radius = Mathf.Max(0.05f, Mathf.Max(bounds.size.x, bounds.size.z) * 2.5f);
            Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (Renderer candidate in allRenderers)
            {
                if (candidate == null || !candidate.enabled)
                    continue;
                if (Vector3.Distance(candidate.bounds.center, bounds.center) > radius)
                    continue;
                bounds.Encapsulate(candidate.bounds);
            }
            return bounds;
        }

        private void CreateDishCamera(Bounds bounds)
        {
            GameObject cameraObject = new GameObject("ResponsiveDishCamera");
            cameraObject.hideFlags = HideFlags.DontSave;
            dishCamera = cameraObject.AddComponent<Camera>();
            dishCamera.orthographic = true;
            dishCamera.clearFlags = CameraClearFlags.SolidColor;
            dishCamera.backgroundColor = new Color(0.005f, 0.018f, 0.025f, 1f);
            dishCamera.nearClipPlane = 0.01f;
            dishCamera.farClipPlane = Mathf.Max(10f, bounds.size.magnitude * 10f);
            dishCamera.depth = -20f;

            float horizontalExtent = Mathf.Max(bounds.extents.x, 0.01f);
            float depthExtent = Mathf.Max(bounds.extents.z, 0.01f);
            dishCamera.orthographicSize = Mathf.Max(horizontalExtent, depthExtent) * 1.22f;
            float height = Mathf.Max(0.25f, bounds.size.magnitude * 2.5f);
            cameraObject.transform.position = bounds.center + Vector3.up * height;
            cameraObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            dishRenderTexture = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32)
            {
                name = "ResponsiveDishRenderTexture",
                antiAliasing = 4,
                filterMode = FilterMode.Bilinear
            };
            dishRenderTexture.Create();
            dishCamera.targetTexture = dishRenderTexture;
        }

        private void CreateViewportImage(RectTransform renderTarget)
        {
            Transform existing = renderTarget.Find("ThreeDimensionalDishView");
            if (existing != null)
                Destroy(existing.gameObject);

            GameObject imageObject = CreateUiObject("ThreeDimensionalDishView", renderTarget);
            imageObject.transform.SetAsLastSibling();
            dishCameraImage = imageObject.AddComponent<RawImage>();
            dishCameraImage.texture = dishRenderTexture;
            dishCameraImage.color = Color.white;
            dishCameraImage.raycastTarget = true;

            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            AspectRatioFitter fitter = imageObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 1f;

            ViewportTapForwarder forwarder = imageObject.AddComponent<ViewportTapForwarder>();
            forwarder.Configure(binder, rect);
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject created = new GameObject(name, typeof(RectTransform));
            created.transform.SetParent(parent, false);
            return created;
        }

        private static Button CreateInstrumentButton(
            Transform parent,
            string name,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            GameObject buttonObject = CreateUiObject(name, parent);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = new Vector2(1f, 1f);
            rect.offsetMax = new Vector2(-1f, -1f);

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.02f, 0.16f, 0.20f, 1f);
            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colours = button.colors;
            colours.normalColor = Color.white;
            colours.highlightedColor = new Color(0.65f, 1f, 1f, 1f);
            colours.pressedColor = new Color(0.25f, 0.8f, 0.85f, 1f);
            colours.disabledColor = new Color(0.25f, 0.35f, 0.38f, 0.65f);
            button.colors = colours;

            GameObject textObject = CreateUiObject("Label", buttonObject.transform);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = label;
            text.fontSize = 16;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.65f, 0.98f, 1f, 1f);
            text.raycastTarget = false;
            return button;
        }

        private static Slider CreateInstrumentSlider(Transform parent)
        {
            GameObject sliderObject = CreateUiObject("TemperatureSlider", parent);
            RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.20f, 0.12f);
            sliderRect.anchorMax = new Vector2(0.80f, 0.88f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            Slider slider = sliderObject.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;

            GameObject background = CreateUiObject("Background", sliderObject.transform);
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0f, 0.38f);
            backgroundRect.anchorMax = new Vector2(1f, 0.62f);
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0.03f, 0.10f, 0.13f, 1f);

            GameObject fillArea = CreateUiObject("Fill Area", sliderObject.transform);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.38f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.62f);
            fillAreaRect.offsetMin = new Vector2(4f, 0f);
            fillAreaRect.offsetMax = new Vector2(-4f, 0f);
            GameObject fill = CreateUiObject("Fill", fillArea.transform);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.05f, 0.75f, 0.82f, 1f);

            GameObject handleArea = CreateUiObject("Handle Slide Area", sliderObject.transform);
            RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(6f, 0f);
            handleAreaRect.offsetMax = new Vector2(-6f, 0f);
            GameObject handle = CreateUiObject("Handle", handleArea.transform);
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(9f, 20f);
            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(0.75f, 1f, 1f, 1f);

            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            return slider;
        }

        private static T FindComponentByName<T>(Transform root, string objectName) where T : Component
        {
            Transform found = FindTransform(root, objectName);
            return found != null ? found.GetComponent<T>() : null;
        }

        private static Transform FindTransform(Transform root, string objectName)
        {
            if (root.name == objectName)
                return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindTransform(root.GetChild(i), objectName);
                if (found != null)
                    return found;
            }
            return null;
        }

        private static T GetPrivateField<T>(object instance, string fieldName) where T : class
        {
            FieldInfo field = instance.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            return field != null ? field.GetValue(instance) as T : null;
        }

        private void OnDestroy()
        {
            if (dishCamera != null)
                Destroy(dishCamera.gameObject);
            if (dishRenderTexture != null)
            {
                dishRenderTexture.Release();
                Destroy(dishRenderTexture);
            }
        }
    }

    public sealed class ViewportTapForwarder : MonoBehaviour, IPointerClickHandler
    {
        private PetriDishResponsiveUIBinder binder;
        private RectTransform target;
        private MethodInfo dishTapMethod;

        public void Configure(PetriDishResponsiveUIBinder value, RectTransform rect)
        {
            binder = value;
            target = rect;
            dishTapMethod = typeof(PetriDishResponsiveUIBinder).GetMethod(
                "OnDishTapped",
                BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (binder == null || target == null || dishTapMethod == null)
                return;

            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    target,
                    eventData.position,
                    eventData.pressEventCamera,
                    out local))
                return;

            Rect rect = target.rect;
            float x = Mathf.InverseLerp(rect.xMin, rect.xMax, local.x);
            float y = Mathf.InverseLerp(rect.yMin, rect.yMax, local.y);
            dishTapMethod.Invoke(binder, new object[] { new Vector2(x, y) });
        }
    }
}
