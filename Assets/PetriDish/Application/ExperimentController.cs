using System;
using System.IO;
using PetriDish.Simulation;
using UnityEngine;

namespace PetriDish.Application
{
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

        public event Action<SimulationSnapshot> SnapshotUpdated;
        public event Action<GuidedStage, string> StageChanged;

        [SerializeField] private float simulationSpeed = 1f;
        private PetriSimulation simulation;
        private float accumulator;
        private bool paused;
        private GuidedStage stage;
        private float stageStartSeconds;
        private bool moistureAddedDuringRescue;
        private string savePath;

        public PetriSimulation Simulation => simulation;
        public GuidedStage Stage => stage;
        public bool Paused => paused;
        public float SimulationSpeed => simulationSpeed;

        private void Awake()
        {
            savePath = Path.Combine(Application.persistentDataPath, "petri_vertical_slice.json");
            StartNew(TutorialSeed);
        }

        private void Update()
        {
            if (paused || simulation == null) return;
            accumulator += Time.unscaledDeltaTime * simulationSpeed;
            while (accumulator >= PetriSimulation.FixedStepSeconds)
            {
                accumulator -= PetriSimulation.FixedStepSeconds;
                simulation.Step();
                UpdateGuidedFlow();
            }

            SnapshotUpdated?.Invoke(simulation.CreateSnapshot());
        }

        public void StartNew(int seed)
        {
            simulation = new PetriSimulation(seed);
            accumulator = 0f;
            paused = false;
            simulationSpeed = 1f;
            moistureAddedDuringRescue = false;
            SetStage(GuidedStage.ObserveCoolStart,
                "Observe the slow colony. The dish is cooler than this culture prefers.");
            SnapshotUpdated?.Invoke(simulation.CreateSnapshot());
        }

        public void SetTemperature(float value)
        {
            simulation.SetTargetTemperature(value);
        }

        public void AddMoisture()
        {
            simulation.AddMoisture(0.16f);
            if (stage == GuidedStage.MoistureRescue) moistureAddedDuringRescue = true;
        }

        public void SetSpeed(float value)
        {
            simulationSpeed = Mathf.Clamp(value, 0.5f, 8f);
        }

        public void TogglePause() => paused = !paused;

        public void RestartSameSeed() => StartNew(TutorialSeed);

        public void RestartNewSeed() => StartNew(UnityEngine.Random.Range(1, int.MaxValue));

        public void Save()
        {
            var wrapper = new ExperimentSave
            {
                simulation = simulation.CaptureSave(),
                stage = stage,
                stageStartSeconds = stageStartSeconds,
                moistureAddedDuringRescue = moistureAddedDuringRescue
            };
            File.WriteAllText(savePath, JsonUtility.ToJson(wrapper, true));
        }

        public bool Load()
        {
            if (!File.Exists(savePath)) return false;
            var wrapper = JsonUtility.FromJson<ExperimentSave>(File.ReadAllText(savePath));
            simulation = new PetriSimulation(wrapper.simulation.seed);
            simulation.Restore(wrapper.simulation);
            stage = wrapper.stage;
            stageStartSeconds = wrapper.stageStartSeconds;
            moistureAddedDuringRescue = wrapper.moistureAddedDuringRescue;
            StageChanged?.Invoke(stage, MessageForStage(stage));
            SnapshotUpdated?.Invoke(simulation.CreateSnapshot());
            return true;
        }

        private void UpdateGuidedFlow()
        {
            float elapsed = simulation.ElapsedSimSeconds - stageStartSeconds;
            SimulationSnapshot snapshot = simulation.CreateSnapshot();

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
                    if (elapsed >= 18f && snapshot.Coverage >= 0.035f)
                    {
                        simulation.SetTargetTemperature(36f);
                        SetStage(GuidedStage.HeaterFault, "Heater fault: the dish is overheating. Bring it below 30°C.");
                    }
                    break;
                case GuidedStage.HeaterFault:
                    if (simulation.TargetTemperature < 30f)
                        SetStage(GuidedStage.MoistureRescue, "Heat increased water loss. Add moisture before the edge dries further.");
                    else if (elapsed > 30f && snapshot.AverageHealth < 0.45f)
                        SetStage(GuidedStage.Failed, "The culture remained overheated too long. Review the cause or restart from the experiment.");
                    break;
                case GuidedStage.MoistureRescue:
                    if (moistureAddedDuringRescue)
                        SetStage(GuidedStage.Recovery, "Conditions are improving. Recovery takes time; keep the temperature stable.");
                    break;
                case GuidedStage.Recovery:
                    if (elapsed >= 20f && snapshot.AverageHealth >= 0.72f && simulation.Temperature < 30f)
                        SetStage(GuidedStage.Complete, "Experiment complete: you found the comfortable range and recovered the culture.");
                    else if (elapsed > 45f && snapshot.AverageHealth < 0.35f)
                        SetStage(GuidedStage.Failed, "The culture could not recover. Temperature and moisture were the main limiting factors.");
                    break;
            }
        }

        private void SetStage(GuidedStage next, string message)
        {
            stage = next;
            stageStartSeconds = simulation != null ? simulation.ElapsedSimSeconds : 0f;
            StageChanged?.Invoke(stage, message);
        }

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
            public SimulationSaveData simulation;
            public GuidedStage stage;
            public float stageStartSeconds;
            public bool moistureAddedDuringRescue;
        }
    }
}
