using UnityEngine;

namespace PetriDish.Content
{
    public enum ScientificConfidence
    {
        Unspecified = 0,
        EducationalPlaceholder = 1,
        Low = 2,
        Moderate = 3,
        High = 4
    }

    [CreateAssetMenu(
        fileName = "OrganismDefinition",
        menuName = "Petri Dish/Definitions/Organism")]
    public sealed class OrganismDefinition : ScriptableObject
    {
        [Header("Identity and education")]
        [SerializeField] private string id;
        [SerializeField, Min(1)] private int definitionVersion = 1;
        [SerializeField] private string displayName;
        [SerializeField] private string scientificName;
        [SerializeField, TextArea] private string educationalDescription;
        [SerializeField, TextArea] private string scientificLabel;
        [SerializeField, TextArea] private string sourceNotes;
        [SerializeField] private ScientificConfidence scientificConfidence;
        [SerializeField, TextArea] private string simplificationNotes;
        [SerializeField] private string visualProfileId;

        [Header("Starting colony")]
        [SerializeField, Range(0f, 1f)] private float initialHealth = 1f;
        [SerializeField, Range(0.1f, 24f)] private float seedRadiusCells = 2.3f;
        [SerializeField, Range(0f, 1f)] private float seedCenterBiomass = 0.28f;
        [SerializeField, Range(0f, 1f)] private float seedEdgeBiomass = 0.08f;

        [Header("Temperature ranges (degrees Celsius)")]
        [SerializeField] private float preferredTemperatureMinimum = 24f;
        [SerializeField] private float preferredTemperatureMaximum = 29f;
        [SerializeField] private float growthTemperatureMinimum = 18.5f;
        [SerializeField] private float growthTemperatureMaximum = 33.5f;
        [SerializeField] private float survivalTemperatureMinimum = 11f;
        [SerializeField] private float survivalTemperatureMaximum = 38f;
        [SerializeField] private float temperatureOptimum = 26f;
        [SerializeField, Range(0.001f, 34f)] private float temperatureResponseHalfRange = 7.5f;

        [Header("Moisture ranges (normalised)")]
        [SerializeField, Range(0f, 1f)] private float preferredMoistureMinimum = 0.60f;
        [SerializeField, Range(0f, 1f)] private float preferredMoistureMaximum = 0.80f;
        [SerializeField, Range(0f, 1f)] private float growthMoistureThreshold = 0.35f;
        [SerializeField, Range(0f, 1f)] private float survivalMoistureThreshold = 0.16f;
        [SerializeField, Range(0f, 1f)] private float moistureOptimum = 0.70f;
        [SerializeField, Range(0.001f, 1f)] private float moistureResponseHalfRange = 0.35f;

        [Header("Growth, demand, and stress per fixed step")]
        [SerializeField, Range(0.001f, 1f)] private float nutrientsForFullSuitability = 0.25f;
        [SerializeField, Range(0f, 1f)] private float healthySuitabilityThreshold = 0.55f;
        [SerializeField, Range(0f, 1f)] private float healthRecoveryRate = 0.015f;
        [SerializeField, Range(0f, 1f)] private float damageSensitivity = 0.018f;
        [SerializeField, Range(0f, 1f)] private float healthDeclineStressFloor = 0.35f;
        [SerializeField, Range(0f, 1f)] private float stressRecovery = 0.025f;
        [SerializeField, Range(0f, 1f)] private float stressSensitivity = 0.025f;
        [SerializeField, Range(0f, 1f)] private float lethalStressSensitivity = 0.06f;
        [SerializeField, Range(0f, 1f)] private float baseGrowthRate = 0.07f;
        [SerializeField, Range(0f, 1f)] private float lethalDeathRate = 0.035f;
        [SerializeField, Range(0f, 1f)] private float stressDeathRate = 0.004f;
        [SerializeField, Range(0f, 1f)] private float nutrientDemand = 0.42f;
        [SerializeField, Range(0f, 1f)] private float moistureDemand = 0.025f;
        [SerializeField, Range(0.01f, 1f)] private float carryingCapacity = 1f;

        [Header("Spread")]
        [SerializeField, Range(0f, 1f)] private float spreadMinimumBiomass = 0.035f;
        [SerializeField, Range(0f, 1f)] private float spreadMinimumSuitability = 0.28f;
        [SerializeField, Range(0f, 1f)] private float spreadRate = 0.006f;
        [SerializeField, Range(0f, 4f)] private float spreadRandomMinimum = 0.75f;
        [SerializeField, Range(0f, 4f)] private float spreadRandomMaximum = 1.25f;

