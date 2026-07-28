using UnityEngine;

namespace PetriDish.Content
{
    [CreateAssetMenu(
        fileName = "MediumDefinition",
        menuName = "Petri Dish/Definitions/Medium")]
    public sealed class MediumDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField, Min(1)] private int definitionVersion = 1;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string scientificLabel;

        [Header("Starting state")]
        [SerializeField, Range(0f, 1f)] private float initialMoisture = 0.72f;
        [SerializeField, Range(0f, 1f)] private float initialNutrients = 1f;
        [SerializeField, Range(0f, 2f)] private float moistureAbsorptionMultiplier = 1f;
        [SerializeField, Range(0f, 1f)] private float moistureApplicationVariance = 0.15f;

        [Header("Thermal response per fixed step")]
        [SerializeField, Range(0.001f, 34f)] private float temperatureResponseRate = 0.18f;

        [Header("Evaporation per fixed step")]
        [SerializeField, Range(0f, 1f)] private float edgeEvaporation = 0.0017f;
        [SerializeField, Range(0f, 1f)] private float interiorEvaporation = 0.00045f;
        [SerializeField, Min(0.001f)] private float edgeFalloffDepthCells = 12f;
        [SerializeField] private float heatEvaporationStartTemperature = 24f;
        [SerializeField, Range(0f, 1f)] private float heatEvaporationPerDegree = 0.00012f;

        public string Id => id;
        public int DefinitionVersion => definitionVersion;
        public string DisplayName => displayName;
        public string ScientificLabel => scientificLabel;

        public MediumSimulationValues ToSimulationValues()
        {
            ValidateOrThrow();
            return new MediumSimulationValues(
                id,
                definitionVersion,
                initialMoisture,
                initialNutrients,
                moistureAbsorptionMultiplier,
                moistureApplicationVariance,
                temperatureResponseRate,
                edgeEvaporation,
                interiorEvaporation,
                edgeFalloffDepthCells,
                heatEvaporationStartTemperature,
                heatEvaporationPerDegree);
        }

        public void ValidateOrThrow()
        {
            DefinitionValidation.ValidateId(id, nameof(MediumDefinition));
            DefinitionValidation.Require(definitionVersion >= 1, id, "Definition version must be at least 1.");
            DefinitionValidation.Require(!string.IsNullOrWhiteSpace(displayName), id, "Display name is required.");
            DefinitionValidation.Require(
                !string.IsNullOrWhiteSpace(scientificLabel),
                id,
                "A scientific or simplification label is required.");
            DefinitionValidation.Unit(initialMoisture, id, nameof(initialMoisture));
            DefinitionValidation.Unit(initialNutrients, id, nameof(initialNutrients));
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
            DefinitionValidation.Range(
                temperatureResponseRate,
                0.001f,
                34f,
                id,
                nameof(temperatureResponseRate));
            DefinitionValidation.Unit(edgeEvaporation, id, nameof(edgeEvaporation));
            DefinitionValidation.Unit(interiorEvaporation, id, nameof(interiorEvaporation));
            DefinitionValidation.Require(
                edgeEvaporation >= interiorEvaporation,
                id,
                "Edge evaporation must be at least the interior evaporation.");
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
    }

    public sealed class MediumSimulationValues
    {
        public readonly string Id;
        public readonly int DefinitionVersion;
        public readonly float InitialMoisture;
        public readonly float InitialNutrients;
        public readonly float MoistureAbsorptionMultiplier;
        public readonly float MoistureApplicationVariance;
        public readonly float TemperatureResponseRate;
        public readonly float EdgeEvaporation;
        public readonly float InteriorEvaporation;
        public readonly float EdgeFalloffDepthCells;
        public readonly float HeatEvaporationStartTemperature;
        public readonly float HeatEvaporationPerDegree;

        internal MediumSimulationValues(
            string id,
            int definitionVersion,
            float initialMoisture,
            float initialNutrients,
            float moistureAbsorptionMultiplier,
            float moistureApplicationVariance,
            float temperatureResponseRate,
            float edgeEvaporation,
            float interiorEvaporation,
            float edgeFalloffDepthCells,
            float heatEvaporationStartTemperature,
            float heatEvaporationPerDegree)
        {
            Id = id;
            DefinitionVersion = definitionVersion;
            InitialMoisture = initialMoisture;
            InitialNutrients = initialNutrients;
            MoistureAbsorptionMultiplier = moistureAbsorptionMultiplier;
            MoistureApplicationVariance = moistureApplicationVariance;
            TemperatureResponseRate = temperatureResponseRate;
            EdgeEvaporation = edgeEvaporation;
            InteriorEvaporation = interiorEvaporation;
            EdgeFalloffDepthCells = edgeFalloffDepthCells;
            HeatEvaporationStartTemperature = heatEvaporationStartTemperature;
            HeatEvaporationPerDegree = heatEvaporationPerDegree;
        }
    }
}
