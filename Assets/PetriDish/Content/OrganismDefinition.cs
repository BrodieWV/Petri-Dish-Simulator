using System;
using UnityEngine;

namespace PetriDish.Content
{
    [CreateAssetMenu(
        fileName = "OrganismDefinition",
        menuName = "Petri Dish/Definitions/Organism")]
    public sealed class OrganismDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField, Min(1)] private int definitionVersion = 1;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string scientificLabel;

        [Header("Starting colony")]
        [SerializeField, Range(0f, 1f)] private float initialHealth = 1f;
        [SerializeField, Min(0.1f)] private float seedRadiusCells = 2.3f;
        [SerializeField, Range(0f, 1f)] private float seedCenterBiomass = 0.28f;
        [SerializeField, Range(0f, 1f)] private float seedEdgeBiomass = 0.08f;

        [Header("Suitability")]
        [SerializeField] private float preferredTemperature = 26f;
        [SerializeField, Min(0.001f)] private float temperatureHalfRange = 7.5f;
        [SerializeField] private float lethalTemperatureMinimum = 11f;
        [SerializeField] private float lethalTemperatureMaximum = 38f;
        [SerializeField, Range(0f, 1f)] private float preferredMoisture = 0.70f;
        [SerializeField, Min(0.001f)] private float moistureHalfRange = 0.35f;
        [SerializeField, Range(0f, 1f)] private float lethalMoistureMinimum = 0.16f;
        [SerializeField, Range(0.001f, 1f)] private float nutrientsForFullSuitability = 0.25f;

        [Header("Growth and stress per fixed step")]
        [SerializeField, Range(0f, 1f)] private float healthySuitabilityThreshold = 0.55f;
        [SerializeField, Range(0f, 1f)] private float healthRecoveryRate = 0.015f;
        [SerializeField, Range(0f, 1f)] private float healthDeclineRate = 0.018f;
        [SerializeField, Range(0f, 1f)] private float healthDeclineStressFloor = 0.35f;
        [SerializeField, Range(0f, 1f)] private float normalStressResponseRate = 0.025f;
        [SerializeField, Range(0f, 1f)] private float lethalStressResponseRate = 0.06f;
        [SerializeField, Range(0f, 1f)] private float growthRate = 0.07f;
        [SerializeField, Range(0f, 1f)] private float lethalDeathRate = 0.035f;
        [SerializeField, Range(0f, 1f)] private float stressDeathRate = 0.004f;
        [SerializeField, Range(0f, 1f)] private float nutrientConsumptionPerGrowth = 0.42f;
        [SerializeField, Range(0f, 1f)] private float moistureConsumptionPerGrowth = 0.025f;

        [Header("Spread")]
        [SerializeField, Range(0f, 1f)] private float spreadMinimumBiomass = 0.035f;
        [SerializeField, Range(0f, 1f)] private float spreadMinimumSuitability = 0.28f;
        [SerializeField, Range(0f, 1f)] private float spreadRate = 0.006f;
        [SerializeField, Min(0f)] private float spreadRandomMinimum = 0.75f;
        [SerializeField, Min(0f)] private float spreadRandomMaximum = 1.25f;

        public string Id => id;
        public int DefinitionVersion => definitionVersion;
        public string DisplayName => displayName;
        public string ScientificLabel => scientificLabel;

        public OrganismSimulationValues ToSimulationValues()
        {
            ValidateOrThrow();
            return new OrganismSimulationValues(
                id,
                definitionVersion,
                initialHealth,
                seedRadiusCells,
                seedCenterBiomass,
                seedEdgeBiomass,
                preferredTemperature,
                temperatureHalfRange,
                lethalTemperatureMinimum,
                lethalTemperatureMaximum,
                preferredMoisture,
                moistureHalfRange,
                lethalMoistureMinimum,
                nutrientsForFullSuitability,
                healthySuitabilityThreshold,
                healthRecoveryRate,
                healthDeclineRate,
                healthDeclineStressFloor,
                normalStressResponseRate,
                lethalStressResponseRate,
                growthRate,
                lethalDeathRate,
                stressDeathRate,
                nutrientConsumptionPerGrowth,
                moistureConsumptionPerGrowth,
                spreadMinimumBiomass,
                spreadMinimumSuitability,
                spreadRate,
                spreadRandomMinimum,
                spreadRandomMaximum);
        }

        public void ValidateOrThrow()
        {
            DefinitionValidation.ValidateId(id, nameof(OrganismDefinition));
            DefinitionValidation.Require(definitionVersion >= 1, id, "Definition version must be at least 1.");
            DefinitionValidation.Require(!string.IsNullOrWhiteSpace(displayName), id, "Display name is required.");
            DefinitionValidation.Require(
                !string.IsNullOrWhiteSpace(scientificLabel),
                id,
                "A scientific or simplification label is required.");
            DefinitionValidation.Unit(initialHealth, id, nameof(initialHealth));
            DefinitionValidation.Range(seedRadiusCells, 0.1f, 24f, id, nameof(seedRadiusCells));
            DefinitionValidation.Unit(seedCenterBiomass, id, nameof(seedCenterBiomass));
            DefinitionValidation.Unit(seedEdgeBiomass, id, nameof(seedEdgeBiomass));
            DefinitionValidation.Require(
                seedCenterBiomass >= seedEdgeBiomass,
                id,
                "Seed centre biomass must be at least the edge biomass.");
            DefinitionValidation.Range(preferredTemperature, 8f, 42f, id, nameof(preferredTemperature));
            DefinitionValidation.Range(temperatureHalfRange, 0.001f, 34f, id, nameof(temperatureHalfRange));
            DefinitionValidation.Range(
                lethalTemperatureMinimum,
                8f,
                42f,
                id,
                nameof(lethalTemperatureMinimum));
            DefinitionValidation.Range(
                lethalTemperatureMaximum,
                8f,
                42f,
                id,
                nameof(lethalTemperatureMaximum));
            DefinitionValidation.Require(
                lethalTemperatureMinimum < preferredTemperature &&
                preferredTemperature < lethalTemperatureMaximum,
                id,
                "Preferred temperature must be inside the lethal temperature bounds.");
            DefinitionValidation.Unit(preferredMoisture, id, nameof(preferredMoisture));
            DefinitionValidation.Positive(moistureHalfRange, id, nameof(moistureHalfRange));
            DefinitionValidation.Unit(lethalMoistureMinimum, id, nameof(lethalMoistureMinimum));
            DefinitionValidation.Require(
                lethalMoistureMinimum < preferredMoisture,
                id,
                "Preferred moisture must exceed the lethal moisture minimum.");
            DefinitionValidation.UnitPositive(
                nutrientsForFullSuitability,
                id,
                nameof(nutrientsForFullSuitability));
            DefinitionValidation.Unit(healthySuitabilityThreshold, id, nameof(healthySuitabilityThreshold));
            DefinitionValidation.Unit(healthRecoveryRate, id, nameof(healthRecoveryRate));
            DefinitionValidation.Unit(healthDeclineRate, id, nameof(healthDeclineRate));
            DefinitionValidation.Unit(healthDeclineStressFloor, id, nameof(healthDeclineStressFloor));
            DefinitionValidation.Unit(normalStressResponseRate, id, nameof(normalStressResponseRate));
            DefinitionValidation.Unit(lethalStressResponseRate, id, nameof(lethalStressResponseRate));
            DefinitionValidation.Require(
                lethalStressResponseRate >= normalStressResponseRate,
                id,
                "Lethal stress response must not be slower than the normal response.");
            DefinitionValidation.Unit(growthRate, id, nameof(growthRate));
            DefinitionValidation.Unit(lethalDeathRate, id, nameof(lethalDeathRate));
            DefinitionValidation.Unit(stressDeathRate, id, nameof(stressDeathRate));
            DefinitionValidation.Unit(nutrientConsumptionPerGrowth, id, nameof(nutrientConsumptionPerGrowth));
            DefinitionValidation.Unit(moistureConsumptionPerGrowth, id, nameof(moistureConsumptionPerGrowth));
            DefinitionValidation.Unit(spreadMinimumBiomass, id, nameof(spreadMinimumBiomass));
            DefinitionValidation.Unit(spreadMinimumSuitability, id, nameof(spreadMinimumSuitability));
            DefinitionValidation.Unit(spreadRate, id, nameof(spreadRate));
            DefinitionValidation.NonNegative(spreadRandomMinimum, id, nameof(spreadRandomMinimum));
            DefinitionValidation.NonNegative(spreadRandomMaximum, id, nameof(spreadRandomMaximum));
            DefinitionValidation.Require(
                spreadRandomMaximum <= 4f,
                id,
                "Spread random maximum cannot exceed 4.");
            DefinitionValidation.Require(
                spreadRandomMaximum >= spreadRandomMinimum,
                id,
                "Spread random maximum must be at least the minimum.");
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
        public readonly float LethalTemperatureMinimum;
        public readonly float LethalTemperatureMaximum;
        public readonly float PreferredMoisture;
        public readonly float MoistureHalfRange;
        public readonly float LethalMoistureMinimum;
        public readonly float NutrientsForFullSuitability;
        public readonly float HealthySuitabilityThreshold;
        public readonly float HealthRecoveryRate;
        public readonly float HealthDeclineRate;
        public readonly float HealthDeclineStressFloor;
        public readonly float NormalStressResponseRate;
        public readonly float LethalStressResponseRate;
        public readonly float GrowthRate;
        public readonly float LethalDeathRate;
        public readonly float StressDeathRate;
        public readonly float NutrientConsumptionPerGrowth;
        public readonly float MoistureConsumptionPerGrowth;
        public readonly float SpreadMinimumBiomass;
        public readonly float SpreadMinimumSuitability;
        public readonly float SpreadRate;
        public readonly float SpreadRandomMinimum;
        public readonly float SpreadRandomMaximum;

        internal OrganismSimulationValues(
            string id,
            int definitionVersion,
            float initialHealth,
            float seedRadiusCells,
            float seedCenterBiomass,
            float seedEdgeBiomass,
            float preferredTemperature,
            float temperatureHalfRange,
            float lethalTemperatureMinimum,
            float lethalTemperatureMaximum,
            float preferredMoisture,
            float moistureHalfRange,
            float lethalMoistureMinimum,
            float nutrientsForFullSuitability,
            float healthySuitabilityThreshold,
            float healthRecoveryRate,
            float healthDeclineRate,
            float healthDeclineStressFloor,
            float normalStressResponseRate,
            float lethalStressResponseRate,
            float growthRate,
            float lethalDeathRate,
            float stressDeathRate,
            float nutrientConsumptionPerGrowth,
            float moistureConsumptionPerGrowth,
            float spreadMinimumBiomass,
            float spreadMinimumSuitability,
            float spreadRate,
            float spreadRandomMinimum,
            float spreadRandomMaximum)
        {
            Id = id;
            DefinitionVersion = definitionVersion;
            InitialHealth = initialHealth;
            SeedRadiusCells = seedRadiusCells;
            SeedCenterBiomass = seedCenterBiomass;
            SeedEdgeBiomass = seedEdgeBiomass;
            PreferredTemperature = preferredTemperature;
            TemperatureHalfRange = temperatureHalfRange;
            LethalTemperatureMinimum = lethalTemperatureMinimum;
            LethalTemperatureMaximum = lethalTemperatureMaximum;
            PreferredMoisture = preferredMoisture;
            MoistureHalfRange = moistureHalfRange;
            LethalMoistureMinimum = lethalMoistureMinimum;
            NutrientsForFullSuitability = nutrientsForFullSuitability;
            HealthySuitabilityThreshold = healthySuitabilityThreshold;
            HealthRecoveryRate = healthRecoveryRate;
            HealthDeclineRate = healthDeclineRate;
            HealthDeclineStressFloor = healthDeclineStressFloor;
            NormalStressResponseRate = normalStressResponseRate;
            LethalStressResponseRate = lethalStressResponseRate;
            GrowthRate = growthRate;
            LethalDeathRate = lethalDeathRate;
            StressDeathRate = stressDeathRate;
            NutrientConsumptionPerGrowth = nutrientConsumptionPerGrowth;
            MoistureConsumptionPerGrowth = moistureConsumptionPerGrowth;
            SpreadMinimumBiomass = spreadMinimumBiomass;
            SpreadMinimumSuitability = spreadMinimumSuitability;
            SpreadRate = spreadRate;
            SpreadRandomMinimum = spreadRandomMinimum;
            SpreadRandomMaximum = spreadRandomMaximum;
        }
    }
}
