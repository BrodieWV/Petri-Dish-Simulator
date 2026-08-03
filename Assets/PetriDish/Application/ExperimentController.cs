using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using PetriDish.Content;
using PetriDish.Simulation;
using UnityEngine;

namespace PetriDish.Application
{
    [Serializable]
    public sealed class NutrientInterventionRecord
    {
        [SerializeField] private long requestTick;
        [SerializeField] private long deliveryStartTick = -1;
        [SerializeField] private long completionTick = -1;
        [SerializeField] private float requestedAmount;
        [SerializeField] private float deliveredAmount;

        public long RequestTick => requestTick;
        public long DeliveryStartTick => deliveryStartTick;
        public long CompletionTick => completionTick;
        public float RequestedAmount => requestedAmount;
        public float DeliveredAmount => deliveredAmount;
        public bool IsComplete => completionTick >= 0;

        internal NutrientInterventionRecord(long requestedAtTick, float amount)
        {
            requestTick = requestedAtTick;
            requestedAmount = amount;
        }

        internal NutrientInterventionRecord Clone()
        {
            return new NutrientInterventionRecord(requestTick, requestedAmount)
            {
                deliveryStartTick = deliveryStartTick,
                completionTick = completionTick,
                deliveredAmount = deliveredAmount
            };
        }

        internal void RecordDelivery(long tick, float amount)
        {
            if (deliveryStartTick < 0) deliveryStartTick = tick;
            deliveredAmount += amount;
        }

        internal void Complete(long tick) => completionTick = tick;
    }

    public enum GuidedStage
    {
        ObserveCoolStart,
        WarmToIdeal,
        HoldIdeal,
        HeaterFault,
        MoistureRescue,
        Recovery,
        Complete,
        Failed
    }

    public sealed class ExperimentController : MonoBehaviour
    {
        public const int TutorialSeed = 260726;
        public const int CurrentExperimentSaveSchemaVersion = 4;
        private const long MaxSaveFileBytes = 4L * 1024L * 1024L;
        private const float MaxPendingSimulationSeconds = 3600f;

        public event Action<SimulationSnapshot> SnapshotUpdated;
        public event Action<GuidedStage, string> StageChanged;

        [SerializeField] private float simulationSpeed = 1f;
        [SerializeField, Min(1)] private int maxSimulationStepsPerFrame = 64;
        [SerializeField] private SimulationDefinitionCatalog definitionCatalog;
        [SerializeField] private string selectedOrganismId;
        [SerializeField] private string selectedMediumId;
        [Header("Nutrient intervention")]
        [SerializeField, Range(0.001f, 1f)] private float nutrientDoseAmount = 0.12f;
        [SerializeField, Min(1)] private int nutrientDoseSupply = 3;
        [SerializeField, Min(0)] private int nutrientCooldownSteps = 20;
        [SerializeField, Min(1)] private int nutrientDelaySteps = 4;
        [SerializeField, Min(1)] private int nutrientReleaseSteps = 12;
        private PetriSimulation simulation;
        private float accumulator;
        private bool paused;
        private GuidedStage stage;
        private float stageStartSeconds;
        private bool moistureAddedDuringRescue;
        private readonly List<NutrientInterventionRecord> nutrientHistory =
            new List<NutrientInterventionRecord>();
        private int nutrientDosesRemaining;
        private long nutrientCooldownEndTick;
        private PendingNutrientDelivery pendingNutrientDelivery;
        private string savePath;

        public PetriSimulation Simulation => simulation;
        public GuidedStage Stage => stage;
        public bool Paused => paused;
        public float SimulationSpeed => simulationSpeed;
        public SimulationDefinitionCatalog DefinitionCatalog => definitionCatalog;
        public string LastPersistenceError { get; private set; }
        public int NutrientDosesRemaining => nutrientDosesRemaining;
        public long NutrientCooldownRemainingSteps => simulation == null
            ? 0
            : Math.Max(0, nutrientCooldownEndTick - simulation.Tick);
        public bool NutrientDeliveryPending => pendingNutrientDelivery != null;
        public int NutrientReleaseStepsCompleted => pendingNutrientDelivery?.releaseStepsCompleted ?? 0;
        public int NutrientReleaseStepCount => nutrientReleaseSteps;
        public IReadOnlyList<NutrientInterventionRecord> NutrientHistory => nutrientHistory;
        public string NutrientFeedback { get; private set; }