        public string Id => id;
        public int DefinitionVersion => definitionVersion;
        public string DisplayName => displayName;
        public string ScientificName => scientificName;
        public string EducationalDescription => educationalDescription;
        public string ScientificLabel => scientificLabel;
        public string SourceNotes => sourceNotes;
        public ScientificConfidence Confidence => scientificConfidence;
        public string SimplificationNotes => simplificationNotes;
        public string VisualProfileId => visualProfileId;
        public float PreferredTemperatureMinimum => preferredTemperatureMinimum;
        public float PreferredTemperatureMaximum => preferredTemperatureMaximum;
        public float GrowthTemperatureMinimum => growthTemperatureMinimum;
        public float GrowthTemperatureMaximum => growthTemperatureMaximum;
        public float SurvivalTemperatureMinimum => survivalTemperatureMinimum;
        public float SurvivalTemperatureMaximum => survivalTemperatureMaximum;
        public float PreferredMoistureMinimum => preferredMoistureMinimum;
        public float PreferredMoistureMaximum => preferredMoistureMaximum;
        public float GrowthMoistureThreshold => growthMoistureThreshold;
        public float SurvivalMoistureThreshold => survivalMoistureThreshold;
        public float BaseGrowthRate => baseGrowthRate;
        public float SpreadRate => spreadRate;
        public float NutrientDemand => nutrientDemand;
        public float MoistureDemand => moistureDemand;
        public float StressRecovery => stressRecovery;
        public float StressSensitivity => stressSensitivity;
        public float DamageSensitivity => damageSensitivity;
        public float CarryingCapacity => carryingCapacity;

        internal float InitialHealth => initialHealth;
        internal float SeedRadiusCells => seedRadiusCells;
        internal float SeedCenterBiomass => seedCenterBiomass;
        internal float SeedEdgeBiomass => seedEdgeBiomass;
        internal float TemperatureOptimum => temperatureOptimum;
        internal float TemperatureResponseHalfRange => temperatureResponseHalfRange;
        internal float MoistureOptimum => moistureOptimum;
        internal float MoistureResponseHalfRange => moistureResponseHalfRange;
        internal float NutrientsForFullSuitability => nutrientsForFullSuitability;
        internal float HealthySuitabilityThreshold => healthySuitabilityThreshold;
        internal float HealthRecoveryRate => healthRecoveryRate;
        internal float HealthDeclineStressFloor => healthDeclineStressFloor;
        internal float LethalStressSensitivity => lethalStressSensitivity;
        internal float LethalDeathRate => lethalDeathRate;
        internal float StressDeathRate => stressDeathRate;
        internal float SpreadMinimumBiomass => spreadMinimumBiomass;
        internal float SpreadMinimumSuitability => spreadMinimumSuitability;
        internal float SpreadRandomMinimum => spreadRandomMinimum;
        internal float SpreadRandomMaximum => spreadRandomMaximum;

        public OrganismSimulationValues ToSimulationValues()
        {
            ValidateOrThrow();
            return new OrganismSimulationValues(this);
        }

