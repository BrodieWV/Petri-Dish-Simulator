using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using PetriDish.Application;
using PetriDish.Content;
using PetriDish.Simulation;
using UnityEditor;
using UnityEngine;

namespace PetriDish.Tests.Editor
{
    public sealed class SimulationDefinitionTests
    {
        private const float FloatTolerance = 0.000001f;
        private readonly List<UnityEngine.Object> temporaryObjects = new List<UnityEngine.Object>();
        private GameObject firstControllerObject;
        private GameObject secondControllerObject;
        private string savePath;

        [SetUp]
        public void SetUp()
        {
            savePath = Path.Combine(
                Path.GetTempPath(),
                $"petri-definition-test-{Guid.NewGuid():N}.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (firstControllerObject != null) UnityEngine.Object.DestroyImmediate(firstControllerObject);
            if (secondControllerObject != null) UnityEngine.Object.DestroyImmediate(secondControllerObject);
            for (int i = temporaryObjects.Count - 1; i >= 0; i--)
            {
                if (temporaryObjects[i] != null)
                    UnityEngine.Object.DestroyImmediate(temporaryObjects[i]);
            }
            temporaryObjects.Clear();
            DeleteIfPresent(savePath);
            DeleteIfPresent(savePath + ".bak");
            DeleteIfPresent(savePath + ".tmp");
        }

        [Test]
        public void DefaultDefinitionsPreservePreviousVerticalSliceBalance()
        {
            SimulationDefinitionCatalog catalog = SimulationDefinitionCatalog.LoadDefaultOrThrow();
            OrganismSimulationValues organism = catalog.DefaultOrganism.ToSimulationValues();
            MediumSimulationValues medium = catalog.DefaultMedium.ToSimulationValues();

            Assert.That(organism.Id, Is.EqualTo("rapid-bacterium"));
            Assert.That(organism.DefinitionVersion, Is.EqualTo(1));
            Assert.That(catalog.DefaultOrganism.ScientificName, Is.Not.Empty);
            Assert.That(catalog.DefaultOrganism.EducationalDescription, Is.Not.Empty);
            Assert.That(catalog.DefaultOrganism.SourceNotes, Is.Not.Empty);
            Assert.That(
                catalog.DefaultOrganism.Confidence,
                Is.EqualTo(ScientificConfidence.EducationalPlaceholder));
            Assert.That(catalog.DefaultOrganism.SimplificationNotes, Is.Not.Empty);
            Assert.That(catalog.DefaultOrganism.VisualProfileId, Is.EqualTo("rapid-bacterium-default"));
            Assert.That(catalog.DefaultOrganism.PreferredTemperatureMinimum, Is.EqualTo(24f));
            Assert.That(catalog.DefaultOrganism.PreferredTemperatureMaximum, Is.EqualTo(29f));
            Assert.That(catalog.DefaultOrganism.GrowthTemperatureMinimum, Is.EqualTo(18.5f));
            Assert.That(catalog.DefaultOrganism.GrowthTemperatureMaximum, Is.EqualTo(33.5f));
            Assert.That(catalog.DefaultOrganism.SurvivalTemperatureMinimum, Is.EqualTo(11f));
            Assert.That(catalog.DefaultOrganism.SurvivalTemperatureMaximum, Is.EqualTo(38f));
            Assert.That(catalog.DefaultOrganism.PreferredMoistureMinimum, Is.EqualTo(0.60f));
            Assert.That(catalog.DefaultOrganism.PreferredMoistureMaximum, Is.EqualTo(0.80f));
            Assert.That(catalog.DefaultOrganism.GrowthMoistureThreshold, Is.EqualTo(0.35f));
            Assert.That(catalog.DefaultOrganism.SurvivalMoistureThreshold, Is.EqualTo(0.16f));
            Assert.That(organism.InitialHealth, Is.EqualTo(1f));
            Assert.That(organism.SeedRadiusCells, Is.EqualTo(2.3f));
            Assert.That(organism.SeedCenterBiomass, Is.EqualTo(0.28f));
            Assert.That(organism.SeedEdgeBiomass, Is.EqualTo(0.08f));
            Assert.That(organism.PreferredTemperature, Is.EqualTo(26f));
            Assert.That(organism.TemperatureHalfRange, Is.EqualTo(7.5f));
            Assert.That(organism.LethalTemperatureMinimum, Is.EqualTo(11f));
            Assert.That(organism.LethalTemperatureMaximum, Is.EqualTo(38f));
            Assert.That(organism.PreferredMoisture, Is.EqualTo(0.70f));
            Assert.That(organism.MoistureHalfRange, Is.EqualTo(0.35f));
            Assert.That(organism.LethalMoistureMinimum, Is.EqualTo(0.16f));
            Assert.That(organism.NutrientsForFullSuitability, Is.EqualTo(0.25f));
            Assert.That(organism.HealthySuitabilityThreshold, Is.EqualTo(0.55f));
            Assert.That(organism.HealthRecoveryRate, Is.EqualTo(0.015f));
            Assert.That(organism.HealthDeclineRate, Is.EqualTo(0.018f));
            Assert.That(organism.GrowthRate, Is.EqualTo(0.07f));
            Assert.That(organism.HealthDeclineStressFloor, Is.EqualTo(0.35f));
            Assert.That(organism.StressRecoveryRate, Is.EqualTo(0.025f));
            Assert.That(organism.StressSensitivity, Is.EqualTo(0.025f));
            Assert.That(organism.LethalStressResponseRate, Is.EqualTo(0.06f));
            Assert.That(organism.LethalDeathRate, Is.EqualTo(0.035f));
            Assert.That(organism.StressDeathRate, Is.EqualTo(0.004f));
            Assert.That(organism.NutrientConsumptionPerGrowth, Is.EqualTo(0.42f));
            Assert.That(organism.MoistureConsumptionPerGrowth, Is.EqualTo(0.025f));
            Assert.That(organism.CarryingCapacity, Is.EqualTo(1f));
            Assert.That(organism.SpreadMinimumBiomass, Is.EqualTo(0.035f));
            Assert.That(organism.SpreadMinimumSuitability, Is.EqualTo(0.28f));
            Assert.That(organism.SpreadRate, Is.EqualTo(0.006f));
            Assert.That(organism.SpreadRandomMinimum, Is.EqualTo(0.75f));
            Assert.That(organism.SpreadRandomMaximum, Is.EqualTo(1.25f));

            Assert.That(medium.Id, Is.EqualTo("nutrient-agar"));
            Assert.That(medium.DefinitionVersion, Is.EqualTo(1));
            Assert.That(catalog.DefaultMedium.EducationalDescription, Is.Not.Empty);
            Assert.That(catalog.DefaultMedium.SourceNotes, Is.Not.Empty);
            Assert.That(
                catalog.DefaultMedium.Confidence,
                Is.EqualTo(ScientificConfidence.EducationalPlaceholder));
            Assert.That(catalog.DefaultMedium.SimplificationNotes, Is.Not.Empty);
            Assert.That(catalog.DefaultMedium.VisualProfileId, Is.EqualTo("nutrient-agar-default"));
            Assert.That(medium.InitialMoisture, Is.EqualTo(0.72f));
            Assert.That(medium.MaximumMoisture, Is.EqualTo(1f));
            Assert.That(medium.InitialNutrients, Is.EqualTo(1f));
            Assert.That(medium.MaximumNutrients, Is.EqualTo(1f));
            Assert.That(medium.MoistureAbsorptionMultiplier, Is.EqualTo(1f));
            Assert.That(medium.MoistureApplicationVariance, Is.EqualTo(0.15f));
            Assert.That(medium.TemperatureResponseRate, Is.EqualTo(0.18f));
            Assert.That(medium.MoistureDiffusion, Is.Zero);
            Assert.That(medium.NutrientDiffusion, Is.Zero);
            Assert.That(medium.SpreadResistance, Is.Zero);
            Assert.That(medium.EdgeEvaporation, Is.EqualTo(0.0017f).Within(FloatTolerance));
            Assert.That(medium.InteriorEvaporation, Is.EqualTo(0.00045f));
            Assert.That(medium.EdgeFalloffDepthCells, Is.EqualTo(12f));
            Assert.That(medium.HeatEvaporationStartTemperature, Is.EqualTo(24f));
            Assert.That(medium.HeatEvaporationPerDegree, Is.EqualTo(0.00012f));

            var simulation = new PetriSimulation(
                123,
                catalog.DefaultOrganism,
                catalog.DefaultMedium);
            SimulationSnapshot initial = simulation.CreateSnapshot();
            Assert.That(initial.AverageMoisture, Is.EqualTo(0.72f).Within(0.00002f));
            Assert.That(initial.AverageNutrients, Is.EqualTo(1f).Within(FloatTolerance));
        }

        [Test]
        public void DifferentOrganismDefinitionsProduceDifferentGrowth()
        {
            SimulationDefinitionCatalog catalog = SimulationDefinitionCatalog.LoadDefaultOrThrow();
            OrganismDefinition slower = CloneOrganism(
                catalog.DefaultOrganism,
                "slow-test-organism",
                growthRate: 0.012f);

            var rapid = new PetriSimulation(456, catalog.DefaultOrganism, catalog.DefaultMedium);
            var slow = new PetriSimulation(456, slower, catalog.DefaultMedium);
            rapid.SetTargetTemperature(26f);
            slow.SetTargetTemperature(26f);
            for (int i = 0; i < 80; i++)
            {
                rapid.Step();
                slow.Step();
            }

            float rapidBiomass = TotalBiomass(rapid.CreateSnapshot());
            float slowBiomass = TotalBiomass(slow.CreateSnapshot());
            Assert.That(rapidBiomass, Is.GreaterThan(slowBiomass * 1.25f));
        }

        [Test]
        public void DifferentMediumDefinitionsProduceDifferentMoistureBehaviour()
        {
            SimulationDefinitionCatalog catalog = SimulationDefinitionCatalog.LoadDefaultOrThrow();
            MediumDefinition dryMedium = CloneMedium(
                catalog.DefaultMedium,
                "fast-drying-test-medium",
                edgeEvaporation: 0.009f,
                interiorEvaporation: 0.004f);

            var nutrientAgar = new PetriSimulation(
                789,
                catalog.DefaultOrganism,
                catalog.DefaultMedium);
            var fastDrying = new PetriSimulation(789, catalog.DefaultOrganism, dryMedium);
            for (int i = 0; i < 40; i++)
            {
                nutrientAgar.Step();
                fastDrying.Step();
            }

            Assert.That(
                nutrientAgar.CreateMetrics().AverageMoisture,
                Is.GreaterThan(fastDrying.CreateMetrics().AverageMoisture + 0.08f));
        }

        [Test]
        public void CustomDefinitionsRemainDeterministicForSameSeedAndInputs()
        {
            SimulationDefinitionCatalog catalog = SimulationDefinitionCatalog.LoadDefaultOrThrow();
            OrganismDefinition organism = CloneOrganism(
                catalog.DefaultOrganism,
                "deterministic-test-organism",
                growthRate: 0.035f);
            MediumDefinition medium = CloneMedium(
                catalog.DefaultMedium,
                "deterministic-test-medium",
                edgeEvaporation: 0.003f,
                interiorEvaporation: 0.001f);
            var first = new PetriSimulation(98765, organism, medium);
            var second = new PetriSimulation(98765, organism, medium);

            first.SetTargetTemperature(28f);
            second.SetTargetTemperature(28f);
            for (int i = 0; i < 64; i++)
            {
                if (i == 24)
                {
                    first.AddMoisture(0.11f);
                    second.AddMoisture(0.11f);
                }
                first.Step();
                second.Step();
            }

            AssertSnapshotsEqual(first.CreateSnapshot(), second.CreateSnapshot());
        }

        [Test]
        public void SaveLoadPreservesSelectedDefinitionIdsAndExactContinuation()
        {
            SimulationDefinitionCatalog defaults = SimulationDefinitionCatalog.LoadDefaultOrThrow();
            OrganismDefinition organism = CloneOrganism(
                defaults.DefaultOrganism,
                "save-test-organism",
                growthRate: 0.031f);
            MediumDefinition medium = CloneMedium(
                defaults.DefaultMedium,
                "save-test-medium",
                edgeEvaporation: 0.0025f,
                interiorEvaporation: 0.0008f);
            SimulationDefinitionCatalog catalog = CreateCatalog(
                defaults.DefaultOrganism,
                defaults.DefaultMedium,
                new[] { defaults.DefaultOrganism, organism },
                new[] { defaults.DefaultMedium, medium });

            ExperimentController writer = CreateController(ref firstControllerObject);
            writer.ConfigureDefinitionCatalog(catalog);
            writer.StartNew(24680, organism.Id, medium.Id);
            writer.SetTemperature(27f);
            for (int i = 0; i < 30; i++) writer.Simulation.Step();
            Assert.That(writer.SaveToPath(savePath), Is.True, writer.LastPersistenceError);

            for (int i = 0; i < 20; i++) writer.Simulation.Step();
            SimulationSnapshot expected = writer.Simulation.CreateSnapshot();

            ExperimentController reader = CreateController(ref secondControllerObject);
            reader.ConfigureDefinitionCatalog(catalog);
            Assert.That(reader.LoadFromPath(savePath), Is.True, reader.LastPersistenceError);
            Assert.That(reader.Simulation.OrganismId, Is.EqualTo(organism.Id));
            Assert.That(reader.Simulation.MediumId, Is.EqualTo(medium.Id));
            for (int i = 0; i < 20; i++) reader.Simulation.Step();

            AssertSnapshotsEqual(expected, reader.Simulation.CreateSnapshot());
        }

        [Test]
        public void SameAndNewSeedRestartsPreserveSelectedDefinitions()
        {
            SimulationDefinitionCatalog defaults = SimulationDefinitionCatalog.LoadDefaultOrThrow();
            OrganismDefinition organism = CloneOrganism(
                defaults.DefaultOrganism,
                "restart-test-organism",
                growthRate: 0.031f);
            MediumDefinition medium = CloneMedium(
                defaults.DefaultMedium,
                "restart-test-medium",
                edgeEvaporation: 0.0025f,
                interiorEvaporation: 0.0008f);
            SimulationDefinitionCatalog catalog = CreateCatalog(
                defaults.DefaultOrganism,
                defaults.DefaultMedium,
                new[] { defaults.DefaultOrganism, organism },
                new[] { defaults.DefaultMedium, medium });

            ExperimentController controller = CreateController(ref firstControllerObject);
            controller.ConfigureDefinitionCatalog(catalog);
            controller.StartNew(45678, organism.Id, medium.Id);

            controller.RestartSameSeed();
            Assert.That(controller.Simulation.Seed, Is.EqualTo(45678));
            Assert.That(controller.Simulation.OrganismId, Is.EqualTo(organism.Id));
            Assert.That(controller.Simulation.MediumId, Is.EqualTo(medium.Id));

            controller.RestartNewSeed();
            Assert.That(controller.Simulation.OrganismId, Is.EqualTo(organism.Id));
            Assert.That(controller.Simulation.MediumId, Is.EqualTo(medium.Id));
        }

        [Test]
        public void SchemaVersionTwoExperimentMigratesToDefaultDefinitions()
        {
            ExperimentController writer = CreateController(ref firstControllerObject);
            writer.StartNew(13579);
            for (int i = 0; i < 20; i++) writer.Simulation.Step();
            Assert.That(writer.SaveToPath(savePath), Is.True, writer.LastPersistenceError);
            string json = File.ReadAllText(savePath);
            json = json.Replace("\"schemaVersion\": 3", "\"schemaVersion\": 2");
            File.WriteAllText(savePath, json);

            ExperimentController reader = CreateController(ref secondControllerObject);
            Assert.That(reader.LoadFromPath(savePath), Is.True, reader.LastPersistenceError);
            Assert.That(
                reader.Simulation.OrganismId,
                Is.EqualTo(SimulationDefinitionCatalog.RapidBacteriumId));
            Assert.That(
                reader.Simulation.MediumId,
                Is.EqualTo(SimulationDefinitionCatalog.NutrientAgarId));
        }

        [Test]
        public void SchemaVersionTwoSimulationRejectsChangedDefaultDefinitionVersion()
        {
            SimulationDefinitionCatalog defaults = SimulationDefinitionCatalog.LoadDefaultOrThrow();
            var original = new PetriSimulation(
                1122,
                defaults.DefaultOrganism,
                defaults.DefaultMedium);
            SimulationSaveData legacySave = original.CaptureSave();
            legacySave.schemaVersion = 2;
            legacySave.organismId = null;
            legacySave.organismDefinitionVersion = 0;
            legacySave.mediumId = null;
            legacySave.mediumDefinitionVersion = 0;

            OrganismDefinition changedOrganism = CloneOrganism(
                defaults.DefaultOrganism,
                defaults.DefaultOrganism.Id,
                growthRate: 0.07f);
            var serialized = new SerializedObject(changedOrganism);
            serialized.FindProperty("definitionVersion").intValue = 2;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var changed = new PetriSimulation(
                1122,
                changedOrganism,
                defaults.DefaultMedium);

            Assert.That(
                Assert.Throws<ArgumentException>(() => changed.Restore(legacySave)).Message,
                Does.Contain("definition-version-1"));
        }

        [Test]
        public void InvalidAndDuplicateDefinitionsAreRejected()
        {
            SimulationDefinitionCatalog defaults = SimulationDefinitionCatalog.LoadDefaultOrThrow();
            OrganismDefinition invalid = CloneOrganism(
                defaults.DefaultOrganism,
                "Invalid ID",
                growthRate: 0.02f);
            Assert.Throws<DefinitionValidationException>(() => invalid.ValidateOrThrow());

            OrganismDefinition unsupported = CloneOrganism(
                defaults.DefaultOrganism,
                "unsupported-test-organism",
                growthRate: -0.1f);
            Assert.That(
                Assert.Throws<DefinitionValidationException>(
                    () => unsupported.ValidateOrThrow()).Message,
                Does.Contain("baseGrowthRate"));

            OrganismDefinition malformedRange = CloneOrganism(
                defaults.DefaultOrganism,
                "malformed-range-organism",
                growthRate: 0.02f);
            var malformedSerialized = new SerializedObject(malformedRange);
            malformedSerialized.FindProperty("growthTemperatureMinimum").floatValue = 34f;
            malformedSerialized.FindProperty("growthTemperatureMaximum").floatValue = 20f;
            malformedSerialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(
                Assert.Throws<DefinitionValidationException>(
                    () => malformedRange.ValidateOrThrow()).Message,
                Does.Contain("malformed"));

            OrganismDefinition nonFinite = CloneOrganism(
                defaults.DefaultOrganism,
                "non-finite-organism",
                growthRate: float.NaN);
            Assert.That(
                Assert.Throws<DefinitionValidationException>(
                    () => nonFinite.ValidateOrThrow()).Message,
                Does.Contain("finite"));

            OrganismDefinition duplicate = CloneOrganism(
                defaults.DefaultOrganism,
                defaults.DefaultOrganism.Id,
                growthRate: 0.02f);
            SimulationDefinitionCatalog catalog = CreateCatalog(
                defaults.DefaultOrganism,
                defaults.DefaultMedium,
                new[] { defaults.DefaultOrganism, duplicate },
                new[] { defaults.DefaultMedium });
            Assert.That(
                Assert.Throws<DefinitionValidationException>(() => catalog.ValidateOrThrow()).Message,
                Does.Contain("Duplicate organism ID"));
        }

        [Test]
        public void MissingScientificMetadataIsRejected()
        {
            SimulationDefinitionCatalog defaults = SimulationDefinitionCatalog.LoadDefaultOrThrow();
            OrganismDefinition missingSource = CloneOrganism(
                defaults.DefaultOrganism,
                "missing-source-organism",
                growthRate: 0.02f);
            var missingSourceSerialized = new SerializedObject(missingSource);
            missingSourceSerialized.FindProperty("sourceNotes").stringValue = string.Empty;
            missingSourceSerialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(
                Assert.Throws<DefinitionValidationException>(
                    () => missingSource.ValidateOrThrow()).Message,
                Does.Contain("sourceNotes"));

            OrganismDefinition unspecifiedConfidence = CloneOrganism(
                defaults.DefaultOrganism,
                "unspecified-confidence-organism",
                growthRate: 0.02f);
            var confidenceSerialized = new SerializedObject(unspecifiedConfidence);
            confidenceSerialized.FindProperty("scientificConfidence").enumValueIndex =
                (int)ScientificConfidence.Unspecified;
            confidenceSerialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(
                Assert.Throws<DefinitionValidationException>(
                    () => unspecifiedConfidence.ValidateOrThrow()).Message,
                Does.Contain("confidence"));

            OrganismDefinition unsupportedConfidence = CloneOrganism(
                defaults.DefaultOrganism,
                "unsupported-confidence-organism",
                growthRate: 0.02f);
            var unsupportedConfidenceSerialized = new SerializedObject(unsupportedConfidence);
            unsupportedConfidenceSerialized.FindProperty("scientificConfidence").intValue = 99;
            unsupportedConfidenceSerialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(
                Assert.Throws<DefinitionValidationException>(
                    () => unsupportedConfidence.ValidateOrThrow()).Message,
                Does.Contain("confidence"));

            MediumDefinition missingSimplification = CloneMedium(
                defaults.DefaultMedium,
                "missing-simplification-medium",
                edgeEvaporation: 0.002f,
                interiorEvaporation: 0.001f);
            var simplificationSerialized = new SerializedObject(missingSimplification);
            simplificationSerialized.FindProperty("simplificationNotes").stringValue = string.Empty;
            simplificationSerialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(
                Assert.Throws<DefinitionValidationException>(
                    () => missingSimplification.ValidateOrThrow()).Message,
                Does.Contain("simplificationNotes"));
        }

        [Test]
        public void MissingSavedDefinitionFailsWithControlledMessage()
        {
            ExperimentController writer = CreateController(ref firstControllerObject);
            writer.StartNew(86420);
            Assert.That(writer.SaveToPath(savePath), Is.True, writer.LastPersistenceError);
            string json = File.ReadAllText(savePath).Replace(
                SimulationDefinitionCatalog.RapidBacteriumId,
                "missing-organism");
            File.WriteAllText(savePath, json);

            ExperimentController reader = CreateController(ref secondControllerObject);
            reader.StartNew(97531);
            PetriSimulation runningSimulation = reader.Simulation;
            Assert.That(reader.LoadFromPath(savePath), Is.False);
            Assert.That(reader.LastPersistenceError, Does.Contain("missing-organism"));
            Assert.That(reader.Simulation, Is.SameAs(runningSimulation));
        }

        private OrganismDefinition CloneOrganism(
            OrganismDefinition source,
            string id,
            float growthRate)
        {
            OrganismDefinition clone = UnityEngine.Object.Instantiate(source);
            temporaryObjects.Add(clone);
            var serialized = new SerializedObject(clone);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("baseGrowthRate").floatValue = growthRate;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return clone;
        }

        private MediumDefinition CloneMedium(
            MediumDefinition source,
            string id,
            float edgeEvaporation,
            float interiorEvaporation)
        {
            MediumDefinition clone = UnityEngine.Object.Instantiate(source);
            temporaryObjects.Add(clone);
            var serialized = new SerializedObject(clone);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("evaporationRate").floatValue = interiorEvaporation;
            serialized.FindProperty("edgeDryingMultiplier").floatValue =
                edgeEvaporation / interiorEvaporation;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return clone;
        }

        private SimulationDefinitionCatalog CreateCatalog(
            OrganismDefinition defaultOrganism,
            MediumDefinition defaultMedium,
            OrganismDefinition[] organisms,
            MediumDefinition[] media)
        {
            var catalog = ScriptableObject.CreateInstance<SimulationDefinitionCatalog>();
            temporaryObjects.Add(catalog);
            var serialized = new SerializedObject(catalog);
            serialized.FindProperty("defaultOrganism").objectReferenceValue = defaultOrganism;
            serialized.FindProperty("defaultMedium").objectReferenceValue = defaultMedium;
            SetObjectArray(serialized.FindProperty("organisms"), organisms);
            SetObjectArray(serialized.FindProperty("media"), media);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return catalog;
        }

        private static void SetObjectArray<T>(SerializedProperty property, T[] values)
            where T : UnityEngine.Object
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        private static ExperimentController CreateController(ref GameObject owner)
        {
            owner = new GameObject("DefinitionExperimentControllerTest");
            return owner.AddComponent<ExperimentController>();
        }

        private static float TotalBiomass(SimulationSnapshot snapshot)
        {
            float total = 0f;
            for (int i = 0; i < snapshot.Biomass.Length; i++) total += snapshot.Biomass[i];
            return total;
        }

        private static void AssertSnapshotsEqual(
            SimulationSnapshot expected,
            SimulationSnapshot actual)
        {
            Assert.That(actual.Tick, Is.EqualTo(expected.Tick));
            Assert.That(actual.Temperature, Is.EqualTo(expected.Temperature).Within(FloatTolerance));
            Assert.That(actual.Coverage, Is.EqualTo(expected.Coverage).Within(FloatTolerance));
            Assert.That(actual.AverageHealth, Is.EqualTo(expected.AverageHealth).Within(FloatTolerance));
            Assert.That(actual.AverageMoisture, Is.EqualTo(expected.AverageMoisture).Within(FloatTolerance));
            Assert.That(actual.AverageNutrients, Is.EqualTo(expected.AverageNutrients).Within(FloatTolerance));
            AssertArraysEqual(expected.Biomass, actual.Biomass);
            AssertArraysEqual(expected.Health, actual.Health);
            AssertArraysEqual(expected.Moisture, actual.Moisture);
            AssertArraysEqual(expected.Nutrients, actual.Nutrients);
        }

        private static void AssertArraysEqual(float[] expected, float[] actual)
        {
            Assert.That(actual.Length, Is.EqualTo(expected.Length));
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(
                    actual[i],
                    Is.EqualTo(expected[i]).Within(FloatTolerance),
                    $"Mismatch at cell {i}");
            }
        }

        private static void DeleteIfPresent(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