        private void Awake()
        {
            ValidateNutrientConfiguration();
            savePath = Path.Combine(UnityEngine.Application.persistentDataPath, "petri_vertical_slice.json");
            EnsureDefinitionSelection();
            ResetExperiment(TutorialSeed, false);
        }

        private void Start()
        {
            if (simulation == null) ResetExperiment(TutorialSeed, false);
            PublishStage();
            PublishSnapshot();
        }

        private void Update()
        {
            AdvanceSimulation(Time.unscaledDeltaTime);
        }

        public bool AdvanceSimulation(float unscaledDeltaTime)
        {
            if (!IsFinite(unscaledDeltaTime) || unscaledDeltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(unscaledDeltaTime),
                    "Elapsed time must be finite and non-negative.");
            if (paused || simulation == null || unscaledDeltaTime == 0f) return false;

            float scaledDelta = unscaledDeltaTime * simulationSpeed;
            if (!IsFinite(scaledDelta))
                throw new ArgumentOutOfRangeException(nameof(unscaledDeltaTime),
                    "Scaled elapsed time is outside the supported range.");

            accumulator += scaledDelta;
            int steps = 0;
            int stepLimit = Mathf.Max(1, maxSimulationStepsPerFrame);
            while (accumulator >= PetriSimulation.FixedStepSeconds && steps < stepLimit)
            {
                accumulator -= PetriSimulation.FixedStepSeconds;
                simulation.Step();
                ProcessNutrientDelivery();
                UpdateGuidedFlow();
                steps++;
            }

            if (steps > 0) PublishSnapshot();
            return steps > 0;
        }

        public void StartNew(int seed)
        {
            ResetExperiment(seed, true);
        }

        public void StartNew(int seed, string organismId, string mediumId)
        {
            EnsureDefinitionCatalog();
            definitionCatalog.ResolveOrganism(organismId);
            definitionCatalog.ResolveMedium(mediumId);
            selectedOrganismId = organismId;
            selectedMediumId = mediumId;
            ResetExperiment(seed, true);
        }

        public void ConfigureDefinitionCatalog(SimulationDefinitionCatalog catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            catalog.ValidateOrThrow();
            definitionCatalog = catalog;
            selectedOrganismId = catalog.DefaultOrganism.Id;
            selectedMediumId = catalog.DefaultMedium.Id;
        }

        private void ResetExperiment(int seed, bool notify)
        {
            EnsureDefinitionSelection();
            simulation = new PetriSimulation(
                seed,
                definitionCatalog.ResolveOrganism(selectedOrganismId),
                definitionCatalog.ResolveMedium(selectedMediumId));
            accumulator = 0f;
            paused = false;
            simulationSpeed = 1f;
            moistureAddedDuringRescue = false;
            nutrientHistory.Clear();
            nutrientDosesRemaining = nutrientDoseSupply;
            nutrientCooldownEndTick = 0;
            pendingNutrientDelivery = null;
            NutrientFeedback = "Nutrient doses are ready.";
            SetStage(GuidedStage.ObserveCoolStart,
                "Observe the slow colony. The dish is cooler than this culture prefers.", notify);
            if (notify) PublishSnapshot();
        }

        public void SetTemperature(float value)
        {
            simulation.SetTargetTemperature(value);
        }

        public void AddMoisture()
        {
            simulation.AddMoisture(0.16f);
            if (stage == GuidedStage.MoistureRescue) moistureAddedDuringRescue = true;
            PublishSnapshot();
        }