        public void ValidateOrThrow()
        {
            DefinitionValidation.ValidateId(id, nameof(OrganismDefinition));
            DefinitionValidation.Require(definitionVersion >= 1, id, "Definition version must be at least 1.");
            RequireText(displayName, nameof(displayName));
            RequireText(scientificName, nameof(scientificName));
            RequireText(educationalDescription, nameof(educationalDescription));
            RequireText(scientificLabel, nameof(scientificLabel));
            RequireText(sourceNotes, nameof(sourceNotes));
            DefinitionValidation.Require(
                System.Enum.IsDefined(typeof(ScientificConfidence), scientificConfidence) &&
                scientificConfidence != ScientificConfidence.Unspecified,
                id,
                "Scientific confidence must be a supported explicit selection.");
            RequireText(simplificationNotes, nameof(simplificationNotes));
            DefinitionValidation.ValidateId(visualProfileId, "Organism visual profile");

            DefinitionValidation.Unit(initialHealth, id, nameof(initialHealth));
            DefinitionValidation.Range(seedRadiusCells, 0.1f, 24f, id, nameof(seedRadiusCells));
            DefinitionValidation.Unit(seedCenterBiomass, id, nameof(seedCenterBiomass));
            DefinitionValidation.Unit(seedEdgeBiomass, id, nameof(seedEdgeBiomass));
            DefinitionValidation.Require(
                seedCenterBiomass >= seedEdgeBiomass,
                id,
                "Seed centre biomass must be at least the edge biomass.");

            ValidateTemperatureRange(survivalTemperatureMinimum, survivalTemperatureMaximum, "survival");
            ValidateTemperatureRange(growthTemperatureMinimum, growthTemperatureMaximum, "growth");
            ValidateTemperatureRange(preferredTemperatureMinimum, preferredTemperatureMaximum, "preferred");
            DefinitionValidation.Require(
                survivalTemperatureMinimum <= growthTemperatureMinimum &&
                growthTemperatureMinimum <= preferredTemperatureMinimum &&
                preferredTemperatureMaximum <= growthTemperatureMaximum &&
                growthTemperatureMaximum <= survivalTemperatureMaximum,
                id,
                "Temperature ranges must nest as survival, growth, then preferred.");
            DefinitionValidation.Range(temperatureOptimum, 8f, 42f, id, nameof(temperatureOptimum));
            DefinitionValidation.Require(
                temperatureOptimum >= preferredTemperatureMinimum &&
                temperatureOptimum <= preferredTemperatureMaximum,
                id,
                "Temperature optimum must be inside the preferred range.");
            DefinitionValidation.Range(
                temperatureResponseHalfRange,
                0.001f,
                34f,
                id,
                nameof(temperatureResponseHalfRange));

            DefinitionValidation.Unit(preferredMoistureMinimum, id, nameof(preferredMoistureMinimum));
            DefinitionValidation.Unit(preferredMoistureMaximum, id, nameof(preferredMoistureMaximum));
            DefinitionValidation.Unit(growthMoistureThreshold, id, nameof(growthMoistureThreshold));
            DefinitionValidation.Unit(survivalMoistureThreshold, id, nameof(survivalMoistureThreshold));
            DefinitionValidation.Require(
                survivalMoistureThreshold <= growthMoistureThreshold &&
                growthMoistureThreshold <= preferredMoistureMinimum &&
                preferredMoistureMinimum <= preferredMoistureMaximum,
                id,
                "Moisture thresholds must progress from survival to growth to preferred.");
            DefinitionValidation.Unit(moistureOptimum, id, nameof(moistureOptimum));
            DefinitionValidation.Require(
                moistureOptimum >= preferredMoistureMinimum &&
                moistureOptimum <= preferredMoistureMaximum,
                id,
                "Moisture optimum must be inside the preferred range.");
            DefinitionValidation.UnitPositive(
                moistureResponseHalfRange,
                id,
                nameof(moistureResponseHalfRange));

            DefinitionValidation.UnitPositive(
                nutrientsForFullSuitability,
                id,
                nameof(nutrientsForFullSuitability));
            DefinitionValidation.Unit(healthySuitabilityThreshold, id, nameof(healthySuitabilityThreshold));
            DefinitionValidation.Unit(healthRecoveryRate, id, nameof(healthRecoveryRate));
            DefinitionValidation.Unit(damageSensitivity, id, nameof(damageSensitivity));
            DefinitionValidation.Unit(healthDeclineStressFloor, id, nameof(healthDeclineStressFloor));
            DefinitionValidation.Unit(stressRecovery, id, nameof(stressRecovery));
            DefinitionValidation.Unit(stressSensitivity, id, nameof(stressSensitivity));
            DefinitionValidation.Unit(lethalStressSensitivity, id, nameof(lethalStressSensitivity));
            DefinitionValidation.Require(
                lethalStressSensitivity >= stressSensitivity,
                id,
                "Lethal stress sensitivity must not be slower than normal stress sensitivity.");
            DefinitionValidation.Unit(baseGrowthRate, id, nameof(baseGrowthRate));
            DefinitionValidation.Unit(lethalDeathRate, id, nameof(lethalDeathRate));
            DefinitionValidation.Unit(stressDeathRate, id, nameof(stressDeathRate));
            DefinitionValidation.Unit(nutrientDemand, id, nameof(nutrientDemand));
            DefinitionValidation.Unit(moistureDemand, id, nameof(moistureDemand));
            DefinitionValidation.Range(carryingCapacity, 0.01f, 1f, id, nameof(carryingCapacity));
            DefinitionValidation.Unit(spreadMinimumBiomass, id, nameof(spreadMinimumBiomass));
            DefinitionValidation.Unit(spreadMinimumSuitability, id, nameof(spreadMinimumSuitability));
            DefinitionValidation.Unit(spreadRate, id, nameof(spreadRate));
            DefinitionValidation.Range(spreadRandomMinimum, 0f, 4f, id, nameof(spreadRandomMinimum));
            DefinitionValidation.Range(spreadRandomMaximum, 0f, 4f, id, nameof(spreadRandomMaximum));
            DefinitionValidation.Require(
                spreadRandomMaximum >= spreadRandomMinimum,
                id,
                "Spread random maximum must be at least the minimum.");
        }

