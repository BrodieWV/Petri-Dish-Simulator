using System.Collections.Generic;
using PetriDish.Application;
using PetriDish.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PetriDish.Presentation
{
    public sealed class RuntimeBootstrap : MonoBehaviour
    {
        private const string TextScalePreferenceKey = "PetriDish.TextScaleMode";

        private readonly Dictionary<Text, int> baseFontSizes = new Dictionary<Text, int>();
        private ExperimentController controller;
        private DishRenderer renderer;
        private Text instruction;
        private Text condition;
        private Text metrics;
        private Text temperatureValue;
        private Text outcome;
        private Text inspection;
        private Text speedLabel;
        private Text pauseLabel;
        private Text simulationState;
        private Text textScaleLabel;
        private Slider temperature;
        private Button moisture;
        private Font font;
        private SimulationSnapshot currentSnapshot;
        private TextScaleMode textScaleMode;
        private bool hasSnapshot;
        private int selectedX = -1;
        private int selectedY = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartRuntime()
        {
            if (FindAnyObjectByType<RuntimeBootstrap>() != null) return;
            var root = new GameObject("PetriDishRuntime");
            DontDestroyOnLoad(root);
            root.AddComponent<ExperimentController>();
            root.AddComponent<RuntimeBootstrap>();
        }

        private void Awake()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            controller = GetComponent<ExperimentController>();
            textScaleMode = TextScalePolicy.FromStoredValue(PlayerPrefs.GetInt(TextScalePreferenceKey, 0));
            CreateEventSystem();
            BuildUI();
            controller.SnapshotUpdated += OnSnapshot;
            controller.StageChanged += OnStage;
            renderer.DishTapped += OnDishTapped;
            RefreshPlaybackState();
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.SnapshotUpdated -= OnSnapshot;
                controller.StageChanged -= OnStage;
            }
            if (renderer != null) renderer.DishTapped -= OnDishTapped;
        }

        private void CreateEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;
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
            SetRect(instruction.rectTransform, new Vector2(0.04f, 0.89f), new Vector2(0.76f, 0.98f));
            instruction.text = "The Comfortable Range";

            var textScale = CreateButton(bg.transform, TextScalePolicy.ButtonLabel(textScaleMode), CycleTextScale);
            SetRect(textScale.GetComponent<RectTransform>(), new Vector2(0.78f, 0.91f), new Vector2(0.96f, 0.97f));
            textScaleLabel = textScale.GetComponentInChildren<Text>();

            condition = Text(bg.transform, "Condition", 29, TextAnchor.MiddleLeft);
            SetRect(condition.rectTransform, new Vector2(0.05f, 0.82f), new Vector2(0.55f, 0.89f));
            metrics = Text(bg.transform, "Metrics", 24, TextAnchor.MiddleRight);
            SetRect(metrics.rectTransform, new Vector2(0.52f, 0.82f), new Vector2(0.95f, 0.89f));

            var dishPanel = Image(bg.transform, "DishPanel", new Color(0.11f, 0.15f, 0.13f));
            SetRect(dishPanel.rectTransform, new Vector2(0.08f, 0.35f), new Vector2(0.92f, 0.80f));
            var dish = new GameObject("Dish", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter), typeof(DishRenderer));
            dish.transform.SetParent(dishPanel.transform, false);
            SetRect(dish.GetComponent<RectTransform>(), new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.96f));
            dish.GetComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            dish.GetComponent<AspectRatioFitter>().aspectRatio = 1f;
            renderer = dish.GetComponent<DishRenderer>();

            var inspectionPanel = Image(dishPanel.transform, "InspectionPanel", new Color(0.03f, 0.05f, 0.04f, 0.88f));
            inspectionPanel.raycastTarget = false;
            SetRect(inspectionPanel.rectTransform, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.24f));
            inspection = Text(inspectionPanel.transform, "Inspection", 22, TextAnchor.MiddleLeft);
            inspection.raycastTarget = false;
            SetRect(inspection.rectTransform, new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.94f));
            inspection.text = "Tap the dish to inspect a local cell.";

            outcome = Text(bg.transform, "Outcome", 28, TextAnchor.MiddleCenter);
            SetRect(outcome.rectTransform, new Vector2(0.06f, 0.315f), new Vector2(0.94f, 0.35f));

            var controls = Image(bg.transform, "Controls", new Color(0.08f, 0.11f, 0.10f));
            SetRect(controls.rectTransform, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.31f));

            var tempLabel = Text(controls.transform, "TemperatureLabel", 27, TextAnchor.MiddleLeft);
            SetRect(tempLabel.rectTransform, new Vector2(0.04f, 0.76f), new Vector2(0.50f, 0.96f));
            tempLabel.text = "Temperature";
            temperatureValue = Text(controls.transform, "TemperatureValue", 27, TextAnchor.MiddleRight);
            SetRect(temperatureValue.rectTransform, new Vector2(0.50f, 0.76f), new Vector2(0.96f, 0.96f));

            temperature = CreateSlider(controls.transform);
            SetRect(temperature.GetComponent<RectTransform>(), new Vector2(0.06f, 0.59f), new Vector2(0.94f, 0.75f));
            temperature.onValueChanged.AddListener(v =>
            {
                controller.SetTemperature(v);
                temperatureValue.text = v.ToString("0.0") + "°C target";
            });

            simulationState = Text(controls.transform, "SimulationState", 20, TextAnchor.MiddleCenter);
            SetRect(simulationState.rectTransform, new Vector2(0.04f, 0.51f), new Vector2(0.96f, 0.59f));

            moisture = CreateButton(controls.transform, "Add moisture", controller.AddMoisture);
            SetRect(moisture.GetComponent<RectTransform>(), new Vector2(0.04f, 0.28f), new Vector2(0.37f, 0.50f));
            var pause = CreateButton(controls.transform, AccessibilityPresentation.PauseButtonLabel(controller.Paused), TogglePause);
            SetRect(pause.GetComponent<RectTransform>(), new Vector2(0.39f, 0.28f), new Vector2(0.72f, 0.50f));
            pauseLabel = pause.GetComponentInChildren<Text>();
            var speed = CreateButton(controls.transform, SimulationSpeedCycle.Label(controller.SimulationSpeed), CycleSpeed);
            SetRect(speed.GetComponent<RectTransform>(), new Vector2(0.74f, 0.28f), new Vector2(0.96f, 0.50f));
            speedLabel = speed.GetComponentInChildren<Text>();

            SetRect(CreateButton(controls.transform, "Save", Save).GetComponent<RectTransform>(), new Vector2(0.04f, 0.03f), new Vector2(0.24f, 0.24f));
            SetRect(CreateButton(controls.transform, "Load", Load).GetComponent<RectTransform>(), new Vector2(0.26f, 0.03f), new Vector2(0.46f, 0.24f));
            SetRect(CreateButton(controls.transform, "Restart", RestartSameSeed).GetComponent<RectTransform>(), new Vector2(0.48f, 0.03f), new Vector2(0.70f, 0.24f));
            SetRect(CreateButton(controls.transform, "New seed", RestartNewSeed).GetComponent<RectTransform>(), new Vector2(0.72f, 0.03f), new Vector2(0.96f, 0.24f));

            temperature.SetValueWithoutNotify(controller.Simulation.TargetTemperature);
            moisture.interactable = true;
            ApplyTextScale();
        }

        private void OnSnapshot(SimulationSnapshot snapshot)
        {
            currentSnapshot = snapshot;
            hasSnapshot = true;
            renderer.Render(snapshot);
            SimulationCondition status = AccessibilityPresentation.GetCondition(snapshot);
            condition.text = AccessibilityPresentation.ConditionLabel(status);
            metrics.text = $"{snapshot.Temperature:0.0}°C • Coverage {snapshot.Coverage * 100f:0}%\nMoisture {snapshot.AverageMoisture * 100f:0}% • Nutrients {snapshot.AverageNutrients * 100f:0}%";
            temperature.SetValueWithoutNotify(controller.Simulation.TargetTemperature);
            temperatureValue.text = controller.Simulation.TargetTemperature.ToString("0.0") + "°C target";
            RefreshInspection();
            RefreshPlaybackState();
        }

        private void OnDishTapped(Vector2 normalizedPoint)
        {
            if (!hasSnapshot) return;
            if (!DishInspection.TryInspect(currentSnapshot, normalizedPoint.x, normalizedPoint.y, out CellInspection cell))
                return;

            selectedX = cell.X;
            selectedY = cell.Y;
            inspection.text = cell.ToDisplayText();
        }

        private void RefreshInspection()
        {
            if (!hasSnapshot || selectedX < 0 || selectedY < 0) return;
            if (DishInspection.TryInspect(currentSnapshot, selectedX, selectedY, out CellInspection cell))
                inspection.text = cell.ToDisplayText();
        }

        private void OnStage(GuidedStage stage, string message)
        {
            instruction.text = message;
            outcome.text = stage == GuidedStage.Complete
                ? "Discovery unlocked: A Comfortable Range"
                : stage == GuidedStage.Failed ? "Experiment ended — review the limiting factors and retry." : string.Empty;
        }

        private void TogglePause()
        {
            controller.TogglePause();
            RefreshPlaybackState();
        }

        private void CycleSpeed()
        {
            float next = SimulationSpeedCycle.Next(controller.SimulationSpeed);
            controller.SetSpeed(next);
            speedLabel.text = SimulationSpeedCycle.Label(next);
            RefreshPlaybackState();
        }

        private void CycleTextScale()
        {
            textScaleMode = TextScalePolicy.Next(textScaleMode);
            PlayerPrefs.SetInt(TextScalePreferenceKey, (int)textScaleMode);
            PlayerPrefs.Save();
            textScaleLabel.text = TextScalePolicy.ButtonLabel(textScaleMode);
            ApplyTextScale();
        }

        private void ApplyTextScale()
        {
            foreach (KeyValuePair<Text, int> entry in baseFontSizes)
            {
                if (entry.Key != null)
                    entry.Key.fontSize = TextScalePolicy.ScaleFontSize(entry.Value, textScaleMode);
            }
        }

        private void Save()
        {
            outcome.text = controller.Save()
                ? "Experiment saved."
                : controller.LastPersistenceError;
        }

        private void Load()
        {
            if (controller.Load())
            {
                ResetInspection();
                speedLabel.text = SimulationSpeedCycle.Label(controller.SimulationSpeed);
            }
            else
                outcome.text = controller.LastPersistenceError;
            RefreshPlaybackState();
        }

        private void RestartSameSeed()
        {
            controller.RestartSameSeed();
            ResetInspection();
            speedLabel.text = SimulationSpeedCycle.Label(controller.SimulationSpeed);
            RefreshPlaybackState();
        }

        private void RestartNewSeed()
        {
            controller.RestartNewSeed();
            ResetInspection();
            speedLabel.text = SimulationSpeedCycle.Label(controller.SimulationSpeed);
            RefreshPlaybackState();
        }

        private void ResetInspection()
        {
            selectedX = -1;
            selectedY = -1;
            inspection.text = "Tap the dish to inspect a local cell.";
        }

        private void RefreshPlaybackState()
        {
            if (pauseLabel != null)
                pauseLabel.text = AccessibilityPresentation.PauseButtonLabel(controller.Paused);
            if (simulationState != null)
                simulationState.text = AccessibilityPresentation.SimulationStateLabel(controller.Paused, controller.SimulationSpeed);
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
            text.fontSize = TextScalePolicy.ScaleFontSize(size, textScaleMode);
            text.alignment = anchor;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            baseFontSizes[text] = size;
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