        public bool TryRequestNutrientDose(out string feedback)
        {
            if (simulation == null)
            {
                feedback = "The experiment has not started.";
                NutrientFeedback = feedback;
                return false;
            }
            if (nutrientDosesRemaining <= 0)
            {
                feedback = "No nutrient doses remain in this experiment.";
                NutrientFeedback = feedback;
                return false;
            }
            if (pendingNutrientDelivery != null)
            {
                feedback = "The current nutrient dose is still being delivered.";
                NutrientFeedback = feedback;
                return false;
            }
            if (NutrientCooldownRemainingSteps > 0)
            {
                float seconds = NutrientCooldownRemainingSteps * PetriSimulation.FixedStepSeconds;
                feedback = $"Nutrient dose ready in {seconds:0.##} simulated seconds.";
                NutrientFeedback = feedback;
                return false;
            }

            long requestTick = simulation.Tick;
            var record = new NutrientInterventionRecord(requestTick, nutrientDoseAmount);
            nutrientHistory.Add(record);
            pendingNutrientDelivery = new PendingNutrientDelivery
            {
                historyIndex = nutrientHistory.Count - 1,
                requestTick = requestTick,
                scheduledStartTick = requestTick + nutrientDelaySteps,
                requestedAmount = nutrientDoseAmount
            };
            nutrientDosesRemaining--;
            nutrientCooldownEndTick = requestTick + nutrientCooldownSteps;
            feedback = "Nutrient dose scheduled; delivery begins after the fixed delay.";
            NutrientFeedback = feedback;
            PublishSnapshot();
            return true;
        }

        private void ProcessNutrientDelivery()
        {
            if (pendingNutrientDelivery == null ||
                simulation.Tick < pendingNutrientDelivery.scheduledStartTick)
                return;

            float stepAmount =
                pendingNutrientDelivery.requestedAmount / nutrientReleaseSteps;
            float delivered = simulation.AddNutrients(stepAmount);
            NutrientInterventionRecord record =
                nutrientHistory[pendingNutrientDelivery.historyIndex];
            record.RecordDelivery(simulation.Tick, delivered);
            pendingNutrientDelivery.deliveredAmount += delivered;
            pendingNutrientDelivery.releaseStepsCompleted++;
            if (delivered + 0.000001f < stepAmount)
                pendingNutrientDelivery.capacityLimited = true;

            if (pendingNutrientDelivery.releaseStepsCompleted < nutrientReleaseSteps)
            {
                NutrientFeedback = pendingNutrientDelivery.capacityLimited
                    ? "Nutrient delivery is active; agar capacity is limiting absorption."
                    : "Nutrient delivery is active.";
                return;
            }

            record.Complete(simulation.Tick);
            NutrientFeedback = pendingNutrientDelivery.capacityLimited
                ? $"Dose complete: agar capacity limited absorption to {record.DeliveredAmount:0.###} of {record.RequestedAmount:0.###}."
                : $"Dose complete: {record.DeliveredAmount:0.###} nutrient units absorbed.";
            pendingNutrientDelivery = null;
        }

        public void SetSpeed(float value)
        {
            if (!IsFinite(value))
                throw new ArgumentOutOfRangeException(nameof(value), "Simulation speed must be finite.");
            simulationSpeed = Mathf.Clamp(value, 0.5f, 8f);
        }

        public void TogglePause() => paused = !paused;

        public void RestartSameSeed() => StartNew(simulation != null ? simulation.Seed : TutorialSeed);

        public void RestartNewSeed() => StartNew(UnityEngine.Random.Range(1, int.MaxValue));

        public bool Save()
        {
            return SaveToPath(savePath);
        }

        public bool SaveToPath(string path)
        {
            LastPersistenceError = null;
            try
            {
                if (simulation == null)
                    throw new InvalidOperationException("The experiment has not been initialized.");
                string fullPath = ValidatePersistencePath(path);
                var wrapper = new ExperimentSave
                {
                    schemaVersion = CurrentExperimentSaveSchemaVersion,
                    simulation = simulation.CaptureSave(),
                    stage = stage,
                    stageStartSeconds = stageStartSeconds,
                    moistureAddedDuringRescue = moistureAddedDuringRescue,
                    accumulator = accumulator,
                    paused = paused,
                    simulationSpeed = simulationSpeed,
                    nutrientDosesRemaining = nutrientDosesRemaining,
                    nutrientCooldownEndTick = nutrientCooldownEndTick,
                    pendingNutrientDelivery = pendingNutrientDelivery?.Clone(),
                    nutrientHistory = CopyNutrientHistory()
                };
                string json = JsonUtility.ToJson(wrapper, true);
                WriteSaveAtomically(fullPath, json);
                return true;
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LastPersistenceError = "The experiment could not be saved safely.";
                Debug.LogWarning($"Petri Dish save failed: {exception.Message}");
                return false;
            }
        }

