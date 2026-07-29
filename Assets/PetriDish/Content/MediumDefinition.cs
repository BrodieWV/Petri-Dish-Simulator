using UnityEngine;

namespace PetriDish.Content
{
    [CreateAssetMenu(
        fileName = "MediumDefinition",
        menuName = "Petri Dish/Definitions/Medium")]
    public sealed class MediumDefinition : ScriptableObject
    {
        [Header("Identity and education")]
        [SerializeField] private string id;
        [SerializeField, Min(1)] private int definitionVersion = 1;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string educationalDescription;
        [SerializeField, TextArea] private string scientificLabel;
        [SerializeField, TextArea] private string sourceNotes;
        [SerializeField] private ScientificConfidence scientificConfidence;
        [SerializeField, TextArea] private string simplificationNotes;
        [SerializeField] private string visualProfileId;

        [Header("Capacity and starting state")]
        [SerializeField, Range(0f, 1f)] private float startingNutrientLevel = 1f;
        [SerializeField, Range(0.001f, 1f)] private float maximumNutrientCapacity = 1f;
        [SerializeField, Range(0f, 1f)] private float startingMoisture = 0.72f;
        [SerializeField, Range(0.001f, 1f)] private float maximumMoistureCapacity = 1f;
        [SerializeField, Range(0f, 2f)] private float moistureAbsorptionMultiplier = 1f;
        [SerializeField, Range(0f, 1f)] private float moistureApplicationVariance = 0.15f;

        [Header("Diffusion and spread per fixed step")]
        [SerializeField, Range(0f, 1f)] private float moistureDiffusion;
        [SerializeField, Range(0f, 1f)] private float nutrientDiffusion;
        [SerializeField, Range(0f, 1f)] private float spreadResistance;

        [Header("Thermal and evaporation response per fixed step")]
        [SerializeField, Range(0.001f, 34f)] private float temperatureResponseRate = 0.18f;
        [SerializeField, Range(0f, 1f)] private float evaporationRate = 0.00045f;
        [SerializeField, Range(1f, 20f)] private float edgeDryingMultiplier = 3.7777778f;
        [SerializeField, Range(0.001f, 48f)] private float edgeFalloffDepthCells = 12f;
        [SerializeField] private float heatEvaporationStartTemperature = 24f;
        [SerializeField, Range(0f, 1f)] private float heatEvaporationPerDegree = 0.00012f;

        public string Id => id;
        public int DefinitionVersion => definitionVersion;
        public string DisplayName => displayName;
        public string EducationalDescription => educationalDescription;
        public string ScientificLabel => scientificLabel;
        public string SourceNotes => sourceNotes;
        public ScientificConfidence Confidence => scientificConfidence;
        public string SimplificationNotes => simplificationNotes;
        public string VisualProfileId => visualProfileId;
        public float StartingNutrientLevel => startingNutrientLevel;
        public float MaximumNutrientCapacity => maximumNutrientCapacity;
        public float StartingMoisture => startingMoisture;
        public float MaximumMoistureCapacity => maximumMoistureCapacity;
        public float MoistureDiffusion => moistureDiffusion;
        public float NutrientDiffusion => nutrientDiffusion;
        public float EvaporationRate => evaporationRate;
        public float EdgeDryingMultiplier => edgeDryingMultiplier;
        public float SpreadResistance => spreadResistance;

        internal float MoistureAbsorptionMultiplier => moistureAbsorptionMultiplier;
        internal float MoistureApplicationVariance => moistureApplicationVariance;
        internal float TemperatureResponseRate => temperatureResponseRate;
        internal float EdgeFalloffDepthCells => edgeFalloffDepthCells;
        internal float HeatEvaporationStartTemperature => heatEvaporationStartTemperature;
        internal float HeatEvaporationPerDegree => heatEvaporationPerDegree;

        public MediumSimulationValues ToSimulationValues()
        {
            ValidateOrThrow();
            return new MediumSimulationValues(this);
        }