        private void ValidateTemperatureRange(float minimum, float maximum, string label)
        {
            DefinitionValidation.Range(minimum, 8f, 42f, id, $"{label}TemperatureMinimum");
            DefinitionValidation.Range(maximum, 8f, 42f, id, $"{label}TemperatureMaximum");
            DefinitionValidation.Require(minimum <= maximum, id, $"{label} temperature range is malformed.");
        }

        private void RequireText(string value, string field)
        {
            DefinitionValidation.Require(!string.IsNullOrWhiteSpace(value), id, $"{field} is required.");
        }
    }

    public sealed class OrganismSimulationValues
    {
        public readonly string Id;
        public readonly int DefinitionVersion;
        public readonly float InitialHealth;
        public readonly float SeedRadiusCells;
        public readonly float SeedCenterBiomass;
        public readonly float SeedEdgeBiomass;
        public readonly float PreferredTemperature;
        public readonly float TemperatureHalfRange;
        public readonly float GrowthTemperatureMinimum;
        public readonly float GrowthTemperatureMaximum;
        public readonly float LethalTemperatureMinimum;
        public readonly float LethalTemperatureMaximum;
        public readonly float PreferredMoisture;
        public readonly float MoistureHalfRange;
        public readonly float GrowthMoistureThreshold;
        public readonly float LethalMoistureMinimum;
        public readonly float NutrientsForFullSuitability;
        public readonly float HealthySuitabilityThreshold;
        public readonly float HealthRecoveryRate;
        public readonly float HealthDeclineRate;
        public readonly float HealthDeclineStressFloor;
        public readonly float StressRecoveryRate;
        public readonly float StressSensitivity;
        public readonly float LethalStressResponseRate;
        public readonly float GrowthRate;
        public readonly float LethalDeathRate;
        public readonly float StressDeathRate;
        public readonly float NutrientConsumptionPerGrowth;
        public readonly float MoistureConsumptionPerGrowth;
        public readonly float CarryingCapacity;
        public readonly float SpreadMinimumBiomass;
        public readonly float SpreadMinimumSuitability;
        public readonly float SpreadRate;
        public readonly float SpreadRandomMinimum;
        public readonly float SpreadRandomMaximum;

        internal OrganismSimulationValues(OrganismDefinition source)
        {
            Id = source.Id;
            DefinitionVersion = source.DefinitionVersion;
            InitialHealth = source.InitialHealth;
            SeedRadiusCells = source.SeedRadiusCells;
            SeedCenterBiomass = source.SeedCenterBiomass;
            SeedEdgeBiomass = source.SeedEdgeBiomass;
            PreferredTemperature = source.TemperatureOptimum;
            TemperatureHalfRange = source.TemperatureResponseHalfRange;
            GrowthTemperatureMinimum = source.GrowthTemperatureMinimum;
            GrowthTemperatureMaximum = source.GrowthTemperatureMaximum;
            LethalTemperatureMinimum = source.SurvivalTemperatureMinimum;
            LethalTemperatureMaximum = source.SurvivalTemperatureMaximum;
            PreferredMoisture = source.MoistureOptimum;
            MoistureHalfRange = source.MoistureResponseHalfRange;
            GrowthMoistureThreshold = source.GrowthMoistureThreshold;
            LethalMoistureMinimum = source.SurvivalMoistureThreshold;
            NutrientsForFullSuitability = source.NutrientsForFullSuitability;
            HealthySuitabilityThreshold = source.HealthySuitabilityThreshold;
            HealthRecoveryRate = source.HealthRecoveryRate;
            HealthDeclineRate = source.DamageSensitivity;
            HealthDeclineStressFloor = source.HealthDeclineStressFloor;
            StressRecoveryRate = source.StressRecovery;
            StressSensitivity = source.StressSensitivity;
            LethalStressResponseRate = source.LethalStressSensitivity;
            GrowthRate = source.BaseGrowthRate;
            LethalDeathRate = source.LethalDeathRate;
            StressDeathRate = source.StressDeathRate;
            NutrientConsumptionPerGrowth = source.NutrientDemand;
            MoistureConsumptionPerGrowth = source.MoistureDemand;
            CarryingCapacity = source.CarryingCapacity;
            SpreadMinimumBiomass = source.SpreadMinimumBiomass;
            SpreadMinimumSuitability = source.SpreadMinimumSuitability;
            SpreadRate = source.SpreadRate;
            SpreadRandomMinimum = source.SpreadRandomMinimum;
            SpreadRandomMaximum = source.SpreadRandomMaximum;
        }
    }
}