        public bool Load()
        {
            return LoadFromPath(savePath);
        }

        public bool LoadFromPath(string path)
        {
            LastPersistenceError = null;
            string fullPath;
            try
            {
                fullPath = ValidatePersistencePath(path);
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LastPersistenceError = "The save location is invalid.";
                Debug.LogWarning($"Petri Dish load failed: {exception.Message}");
                return false;
            }

            if (TryLoadCandidate(fullPath, out string primaryError)) return true;

            string backupPath = fullPath + ".bak";
            string backupError = null;
            if (File.Exists(backupPath) && TryLoadCandidate(backupPath, out backupError))
            {
                Debug.LogWarning($"Petri Dish restored the previous backup because the main save was invalid: {primaryError}");
                return true;
            }

            LastPersistenceError = File.Exists(fullPath)
                ? $"The saved experiment could not be loaded: {primaryError}"
                : "No saved experiment was found.";
            if (!string.IsNullOrEmpty(primaryError))
                Debug.LogWarning($"Petri Dish load failed: {primaryError}");
            if (!string.IsNullOrEmpty(backupError))
                Debug.LogWarning($"Petri Dish backup load failed: {backupError}");
            return false;
        }

        private void UpdateGuidedFlow()
        {
            float elapsed = simulation.ElapsedSimSeconds - stageStartSeconds;

            switch (stage)
            {
                case GuidedStage.ObserveCoolStart:
                    if (elapsed >= 8f)
                        SetStage(GuidedStage.WarmToIdeal, "Raise the temperature into the comfortable range: 24–29°C.");
                    break;
                case GuidedStage.WarmToIdeal:
                    if (simulation.TargetTemperature >= 24f && simulation.TargetTemperature <= 29f)
                        SetStage(GuidedStage.HoldIdeal, "Good. Hold the culture in range and watch the active edge expand.");
                    break;
                case GuidedStage.HoldIdeal:
                    {
                        SimulationMetrics metrics = simulation.CreateMetrics();
                        if (elapsed >= 18f && metrics.Coverage >= 0.035f)
                        {
                            simulation.SetTargetTemperature(36f);
                            SetStage(GuidedStage.HeaterFault, "Heater fault: the dish is overheating. Bring it below 30°C.");
                        }
                        break;
                    }
                case GuidedStage.HeaterFault:
                    {
                        if (simulation.TargetTemperature < 30f)
                        {
                            SetStage(GuidedStage.MoistureRescue, "Heat increased water loss. Add moisture before the edge dries further.");
                        }
                        else
                        {
                            SimulationMetrics metrics = simulation.CreateMetrics();
                            if (elapsed > 30f && metrics.AverageHealth < 0.45f)
                                SetStage(GuidedStage.Failed, "The culture remained overheated too long. Review the cause or restart from the experiment.");
                        }
                        break;
                    }
                case GuidedStage.MoistureRescue:
                    if (moistureAddedDuringRescue)
                        SetStage(GuidedStage.Recovery, "Conditions are improving. Recovery takes time; keep the temperature stable.");
                    break;
                case GuidedStage.Recovery:
                    {
                        SimulationMetrics metrics = simulation.CreateMetrics();
                        if (elapsed >= 20f && metrics.AverageHealth >= 0.72f && simulation.Temperature < 30f)
                            SetStage(GuidedStage.Complete, "Experiment complete: you found the comfortable range and recovered the culture.");
                        else if (elapsed > 45f && metrics.AverageHealth < 0.35f)
                            SetStage(GuidedStage.Failed, "The culture could not recover. Temperature and moisture were the main limiting factors.");
                        break;
                    }
            }
        }