        public void ValidateOrThrow()
        {
            DefinitionValidation.ValidateId(id, nameof(MediumDefinition));
            DefinitionValidation.Require(definitionVersion >= 1, id, "Definition version must be at least 1.");
            RequireText(displayName, nameof(displayName));
            RequireText(educationalDescription, nameof(educationalDescription));
            RequireText(scientificLabel, nameof(scientificLabel));
            RequireText(sourceNotes, nameof(sourceNotes));
            DefinitionValidation.Require(
                System.Enum.IsDefined(typeof(ScientificConfidence), scientificConfidence) &&
                scientificConfidence != ScientificConfidence.Unspecified,
                id,
                "Scientific confidence must be a supported explicit selection.");
            RequireText(simplificationNotes, nameof(simplificationNotes));
            DefinitionValidation.ValidateId(visualProfileId, "Medium visual profile");
            DefinitionValidation.Unit(startingNutrientLevel, id, nameof(startingNutrientLevel));
            DefinitionValidation.UnitPositive(
                maximumNutrientCapacity,
                id,
                nameof(maximumNutrientCapacity));
            DefinitionValidation.Require(
                startingNutrientLevel <= maximumNutrientCapacity,
                id,
                "Starting nutrients cannot exceed maximum nutrient capacity.");
            DefinitionValidation.Unit(startingMoisture, id, nameof(startingMoisture));
            DefinitionValidation.UnitPositive(
                maximumMoistureCapacity,
                id,
                nameof(maximumMoistureCapacity));
            DefinitionValidation.Require(
                startingMoisture <= maximumMoistureCapacity,
                id,
                "Starting moisture cannot exceed maximum moisture capacity.");
            DefinitionValidation.Range(
                moistureAbsorptionMultiplier,
                0f,
                2f,
                id,
                nameof(moistureAbsorptionMultiplier));
            DefinitionValidation.Unit(
                moistureApplicationVariance,
                id,
                nameof(moistureApplicationVariance));
            DefinitionValidation.Unit(moistureDiffusion, id, nameof(moistureDiffusion));
            DefinitionValidation.Unit(nutrientDiffusion, id, nameof(nutrientDiffusion));
            DefinitionValidation.Unit(spreadResistance, id, nameof(spreadResistance));
            DefinitionValidation.Range(
                temperatureResponseRate,
                0.001f,
                34f,
                id,
                nameof(temperatureResponseRate));
            DefinitionValidation.Unit(evaporationRate, id, nameof(evaporationRate));
            DefinitionValidation.Range(
                edgeDryingMultiplier,
                1f,
                20f,
                id,
                nameof(edgeDryingMultiplier));
            DefinitionValidation.Require(
                evaporationRate * edgeDryingMultiplier <= maximumMoistureCapacity,
                id,
                "Edge evaporation cannot exceed the supported moisture range per step.");
            DefinitionValidation.Range(
                edgeFalloffDepthCells,
                0.001f,
                48f,
                id,
                nameof(edgeFalloffDepthCells));
            DefinitionValidation.Range(
                heatEvaporationStartTemperature,
                8f,
                42f,
                id,
                nameof(heatEvaporationStartTemperature));
            DefinitionValidation.Unit(
                heatEvaporationPerDegree,
                id,
                nameof(heatEvaporationPerDegree));
        }

        private void RequireText(string value, string field)
        {
            DefinitionValidation.Require(!string.IsNullOrWhiteSpace(value), id, $"{field} is required.");
        }
    }

    public sealed class MediumSimulationValues
    {
        public readonly string Id;
        public readonly int DefinitionVersion;
        public readonly float InitialMoisture;
        public readonly float MaximumMoisture;
        public readonly float InitialNutrients;
        public readonly float MaximumNutrients;
        public readonly float MoistureAbsorptionMultiplier;
        public readonly float MoistureApplicationVariance;
        public readonly float MoistureDiffusion;
        public readonly float NutrientDiffusion;
        public readonly float SpreadResistance;
        public readonly float TemperatureResponseRate;
        public readonly float EdgeEvaporation;
        public readonly float InteriorEvaporation;
        public readonly float EdgeFalloffDepthCells;
        public readonly float HeatEvaporationStartTemperature;
        public readonly float HeatEvaporationPerDegree;

        internal MediumSimulationValues(MediumDefinition source)
        {
            Id = source.Id;
            DefinitionVersion = source.DefinitionVersion;
            InitialMoisture = source.StartingMoisture;
            MaximumMoisture = source.MaximumMoistureCapacity;
            InitialNutrients = source.StartingNutrientLevel;
            MaximumNutrients = source.MaximumNutrientCapacity;
            MoistureAbsorptionMultiplier = source.MoistureAbsorptionMultiplier;
            MoistureApplicationVariance = source.MoistureApplicationVariance;
            MoistureDiffusion = source.MoistureDiffusion;
            NutrientDiffusion = source.NutrientDiffusion;
            SpreadResistance = source.SpreadResistance;
            TemperatureResponseRate = source.TemperatureResponseRate;
            InteriorEvaporation = source.EvaporationRate;
            EdgeEvaporation = source.EvaporationRate * source.EdgeDryingMultiplier;
            EdgeFalloffDepthCells = source.EdgeFalloffDepthCells;
            HeatEvaporationStartTemperature = source.HeatEvaporationStartTemperature;
            HeatEvaporationPerDegree = source.HeatEvaporationPerDegree;
        }
    }
}
