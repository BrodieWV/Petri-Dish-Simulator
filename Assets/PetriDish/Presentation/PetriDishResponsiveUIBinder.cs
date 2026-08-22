using System;
using PetriDish.Application;
using PetriDish.Content;
using PetriDish.Presentation.UI;
using PetriDish.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace PetriDish.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PetriDishResponsiveUIBinder : MonoBehaviour
    {
        [Header("Experiment setup")]
        [SerializeField] private Button organismButton;
        [SerializeField] private Button mediumButton;
        [SerializeField] private Button temperatureButton;
        [SerializeField] private Button moistureButton;
        [Header("Interventions and playback")]
        [SerializeField] private Button addMoistureButton;
        [SerializeField] private Button addNutrientsButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button speedButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button newSeedButton;
        [Header("Live values")]
        [SerializeField] private Text experimentName;
        [SerializeField] private Text conditionLabel;
        [SerializeField] private Text temperatureValue;
        [SerializeField] private Text coverageValue;
        [SerializeField] private Text moistureValue;
        [SerializeField] private Text nutrientsValue;
        [SerializeField] private Text simulationState;
        [SerializeField] private Text inspectionText;
        [Header("3D viewport bridge")]
        [SerializeField] private RectTransform dishRenderTarget;
        [SerializeField] private Text viewportHint;
        [SerializeField] private DishRenderer dishRenderer;

        private ExperimentController controller;
        private SimulationSnapshot snapshot;
        private bool hasSnapshot;
        private bool initialized;
        private bool listenersBound;
        private int selectedX = -1;
        private int selectedY = -1;
        private string feedback;
        private Button hubReturnButton;

        public DishRenderer ColonyTextureSource => dishRenderer;
        public bool IsInitialized => initialized;

        public DishRenderer Initialize(ExperimentController value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!AutoAssignReferences())
            {
                Debug.LogError("Responsive UI references could not be resolved.", this);
                return null;
            }
            if (initialized && controller == value) return dishRenderer;
            Unbind();
            controller = value;
            EnsureDishRenderer();
            ConfigureViewport();
            EnsureHubReturnButton();
            BindListeners();
            controller.SnapshotUpdated += OnSnapshot;
            controller.StageChanged += OnStageChanged;
            dishRenderer.DishTapped += OnDishTapped;
            initialized = true;
            OnSnapshot(controller.Simulation.CreateSnapshot());
            return dishRenderer;
        }

        private void OnDestroy() => Unbind();

        public void PresentNewExperimentSetup()
        {
            if (!CanInteract()) return;
            Transform setupPanel = FindNamedTransform("SetupPanel");
            ResponsivePetriDishLayout layout = GetComponentInChildren<ResponsivePetriDishLayout>(true);
            if (layout != null && layout.IsCompact && setupPanel != null && !setupPanel.gameObject.activeSelf)
                layout.ToggleLeftDrawer();
            feedback = "Choose an organism and medium in Experiment Setup to start a new experiment.";
            RefreshAll();
        }

        public void PresentOpenSelectedDish(string dishId)
        {
            if (!CanInteract()) return;
            feedback = string.IsNullOrWhiteSpace(dishId)
                ? "Current experiment opened."
                : "Dish " + dishId + " opened without restarting the experiment.";
            RefreshAll();
        }

        public void ReturnToLaboratoryHub()
        {
            new UnityLaboratoryHubNavigator().Navigate(
                LaboratoryHubAction.Lab,
                new LaboratoryHubNavigationContext(null, 0));
        }

        [ContextMenu("Auto Assign Responsive UI References")]
        public bool AutoAssignReferences()
        {
            organismButton = Resolve(organismButton, "OrganismButton");
            mediumButton = Resolve(mediumButton, "MediumButton");
            temperatureButton = Resolve(temperatureButton, "TemperatureButton");
            moistureButton = Resolve(moistureButton, "MoistureButton");
            addMoistureButton = Resolve(addMoistureButton, "AddMoistureButton");
            addNutrientsButton = Resolve(addNutrientsButton, "AddNutrientsButton");
            pauseButton = Resolve(pauseButton, "PauseButton");
            speedButton = Resolve(speedButton, "SpeedButton");
            saveButton = Resolve(saveButton, "SaveButton");
            loadButton = Resolve(loadButton, "LoadButton");
            restartButton = Resolve(restartButton, "RestartButton");
            newSeedButton = Resolve(newSeedButton, "NewSeedButton");
            experimentName = Resolve(experimentName, "ExperimentName");
            conditionLabel = Resolve(conditionLabel, "ConditionLabel");
            temperatureValue = ResolveMetric(temperatureValue, "TemperatureMetric");
            coverageValue = ResolveMetric(coverageValue, "CoverageMetric");
            moistureValue = ResolveMetric(moistureValue, "MoistureMetric");
            nutrientsValue = ResolveMetric(nutrientsValue, "NutrientsMetric");
            simulationState = Resolve(simulationState, "SimulationState");
            inspectionText = Resolve(inspectionText, "InspectionText");
            viewportHint = Resolve(viewportHint, "ViewportHint");
            dishRenderTarget = Resolve(dishRenderTarget, "DishRenderTarget");
            return organismButton && mediumButton && temperatureButton && moistureButton &&
                   addMoistureButton && addNutrientsButton && pauseButton && speedButton &&
                   saveButton && loadButton && restartButton && newSeedButton &&
                   experimentName && conditionLabel && temperatureValue && coverageValue &&
                   moistureValue && nutrientsValue && simulationState && inspectionText &&
                   dishRenderTarget;
        }

        public void SelectNextOrganism()
        {
            if (!CanInteract()) return;
            ExperimentSetupSelection selection = CurrentSelection();
            selection.SelectNextOrganism();
            feedback = "Starting " + selection.Organism.DisplayName + ".";
            StartSelected(selection);
        }

        public void SelectNextMedium()
        {
            if (!CanInteract()) return;
            ExperimentSetupSelection selection = CurrentSelection();
            selection.SelectNextMedium();
            feedback = "Selecting " + selection.Medium.DisplayName + ".";
            StartSelected(selection);
        }

        private void StartSelected(ExperimentSetupSelection selection)
        {
            controller.StartNew(
                controller.Simulation.Seed,
                selection.Organism.Id,
                selection.Medium.Id);
            selectedX = selectedY = -1;
            RefreshAll();
        }

        public void StepTemperature()
        {
            if (!CanInteract()) return;
            float next = controller.Simulation.TargetTemperature + 1f;
            if (next > 42f) next = 8f;
            controller.SetTemperature(next);
            feedback = string.Format("Target temperature set to {0:0.0}?C.", next);
            RefreshAll();
        }

        public void AddMoisture()
        {
            if (!CanInteract()) return;
            controller.AddMoisture();
            feedback = "Moisture added. The agar is rehydrating.";
            RefreshAll();
        }

        public void AddNutrients()
        {
            if (!CanInteract()) return;
            controller.TryRequestNutrientDose(out feedback);
            RefreshAll();
        }

        public void TogglePause()
        {
            if (!CanInteract()) return;
            controller.TogglePause();
            RefreshAll();
        }

        public void CycleSpeed()
        {
            if (!CanInteract()) return;
            controller.SetSpeed(SimulationSpeedCycle.Next(controller.SimulationSpeed));
            RefreshAll();
        }

        public void SaveExperiment()
        {
            if (!CanInteract()) return;
            feedback = controller.Save() ? "Experiment saved." : controller.LastPersistenceError;
            RefreshAll();
        }

        public void LoadExperiment()
        {
            if (!CanInteract()) return;
            feedback = controller.Load() ? "Experiment loaded." : controller.LastPersistenceError;
            selectedX = selectedY = -1;
            RefreshAll();
        }

        public void RestartSameSeed()
        {
            if (!CanInteract()) return;
            controller.RestartSameSeed();
            selectedX = selectedY = -1;
            feedback = "Experiment restarted with the same seed.";
            RefreshAll();
        }

        public void RestartNewSeed()
        {
            if (!CanInteract()) return;
            controller.RestartNewSeed();
            selectedX = selectedY = -1;
            feedback = "New seeded experiment started.";
            RefreshAll();
        }

        public static bool IsNutrientDoseAvailable(int doses, bool pending, long cooldown)
        {
            return doses > 0 && !pending && cooldown <= 0;
        }

        public static string FormatNutrientButtonLabel(
            int doses,
            bool pending,
            int released,
            int releaseCount,
            long cooldown)
        {
            if (pending)
                return string.Format(
                    "Add nutrients ? delivering {0}/{1} ? {2} left",
                    released,
                    releaseCount,
                    doses);
            if (cooldown > 0)
                return string.Format(
                    "Add nutrients ? ready in {0:0.##}s ? {1} left",
                    cooldown * PetriSimulation.FixedStepSeconds,
                    doses);
            return doses > 0
                ? string.Format("Add nutrients ({0})", doses)
                : "Add nutrients ? none left";
        }

        public static string FormatSimulationState(long tick, bool paused, float speed)
        {
            TimeSpan elapsed = TimeSpan.FromSeconds(tick * PetriSimulation.FixedStepSeconds);
            string clock = elapsed.TotalHours >= 1d
                ? string.Format(
                    "{0:00}:{1:00}:{2:00}",
                    (int)elapsed.TotalHours,
                    elapsed.Minutes,
                    elapsed.Seconds)
                : string.Format("{0:00}:{1:00}", elapsed.Minutes, elapsed.Seconds);
            return string.Format(
                "T+{0} ? {1} ? {2:0.#}?",
                clock,
                paused ? "SIMULATION PAUSED" : "SIMULATION RUNNING",
                speed);
        }

        private void OnSnapshot(SimulationSnapshot value)
        {
            snapshot = value;
            hasSnapshot = true;
            dishRenderer.Render(value);
            RefreshAll();
        }

        private void OnStageChanged(GuidedStage stage, string message)
        {
            feedback = message;
            RefreshAll();
        }

        private void OnDishTapped(Vector2 point)
        {
            CellInspection inspection;
            if (!hasSnapshot ||
                !DishInspection.TryInspect(snapshot, point.x, point.y, out inspection))
                return;
            selectedX = inspection.X;
            selectedY = inspection.Y;
            inspectionText.text = inspection.ToDisplayText();
        }

        private void RefreshAll()
        {
            if (!CanInteract()) return;
            PetriSimulation simulation = controller.Simulation;
            OrganismDefinition organism =
                controller.DefinitionCatalog.ResolveOrganism(simulation.OrganismId);
            MediumDefinition medium =
                controller.DefinitionCatalog.ResolveMedium(simulation.MediumId);
            Label(organismButton, organism.DisplayName);
            Label(mediumButton, medium.DisplayName);
            Label(
                temperatureButton,
                string.Format("Temperature {0:0.0}?C (+1?)", simulation.TargetTemperature));
            Label(pauseButton, AccessibilityPresentation.PauseButtonLabel(controller.Paused));
            Label(speedButton, SimulationSpeedCycle.Label(controller.SimulationSpeed));
            Label(
                addNutrientsButton,
                FormatNutrientButtonLabel(
                    controller.NutrientDosesRemaining,
                    controller.NutrientDeliveryPending,
                    controller.NutrientReleaseStepsCompleted,
                    controller.NutrientReleaseStepCount,
                    controller.NutrientCooldownRemainingSteps));
            addNutrientsButton.interactable = IsNutrientDoseAvailable(
                controller.NutrientDosesRemaining,
                controller.NutrientDeliveryPending,
                controller.NutrientCooldownRemainingSteps);
            experimentName.text = string.Format(
                "RUN SEED: {0} ? SAMPLE: {1} ? MEDIUM: {2}",
                simulation.Seed,
                organism.DisplayName,
                medium.DisplayName);
            simulationState.text = FormatSimulationState(
                simulation.Tick,
                controller.Paused,
                controller.SimulationSpeed);
            if (!hasSnapshot) return;
            temperatureValue.text = string.Format("{0:0.0}?C", snapshot.Temperature);
            coverageValue.text = string.Format("{0:0}%", snapshot.Coverage * 100f);
            moistureValue.text = string.Format("{0:0}%", snapshot.AverageMoisture * 100f);
            nutrientsValue.text = string.Format(
                "{0:0}% ? {1} doses ? {2} recorded",
                snapshot.AverageNutrients * 100f,
                controller.NutrientDosesRemaining,
                controller.NutrientHistory.Count);
            Label(moistureButton, string.Format("Moisture {0:0}%", snapshot.AverageMoisture * 100f));
            conditionLabel.text =
                "STATUS: " +
                AccessibilityPresentation.ConditionLabel(
                    AccessibilityPresentation.GetCondition(snapshot));
            RefreshInspection();
        }

        private void RefreshInspection()
        {
            CellInspection inspection;
            if (selectedX >= 0 && selectedY >= 0 &&
                DishInspection.TryInspect(snapshot, selectedX, selectedY, out inspection))
            {
                inspectionText.text = inspection.ToDisplayText();
                return;
            }
            inspectionText.text = string.IsNullOrWhiteSpace(feedback)
                ? "SELECT A COLONY TO INSPECT LOCAL GROWTH CONDITIONS"
                : feedback;
        }

        private ExperimentSetupSelection CurrentSelection()
        {
            return new ExperimentSetupSelection(
                controller.DefinitionCatalog,
                controller.Simulation.OrganismId,
                controller.Simulation.MediumId);
        }

        private bool CanInteract() =>
            initialized && controller != null && controller.Simulation != null;

        private void BindListeners()
        {
            if (listenersBound) return;
            organismButton.onClick.AddListener(SelectNextOrganism);
            mediumButton.onClick.AddListener(SelectNextMedium);
            temperatureButton.onClick.AddListener(StepTemperature);
            moistureButton.onClick.AddListener(AddMoisture);
            addMoistureButton.onClick.AddListener(AddMoisture);
            addNutrientsButton.onClick.AddListener(AddNutrients);
            pauseButton.onClick.AddListener(TogglePause);
            speedButton.onClick.AddListener(CycleSpeed);
            saveButton.onClick.AddListener(SaveExperiment);
            loadButton.onClick.AddListener(LoadExperiment);
            restartButton.onClick.AddListener(RestartSameSeed);
            newSeedButton.onClick.AddListener(RestartNewSeed);
            if (hubReturnButton != null)
                hubReturnButton.onClick.AddListener(ReturnToLaboratoryHub);
            listenersBound = true;
        }

        private void Unbind()
        {
            if (controller != null)
            {
                controller.SnapshotUpdated -= OnSnapshot;
                controller.StageChanged -= OnStageChanged;
            }
            if (dishRenderer != null) dishRenderer.DishTapped -= OnDishTapped;
            if (listenersBound)
            {
                organismButton.onClick.RemoveListener(SelectNextOrganism);
                mediumButton.onClick.RemoveListener(SelectNextMedium);
                temperatureButton.onClick.RemoveListener(StepTemperature);
                moistureButton.onClick.RemoveListener(AddMoisture);
                addMoistureButton.onClick.RemoveListener(AddMoisture);
                addNutrientsButton.onClick.RemoveListener(AddNutrients);
                pauseButton.onClick.RemoveListener(TogglePause);
                speedButton.onClick.RemoveListener(CycleSpeed);
                saveButton.onClick.RemoveListener(SaveExperiment);
                loadButton.onClick.RemoveListener(LoadExperiment);
                restartButton.onClick.RemoveListener(RestartSameSeed);
                newSeedButton.onClick.RemoveListener(RestartNewSeed);
                if (hubReturnButton != null)
                    hubReturnButton.onClick.RemoveListener(ReturnToLaboratoryHub);
            }
            listenersBound = false;
            initialized = false;
            controller = null;
        }

        private void EnsureHubReturnButton()
        {
            if (hubReturnButton != null) return;
            Transform existing = FindNamedTransform("LabHubButton") ?? FindNamedTransform("SettingsButton");
            if (existing == null) return;
            existing.name = "LabHubButton";
            hubReturnButton = existing.GetComponent<Button>();
            if (hubReturnButton != null)
                Label(hubReturnButton, "Lab");
        }

        private void EnsureDishRenderer()
        {
            if (dishRenderer != null) return;
            Transform surface = dishRenderTarget.Find("DishInteractionSurface");
            if (surface != null) dishRenderer = surface.GetComponent<DishRenderer>();
            if (dishRenderer != null) return;
            GameObject created = new GameObject(
                "DishInteractionSurface",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(RawImage),
                typeof(AspectRatioFitter),
                typeof(DishRenderer));
            created.transform.SetParent(dishRenderTarget, false);
            RectTransform rect = created.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            AspectRatioFitter fitter = created.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 1f;
            dishRenderer = created.GetComponent<DishRenderer>();
        }

        private void ConfigureViewport()
        {
            if (viewportHint != null) viewportHint.gameObject.SetActive(false);
            Transform backgroundTransform = FindNamedTransform("Background");
            Image rootBackground =
                backgroundTransform != null ? backgroundTransform.GetComponent<Image>() : null;
            Image targetBackground = dishRenderTarget.GetComponent<Image>();
            Image panelBackground = dishRenderTarget.parent.GetComponent<Image>();
            if (rootBackground == null || targetBackground == null || panelBackground == null)
                return;
            RectTransform rootBackdrop = Backdrop(
                "ResponsiveViewportBackdrop",
                transform,
                rootBackground.transform.GetSiblingIndex() + 1);
            Presenter(rootBackdrop).Configure(
                dishRenderTarget,
                rootBackground,
                dishRenderer,
                rootBackground.color);
            Color panelColour = panelBackground.color;
            panelColour.a = 1f;
            Color transparent = panelBackground.color;
            transparent.a = 0f;
            panelBackground.color = transparent;
            RectTransform panelBackdrop =
                Backdrop("PanelViewportBackdrop", dishRenderTarget.parent, 0);
            Presenter(panelBackdrop).Configure(
                dishRenderTarget,
                targetBackground,
                dishRenderer,
                panelColour);
        }

        private static DishViewportPresenter Presenter(RectTransform root)
        {
            DishViewportPresenter value = root.GetComponent<DishViewportPresenter>();
            return value != null ? value : root.gameObject.AddComponent<DishViewportPresenter>();
        }

        private static RectTransform Backdrop(string name, Transform parent, int index)
        {
            Transform existing = parent.Find(name);
            RectTransform rect;
            if (existing != null)
            {
                rect = existing.GetComponent<RectTransform>();
            }
            else
            {
                GameObject created = new GameObject(name, typeof(RectTransform));
                created.transform.SetParent(parent, false);
                rect = created.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            rect.SetSiblingIndex(Mathf.Clamp(index, 0, parent.childCount - 1));
            return rect;
        }

        private T Resolve<T>(T current, string objectName) where T : Component
        {
            if (current != null) return current;
            Transform target = FindNamedTransform(objectName);
            return target != null ? target.GetComponent<T>() : null;
        }

        private Text ResolveMetric(Text current, string cardName)
        {
            if (current != null) return current;
            Transform card = FindNamedTransform(cardName);
            Transform value = card != null ? card.Find("Value") : null;
            return value != null ? value.GetComponent<Text>() : null;
        }

        private Transform FindNamedTransform(string objectName)
        {
            Transform[] descendants = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < descendants.Length; i++)
                if (descendants[i].name == objectName) return descendants[i];
            return null;
        }

        private static void Label(Button button, string value)
        {
            Text text = button.GetComponentInChildren<Text>(true);
            if (text != null) text.text = value;
        }
    }
}