        private void SetStage(GuidedStage next, string message, bool notify = true)
        {
            stage = next;
            stageStartSeconds = simulation != null ? simulation.ElapsedSimSeconds : 0f;
            if (notify) StageChanged?.Invoke(stage, message);
        }

        private bool TryLoadCandidate(string path, out string error)
        {
            error = null;
            if (!File.Exists(path)) return false;

            try
            {
                var file = new FileInfo(path);
                if (file.Length <= 0 || file.Length > MaxSaveFileBytes)
                    throw new InvalidDataException("Save size is outside the supported range.");

                ExperimentSave wrapper = JsonUtility.FromJson<ExperimentSave>(
                    File.ReadAllText(path, Encoding.UTF8));
                ValidateExperimentSave(wrapper);

                ResolveSavedDefinitions(
                    wrapper,
                    out OrganismDefinition organismDefinition,
                    out MediumDefinition mediumDefinition);
                var restoredSimulation = new PetriSimulation(
                    wrapper.simulation.seed,
                    organismDefinition,
                    mediumDefinition);
                restoredSimulation.Restore(wrapper.simulation);

                bool legacy = wrapper.schemaVersion == 0;
                simulation = restoredSimulation;
                selectedOrganismId = organismDefinition.Id;
                selectedMediumId = mediumDefinition.Id;
                stage = wrapper.stage;
                stageStartSeconds = wrapper.stageStartSeconds;
                moistureAddedDuringRescue = wrapper.moistureAddedDuringRescue;
                accumulator = legacy ? 0f : wrapper.accumulator;
                paused = legacy ? false : wrapper.paused;
                simulationSpeed = legacy ? 1f : wrapper.simulationSpeed;
                RestoreNutrientState(wrapper);
                PublishStage();
                PublishSnapshot();
                return true;
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                error = exception.Message;
                return false;
            }
        }

        private void ValidateExperimentSave(ExperimentSave wrapper)
        {
            if (wrapper == null || wrapper.simulation == null)
                throw new InvalidDataException("Save data is incomplete.");
            if (wrapper.schemaVersion != 0 &&
                wrapper.schemaVersion != 2 &&
                wrapper.schemaVersion != 3 &&
                wrapper.schemaVersion != CurrentExperimentSaveSchemaVersion)
                throw new InvalidDataException($"Unsupported experiment save schema {wrapper.schemaVersion}.");
            if (wrapper.schemaVersion >= 3 &&
                wrapper.simulation.schemaVersion != PetriSimulation.CurrentSaveSchemaVersion)
                throw new InvalidDataException(
                    "Experiment and simulation save schemas do not describe the same content selection.");
            if (!Enum.IsDefined(typeof(GuidedStage), wrapper.stage))
                throw new InvalidDataException("Save data contains an invalid guided stage.");
            if (!IsFinite(wrapper.stageStartSeconds) || wrapper.stageStartSeconds < 0f ||
                wrapper.stageStartSeconds > wrapper.simulation.elapsedSimSeconds)
                throw new InvalidDataException("Save data contains an invalid stage time.");

            if (wrapper.schemaVersion == 0) return;
            if (!IsFinite(wrapper.accumulator) || wrapper.accumulator < 0f ||
                wrapper.accumulator > MaxPendingSimulationSeconds)
                throw new InvalidDataException("Save data contains an invalid fixed-step accumulator.");
            if (!IsFinite(wrapper.simulationSpeed) ||
                wrapper.simulationSpeed < 0.5f || wrapper.simulationSpeed > 8f)
                throw new InvalidDataException("Save data contains an invalid simulation speed.");
            if (wrapper.schemaVersion >= 4) ValidateNutrientSave(wrapper);
        }

        private void ResolveSavedDefinitions(
            ExperimentSave wrapper,
            out OrganismDefinition organismDefinition,
            out MediumDefinition mediumDefinition)
        {
            EnsureDefinitionCatalog();
            bool legacyContentSelection =
                wrapper.schemaVersion <= 2 || wrapper.simulation.schemaVersion <= 2;
            if (legacyContentSelection)
            {
                organismDefinition = definitionCatalog.ResolveOrganism(
                    SimulationDefinitionCatalog.RapidBacteriumId);
                mediumDefinition = definitionCatalog.ResolveMedium(
                    SimulationDefinitionCatalog.NutrientAgarId);
                return;
            }

            organismDefinition = definitionCatalog.ResolveOrganism(wrapper.simulation.organismId);
            mediumDefinition = definitionCatalog.ResolveMedium(wrapper.simulation.mediumId);
        }

