using PetriDish.Application;
using PetriDish.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PetriDish.Presentation
{
    public sealed class RuntimeBootstrap : MonoBehaviour
    {
        private ExperimentController controller;
        private DishRenderer renderer;
        private Text instruction;
        private Text condition;
        private Text metrics;
        private Text temperatureValue;
        private Text outcome;
        private Slider temperature;
        private Button moisture;
        private Font font;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartRuntime()
        {
            if (FindObjectOfType<RuntimeBootstrap>() != null) return;
            var root = new GameObject("PetriDishRuntime");
            DontDestroyOnLoad(root);
            root.AddComponent<ExperimentController>();
            root.AddComponent<RuntimeBootstrap>();
        }

        private void Awake()
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            controller = GetComponent<ExperimentController>();
            CreateEventSystem();
            BuildUI();
            controller.SnapshotUpdated += OnSnapshot;
            controller.StageChanged += OnStage;
        }

        private void OnDestroy()
        {
            if (controller == null) return;
            controller.SnapshotUpdated -= OnSnapshot;
            controller.StageChanged -= OnStage;
        }

        private void CreateEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(go);
        }

        private void BuildUI()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            var bg = Image(canvasGo.transform, "Background", new Color(0.05f, 0.07f, 0.06f));
            SetRect(bg.rectTransform, Vector2.zero, Vector2.one);

            instruction = Text(bg.transform, "Instruction", 34, TextAnchor.MiddleCenter);
            SetRect(instruction.rectTransform, new Vector2(0.04f, 0.89f), new Vector2(0.96f, 0.98f));
            instruction.text = "The Comfortable Range";

            condition = Text(bg.transform, "Condition", 29, TextAnchor.MiddleLeft);
            SetRect(condition.rectTransform, new Vector2(0.05f, 0.82f), new Vector2(0.50f, 0.89f));
            metrics = Text(bg.transform, "Metrics", 24, TextAnchor.MiddleRight);
            SetRect(metrics.rectTransform, new Vector2(0.48f, 0.82f), new Vector2(0.95f, 0.89f));

            var dishPanel = Image(bg.transform, "DishPanel", new Color(0.11f, 0.15f, 0.13f));
            SetRect(dishPanel.rectTransform, new Vector2(0.08f, 0.35f), new Vector2(0.92f, 0.80f));
            var dish = new GameObject("Dish", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter), typeof(DishRenderer));
            dish.transform.SetParent(dishPanel.transform, false);
            SetRect(dish.GetComponent<RectTransform>(), new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.96f));
            dish.GetComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            dish.GetComponent<AspectRatioFitter>().aspectRatio = 1f;
            renderer = dish.GetComponent<DishRenderer>();

            outcome = Text(bg.transform, "Outcome", 28, TextAnchor.MiddleCenter);
            SetRect(outcome.rectTransform, new Vector2(0.06f, 0.315f), new Vector2(0.94f, 0.35f));

            var controls = Image(bg.transform, "Controls", new Color(0.08f, 0.11f, 0.10f));
            SetRect(controls.rectTransform, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.31f));

            var tempLabel = Text(controls.transform, "TemperatureLabel", 27, TextAnchor.MiddleLeft);
            SetRect(tempLabel.rectTransform, new Vector2(0.04f, 0.74f), new Vector2(0.50f, 0.96f));
            tempLabel.text = "Temperature";
            temperatureValue = Text(controls.transform, "TemperatureValue", 27, TextAnchor.MiddleRight);
            SetRect(temperatureValue.rectTransform, new Vector2(0.50f, 0.74f), new Vector2(0.96f, 0.96f));

            temperature = CreateSlider(controls.transform);
            SetRect(temperature.GetComponent<RectTransform>(), new Vector2(0.06f, 0.56f), new Vector2(0.94f, 0.73f));
            temperature.onValueChanged.AddListener(v =>
            {
                controller.SetTemperature(v);
                temperatureValue.text = v.ToString("0.0") + "°C target";
            });

            moisture = CreateButton(controls.transform, "Add moisture", controller.AddMoisture);
            SetRect(moisture.GetComponent<RectTransform>(), new Vector2(0.04f, 0.30f), new Vector2(0.37f, 0.52f));
            var pause = CreateButton(controls.transform, "Pause / Resume", controller.TogglePause);
            SetRect(pause.GetComponent<RectTransform>(), new Vector2(0.39f, 0.30f), new Vector2(0.72f, 0.52f));
            var speed = CreateButton(controls.transform, "Speed", CycleSpeed);
            SetRect(speed.GetComponent<RectTransform>(), new Vector2(0.74f, 0.30f), new Vector2(0.96f, 0.52f));

            SetRect(CreateButton(controls.transform, "Save", controller.Save).GetComponent<RectTransform>(), new Vector2(0.04f, 0.04f), new Vector2(0.24f, 0.25f));
            SetRect(CreateButton(controls.transform, "Load", () => controller.Load()).GetComponent<RectTransform>(), new Vector2(0.26f, 0.04f), new Vector2(0.46f, 0.25f));
            SetRect(CreateButton(controls.transform, "Restart", controller.RestartSameSeed).GetComponent<RectTransform>(), new Vector2(0.48f, 0.04f), new Vector2(0.70f, 0.25f));
            SetRect(CreateButton(controls.transform, "New seed", controller.RestartNewSeed).GetComponent<RectTransform>(), new Vector2(0.72f, 0.04f), new Vector2(0.96f, 0.25f));

            temperature.value = 21f;
            moisture.interactable = false;
        }

        private void OnSnapshot(SimulationSnapshot s)
        {
            renderer.Render(s);
            condition.text = GetCondition(s);
            metrics.text = $"{s.Temperature:0.0}°C • Coverage {s.Coverage * 100f:0}%\nMoisture {s.AverageMoisture * 100f:0}% • Nutrients {s.AverageNutrients * 100f:0}%";
            temperatureValue.text = controller.Simulation.TargetTemperature.ToString("0.0") + "°C target";
        }

        private void OnStage(GuidedStage stage, string message)
        {
            instruction.text = message;
            moisture.interactable = stage == GuidedStage.MoistureRescue || stage == GuidedStage.Recovery;
            outcome.text = stage == GuidedStage.Complete
                ? "Discovery unlocked: A Comfortable Range"
                : stage == GuidedStage.Failed ? "Experiment ended — review the limiting factors and retry." : string.Empty;
        }

        private static string GetCondition(SimulationSnapshot s)
        {
            if (s.Temperature > 34f) return "Heat stressed";
            if (s.AverageMoisture < 0.30f) return "Too dry";
            if (s.AverageNutrients < 0.18f) return "Nutrient limited";
            if (s.AverageHealth < 0.45f) return "Declining";
            if (s.Temperature >= 24f && s.Temperature <= 29f) return "Growing well";
            return "Growing slowly";
        }

        private void CycleSpeed()
        {
            float next = controller.SimulationSpeed < 1.5f ? 2f : controller.SimulationSpeed < 3f ? 4f : 1f;
            controller.SetSpeed(next);
        }

        private Image Image(Transform parent, string name, Color color)
        {
            var image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            image.transform.SetParent(parent, false);
            image.color = color;
            return image;
        }

        private Text Text(Transform parent, string name, int size, TextAnchor anchor)
        {
            var text = new GameObject(name, typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            text.transform.SetParent(parent, false);
            text.font = font;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction action)
        {
            var image = Image(parent, label + "Button", new Color(0.18f, 0.32f, 0.26f));
            var button = image.gameObject.AddComponent<Button>();
            button.onClick.AddListener(action);
            var text = Text(image.transform, "Label", 22, TextAnchor.MiddleCenter);
            text.text = label;
            SetRect(text.rectTransform, Vector2.zero, Vector2.one);
            return button;
        }

        private Slider CreateSlider(Transform parent)
        {
            var root = new GameObject("TemperatureSlider", typeof(RectTransform), typeof(Slider));
            root.transform.SetParent(parent, false);
            var background = Image(root.transform, "Background", new Color(0.20f, 0.24f, 0.22f));
            SetRect(background.rectTransform, new Vector2(0f, 0.35f), new Vector2(1f, 0.65f));
            var fillArea = new GameObject("FillArea", typeof(RectTransform));
            fillArea.transform.SetParent(root.transform, false);
            SetRect(fillArea.GetComponent<RectTransform>(), new Vector2(0.02f, 0.35f), new Vector2(0.98f, 0.65f));
            var fill = Image(fillArea.transform, "Fill", new Color(0.55f, 0.82f, 0.64f));
            SetRect(fill.rectTransform, Vector2.zero, Vector2.one);
            var handleArea = new GameObject("HandleArea", typeof(RectTransform));
            handleArea.transform.SetParent(root.transform, false);
            SetRect(handleArea.GetComponent<RectTransform>(), new Vector2(0.02f, 0f), new Vector2(0.98f, 1f));
            var handle = Image(handleArea.transform, "Handle", Color.white);
            handle.rectTransform.sizeDelta = new Vector2(40f, 40f);
            var slider = root.GetComponent<Slider>();
            slider.minValue = 8f;
            slider.maxValue = 42f;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            return slider;
        }

        private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