        private NutrientInterventionRecord[] CopyNutrientHistory()
        {
            var copy = new NutrientInterventionRecord[nutrientHistory.Count];
            for (int i = 0; i < copy.Length; i++) copy[i] = nutrientHistory[i].Clone();
            return copy;
        }

        private void RestoreNutrientState(ExperimentSave wrapper)
        {
            nutrientHistory.Clear();
            if (wrapper.schemaVersion < 4)
            {
                nutrientDosesRemaining = nutrientDoseSupply;
                nutrientCooldownEndTick = 0;
                pendingNutrientDelivery = null;
                NutrientFeedback = "Legacy save loaded; all nutrient doses are available.";
                return;
            }

            nutrientDosesRemaining = wrapper.nutrientDosesRemaining;
            nutrientCooldownEndTick = wrapper.nutrientCooldownEndTick;
            for (int i = 0; i < wrapper.nutrientHistory.Length; i++)
                nutrientHistory.Add(wrapper.nutrientHistory[i].Clone());
            pendingNutrientDelivery = HasPendingNutrientDelivery(wrapper.pendingNutrientDelivery)
                ? wrapper.pendingNutrientDelivery.Clone()
                : null;
            NutrientFeedback = pendingNutrientDelivery != null
                ? "Saved nutrient delivery resumed."
                : nutrientDosesRemaining > 0
                    ? "Nutrient doses are ready."
                    : "No nutrient doses remain in this experiment.";
        }

        private void ValidateNutrientSave(ExperimentSave wrapper)
        {
            if (wrapper.nutrientDosesRemaining < 0 ||
                wrapper.nutrientDosesRemaining > nutrientDoseSupply)
                throw new InvalidDataException("Save data contains an invalid nutrient supply.");
            if (wrapper.nutrientCooldownEndTick < 0)
                throw new InvalidDataException("Save data contains an invalid nutrient cooldown.");
            if (wrapper.nutrientHistory == null)
                throw new InvalidDataException("Save data is missing nutrient intervention history.");
            if (wrapper.nutrientHistory.Length !=
                nutrientDoseSupply - wrapper.nutrientDosesRemaining)
                throw new InvalidDataException("Nutrient supply does not match intervention history.");

            int incompleteIndex = -1;
            for (int i = 0; i < wrapper.nutrientHistory.Length; i++)
            {
                NutrientInterventionRecord record = wrapper.nutrientHistory[i];
                if (record == null ||
                    record.RequestTick < 0 ||
                    record.RequestTick > wrapper.simulation.tick ||
                    !IsFinite(record.RequestedAmount) ||
                    Mathf.Abs(record.RequestedAmount - nutrientDoseAmount) > 0.000001f ||
                    !IsFinite(record.DeliveredAmount) ||
                    record.DeliveredAmount < 0f ||
                    record.DeliveredAmount > record.RequestedAmount + 0.000001f)
                    throw new InvalidDataException("Save data contains an invalid nutrient history record.");
                if (i > 0 && record.RequestTick < wrapper.nutrientHistory[i - 1].RequestTick)
                    throw new InvalidDataException("Nutrient history is not ordered by request tick.");
                if (record.DeliveryStartTick >= 0 &&
                    (record.DeliveryStartTick != record.RequestTick + nutrientDelaySteps ||
                     record.DeliveryStartTick > wrapper.simulation.tick))
                    throw new InvalidDataException("Nutrient history contains an invalid delivery start tick.");
                if (record.IsComplete)
                {
                    if (record.DeliveryStartTick < 0 ||
                        record.CompletionTick != record.DeliveryStartTick + nutrientReleaseSteps - 1 ||
                        record.CompletionTick > wrapper.simulation.tick)
                        throw new InvalidDataException("Nutrient history contains an invalid completion tick.");
                }
                else
                {
                    if (incompleteIndex >= 0 || i != wrapper.nutrientHistory.Length - 1)
                        throw new InvalidDataException("Save data contains multiple pending nutrient doses.");
                    incompleteIndex = i;
                }
            }

            if (wrapper.nutrientHistory.Length == 0)
            {
                if (wrapper.nutrientCooldownEndTick != 0 ||
                    HasPendingNutrientDelivery(wrapper.pendingNutrientDelivery))
                    throw new InvalidDataException("Empty nutrient history contains pending state.");
                return;
            }

            NutrientInterventionRecord latest =
                wrapper.nutrientHistory[wrapper.nutrientHistory.Length - 1];
            if (wrapper.nutrientCooldownEndTick != latest.RequestTick + nutrientCooldownSteps)
                throw new InvalidDataException("Save data contains an invalid nutrient cooldown tick.");
            if (incompleteIndex < 0)
            {
                if (HasPendingNutrientDelivery(wrapper.pendingNutrientDelivery))
                    throw new InvalidDataException("Completed nutrient history contains pending state.");
                return;
            }

            PendingNutrientDelivery pending = wrapper.pendingNutrientDelivery;
            if (pending == null ||
                pending.historyIndex != incompleteIndex ||
                pending.requestTick != latest.RequestTick ||
                pending.scheduledStartTick != latest.RequestTick + nutrientDelaySteps ||
                !IsFinite(pending.requestedAmount) ||
                Mathf.Abs(pending.requestedAmount - latest.RequestedAmount) > 0.000001f ||
                !IsFinite(pending.deliveredAmount) ||
                Mathf.Abs(pending.deliveredAmount - latest.DeliveredAmount) > 0.000001f ||
                pending.releaseStepsCompleted < 0 ||
                pending.releaseStepsCompleted >= nutrientReleaseSteps)
                throw new InvalidDataException("Save data contains malformed pending nutrient delivery.");
            if ((pending.releaseStepsCompleted == 0 && latest.DeliveryStartTick >= 0) ||
                (pending.releaseStepsCompleted > 0 && latest.DeliveryStartTick < 0))
                throw new InvalidDataException("Pending nutrient progress does not match its history.");
        }

        private static bool HasPendingNutrientDelivery(PendingNutrientDelivery pending)
        {
            return pending != null &&
                (pending.requestedAmount != 0f || pending.scheduledStartTick != 0 ||
                 pending.requestTick != 0 || pending.deliveredAmount != 0f ||
                 pending.releaseStepsCompleted != 0 || pending.capacityLimited);
        }

        private void ValidateNutrientConfiguration()
        {
            if (!IsFinite(nutrientDoseAmount) || nutrientDoseAmount <= 0f ||
                nutrientDoseAmount > 1f || nutrientDoseSupply < 1 ||
                nutrientCooldownSteps < 0 || nutrientDelaySteps < 1 ||
                nutrientReleaseSteps < 1)
                throw new InvalidOperationException(
                    "Nutrient intervention settings are outside the supported range.");
        }

        private void EnsureDefinitionSelection()
        {
            EnsureDefinitionCatalog();
            if (string.IsNullOrWhiteSpace(selectedOrganismId))
                selectedOrganismId = definitionCatalog.DefaultOrganism.Id;
            if (string.IsNullOrWhiteSpace(selectedMediumId))
                selectedMediumId = definitionCatalog.DefaultMedium.Id;
            definitionCatalog.ResolveOrganism(selectedOrganismId);
            definitionCatalog.ResolveMedium(selectedMediumId);
        }

        private void EnsureDefinitionCatalog()
        {
            if (definitionCatalog == null)
                definitionCatalog = SimulationDefinitionCatalog.LoadDefaultOrThrow();
            definitionCatalog.ValidateOrThrow();
        }

        private static string ValidatePersistencePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("A save path is required.", nameof(path));
            return Path.GetFullPath(path);
        }

        private static void WriteSaveAtomically(string path, string json)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            string temporaryPath = path + ".tmp";
            string backupPath = path + ".bak";
            try
            {
                File.WriteAllText(temporaryPath, json, new UTF8Encoding(false));
                if (File.Exists(path))
                    ReplaceWithBackup(temporaryPath, path, backupPath);
                else
                    File.Move(temporaryPath, path);
            }
            finally
            {
                TryDelete(temporaryPath);
            }
        }

        private static void ReplaceWithBackup(string temporaryPath, string path, string backupPath)
        {
            try
            {
                File.Replace(temporaryPath, path, backupPath);
            }
            catch (PlatformNotSupportedException)
            {
                ReplaceWithRecoveryMoves(temporaryPath, path, backupPath);
            }
            catch (NotSupportedException)
            {
                ReplaceWithRecoveryMoves(temporaryPath, path, backupPath);
            }
        }

        private static void ReplaceWithRecoveryMoves(string temporaryPath, string path, string backupPath)
        {
            if (File.Exists(backupPath)) File.Delete(backupPath);
            File.Move(path, backupPath);
            try
            {
                File.Move(temporaryPath, path);
            }
            catch (Exception replacementError) when (IsPersistenceException(replacementError))
            {
                try
                {
                    if (!File.Exists(path) && File.Exists(backupPath))
                        File.Move(backupPath, path);
                }
                catch (Exception recoveryError) when (IsPersistenceException(recoveryError))
                {
                    throw new IOException(
                        "Save replacement failed and the previous save could not be restored automatically.",
                        recoveryError);
                }
                throw;
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private void PublishStage()
        {
            StageChanged?.Invoke(stage, MessageForStage(stage));
        }

        private void PublishSnapshot()
        {
            SnapshotUpdated?.Invoke(simulation.CreateSnapshot());
        }

        private static bool IsPersistenceException(Exception exception)
        {
            return exception is IOException ||
                   exception is UnauthorizedAccessException ||
                   exception is ArgumentException ||
                   exception is InvalidDataException ||
                   exception is NotSupportedException ||
                   exception is SecurityException ||
                   exception is InvalidOperationException;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private static string MessageForStage(GuidedStage value)
        {
            switch (value)
            {
                case GuidedStage.ObserveCoolStart: return "Observe the slow colony. The dish is cooler than this culture prefers.";
                case GuidedStage.WarmToIdeal: return "Raise the temperature into the comfortable range: 24–29°C.";
                case GuidedStage.HoldIdeal: return "Hold the culture in range and watch the active edge expand.";
                case GuidedStage.HeaterFault: return "Heater fault: bring the dish below 30°C.";
                case GuidedStage.MoistureRescue: return "Add moisture before the edge dries further.";
                case GuidedStage.Recovery: return "Conditions are improving. Recovery takes time.";
                case GuidedStage.Complete: return "Experiment complete.";
                default: return "The experiment failed. Review the limiting factors and retry.";
            }
        }

        [Serializable]
        private sealed class ExperimentSave
        {
            public int schemaVersion;
            public SimulationSaveData simulation;
            public GuidedStage stage;
            public float stageStartSeconds;
            public bool moistureAddedDuringRescue;
            public float accumulator;
            public bool paused;
            public float simulationSpeed;
            public int nutrientDosesRemaining;
            public long nutrientCooldownEndTick;
            public PendingNutrientDelivery pendingNutrientDelivery;
            public NutrientInterventionRecord[] nutrientHistory;
        }

        [Serializable]
        private sealed class PendingNutrientDelivery
        {
            public int historyIndex;
            public long requestTick;
            public long scheduledStartTick;
            public float requestedAmount;
            public float deliveredAmount;
            public int releaseStepsCompleted;
            public bool capacityLimited;

            public PendingNutrientDelivery Clone()
            {
                return new PendingNutrientDelivery
                {
                    historyIndex = historyIndex,
                    requestTick = requestTick,
                    scheduledStartTick = scheduledStartTick,
                    requestedAmount = requestedAmount,
                    deliveredAmount = deliveredAmount,
                    releaseStepsCompleted = releaseStepsCompleted,
                    capacityLimited = capacityLimited
                };
            }
        }
    }
}
