using System;
using System.IO;
using NUnit.Framework;
using PetriDish.Application;
using PetriDish.Content;
using PetriDish.Simulation;
using UnityEngine;

namespace PetriDish.Tests.Editor
{
    public sealed class NutrientInterventionTests
    {
        private const float FloatTolerance = 0.000001f;
        private GameObject firstObject;
        private GameObject secondObject;
        private string savePath;

        [SetUp]
        public void SetUp()
        {
            savePath = Path.Combine(
                Path.GetTempPath(),
                $"petri-nutrient-test-{Guid.NewGuid():N}.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (firstObject != null) UnityEngine.Object.DestroyImmediate(firstObject);
            if (secondObject != null) UnityEngine.Object.DestroyImmediate(secondObject);
            DeleteIfPresent(savePath);
            DeleteIfPresent(savePath + ".bak");
            DeleteIfPresent(savePath + ".tmp");
        }

        [Test]
        public void DoseUsesApprovedDelayReleaseCooldownAndFiniteSupply()
        {
            ExperimentController controller = CreateController(ref firstObject);
            controller.StartNew(12345);

            Assert.That(controller.TryRequestNutrientDose(out _), Is.True);
            Assert.That(controller.NutrientDosesRemaining, Is.EqualTo(2));
            Assert.That(controller.NutrientHistory, Has.Count.EqualTo(1));
            Assert.That(controller.NutrientHistory[0].RequestTick, Is.Zero);
            Assert.That(controller.NutrientHistory[0].RequestedAmount, Is.EqualTo(0.12f));

            AdvanceSteps(controller, 3);
            Assert.That(controller.NutrientHistory[0].DeliveryStartTick, Is.EqualTo(-1));
            AdvanceSteps(controller, 1);
            Assert.That(controller.NutrientHistory[0].DeliveryStartTick, Is.EqualTo(4));
            Assert.That(controller.NutrientReleaseStepsCompleted, Is.EqualTo(1));

            AdvanceSteps(controller, 11);
            Assert.That(controller.NutrientDeliveryPending, Is.False);
            Assert.That(controller.NutrientHistory[0].CompletionTick, Is.EqualTo(15));
            Assert.That(controller.NutrientCooldownRemainingSteps, Is.EqualTo(5));
            Assert.That(controller.TryRequestNutrientDose(out string cooldownFeedback), Is.False);
            Assert.That(cooldownFeedback, Does.Contain("ready in"));

            AdvanceSteps(controller, 5);
            Assert.That(controller.TryRequestNutrientDose(out _), Is.True);
            AdvanceSteps(controller, 20);
            Assert.That(controller.TryRequestNutrientDose(out _), Is.True);
            Assert.That(controller.NutrientDosesRemaining, Is.Zero);
            Assert.That(controller.TryRequestNutrientDose(out string emptyFeedback), Is.False);
            Assert.That(emptyFeedback, Does.Contain("No nutrient doses"));
        }

        [TestCase(2)]
        [TestCase(8)]
        public void SaveLoadDuringPendingDoseContinuesExactly(int stepsBeforeSave)
        {
            ExperimentController uninterrupted = CreateController(ref firstObject);
            uninterrupted.StartNew(
                24680,
                SimulationDefinitionCatalog.RapidBacteriumId,
                SimulationDefinitionCatalog.LowNutrientAgarId);
            Assert.That(uninterrupted.TryRequestNutrientDose(out _), Is.True);
            AdvanceSteps(uninterrupted, stepsBeforeSave);
            Assert.That(uninterrupted.SaveToPath(savePath), Is.True, uninterrupted.LastPersistenceError);

            AdvanceSteps(uninterrupted, 24);
            SimulationSnapshot expected = uninterrupted.Simulation.CreateSnapshot();

            ExperimentController resumed = CreateController(ref secondObject);
            Assert.That(resumed.LoadFromPath(savePath), Is.True, resumed.LastPersistenceError);
            AdvanceSteps(resumed, 24);

            AssertSnapshotsEqual(expected, resumed.Simulation.CreateSnapshot());
            AssertHistoriesEqual(uninterrupted, resumed);
            Assert.That(resumed.NutrientDosesRemaining, Is.EqualTo(uninterrupted.NutrientDosesRemaining));
            Assert.That(
                resumed.NutrientCooldownRemainingSteps,
                Is.EqualTo(uninterrupted.NutrientCooldownRemainingSteps));
        }

        [Test]
        public void SameSeedAndDoseScheduleRemainDeterministic()
        {
            ExperimentController first = CreateController(ref firstObject);
            ExperimentController second = CreateController(ref secondObject);
            first.StartNew(98765);
            second.StartNew(98765);

            Assert.That(first.TryRequestNutrientDose(out _), Is.True);
            Assert.That(second.TryRequestNutrientDose(out _), Is.True);
            AdvanceSteps(first, 25);
            AdvanceSteps(second, 25);
            Assert.That(first.TryRequestNutrientDose(out _), Is.True);
            Assert.That(second.TryRequestNutrientDose(out _), Is.True);
            AdvanceSteps(first, 18);
            AdvanceSteps(second, 18);

            AssertSnapshotsEqual(first.Simulation.CreateSnapshot(), second.Simulation.CreateSnapshot());
            AssertHistoriesEqual(first, second);
        }

        [Test]
        public void SchemaThreeExperimentMigratesToFullUnusedNutrientSupply()
        {
            ExperimentController writer = CreateController(ref firstObject);
            writer.StartNew(35791);
            Assert.That(writer.TryRequestNutrientDose(out _), Is.True);
            AdvanceSteps(writer, 7);
            Assert.That(writer.SaveToPath(savePath), Is.True, writer.LastPersistenceError);
            string json = File.ReadAllText(savePath).Replace(
                "\"schemaVersion\": 4",
                "\"schemaVersion\": 3");
            File.WriteAllText(savePath, json);

            ExperimentController reader = CreateController(ref secondObject);
            Assert.That(reader.LoadFromPath(savePath), Is.True, reader.LastPersistenceError);
            Assert.That(ExperimentController.CurrentExperimentSaveSchemaVersion, Is.EqualTo(4));
            Assert.That(reader.Simulation.CaptureSave().schemaVersion, Is.EqualTo(3));
            Assert.That(reader.NutrientDosesRemaining, Is.EqualTo(3));
            Assert.That(reader.NutrientDeliveryPending, Is.False);
            Assert.That(reader.NutrientHistory, Is.Empty);
            Assert.That(reader.NutrientCooldownRemainingSteps, Is.Zero);
            Assert.That(reader.SaveToPath(savePath), Is.True, reader.LastPersistenceError);
            Assert.That(writer.LoadFromPath(savePath), Is.True, writer.LastPersistenceError);
            Assert.That(writer.NutrientDosesRemaining, Is.EqualTo(3));
            Assert.That(writer.NutrientHistory, Is.Empty);
        }

        [Test]
        public void MalformedSchemaFourNutrientStateDoesNotReplaceRunningExperiment()
        {
            ExperimentController writer = CreateController(ref firstObject);
            writer.StartNew(11111);
            Assert.That(writer.SaveToPath(savePath), Is.True, writer.LastPersistenceError);
            string json = File.ReadAllText(savePath).Replace(
                "\"nutrientDosesRemaining\": 3",
                "\"nutrientDosesRemaining\": 99");
            File.WriteAllText(savePath, json);

            ExperimentController reader = CreateController(ref secondObject);
            reader.StartNew(22222);
            PetriSimulation running = reader.Simulation;
            Assert.That(reader.LoadFromPath(savePath), Is.False);
            Assert.That(reader.Simulation, Is.SameAs(running));
            Assert.That(reader.Simulation.Seed, Is.EqualTo(22222));
            Assert.That(reader.LastPersistenceError, Does.Contain("nutrient supply"));
        }

        [Test]
        public void RestartResetsSupplyCooldownPendingDeliveryAndHistory()
        {
            ExperimentController controller = CreateController(ref firstObject);
            controller.StartNew(44444);
            Assert.That(controller.TryRequestNutrientDose(out _), Is.True);
            AdvanceSteps(controller, 8);

            controller.RestartSameSeed();

            Assert.That(controller.Simulation.Seed, Is.EqualTo(44444));
            Assert.That(controller.NutrientDosesRemaining, Is.EqualTo(3));
            Assert.That(controller.NutrientCooldownRemainingSteps, Is.Zero);
            Assert.That(controller.NutrientDeliveryPending, Is.False);
            Assert.That(controller.NutrientHistory, Is.Empty);
        }

        [Test]
        public void SimulationNutrientApplicationReportsCapacityLimitedDelivery()
        {
            SimulationDefinitionCatalog catalog = SimulationDefinitionCatalog.LoadDefaultOrThrow();
            MediumDefinition lowNutrient = catalog.ResolveMedium(
                SimulationDefinitionCatalog.LowNutrientAgarId);
            var simulation = new PetriSimulation(54321, catalog.DefaultOrganism, lowNutrient);

            float delivered = simulation.AddNutrients(0.12f);
            SimulationSnapshot snapshot = simulation.CreateSnapshot();

            Assert.That(delivered, Is.EqualTo(0.10f).Within(0.00002f));
            Assert.That(snapshot.AverageNutrients, Is.EqualTo(0.55f).Within(0.00002f));
            Assert.That(simulation.AddNutrients(0.01f), Is.Zero.Within(FloatTolerance));
        }

        [Test]
        public void ProductionMediaRetainDistinctNutrientStatesAfterEqualDoses()
        {
            ExperimentController nutrientAgar = CreateController(ref firstObject);
            ExperimentController lowNutrientAgar = CreateController(ref secondObject);
            nutrientAgar.StartNew(
                60606,
                SimulationDefinitionCatalog.RapidBacteriumId,
                SimulationDefinitionCatalog.NutrientAgarId);
            lowNutrientAgar.StartNew(
                60606,
                SimulationDefinitionCatalog.RapidBacteriumId,
                SimulationDefinitionCatalog.LowNutrientAgarId);
            nutrientAgar.SetTemperature(26f);
            lowNutrientAgar.SetTemperature(26f);
            AdvanceSteps(nutrientAgar, 80);
            AdvanceSteps(lowNutrientAgar, 80);

            Assert.That(nutrientAgar.TryRequestNutrientDose(out _), Is.True);
            Assert.That(lowNutrientAgar.TryRequestNutrientDose(out _), Is.True);
            AdvanceSteps(nutrientAgar, 16);
            AdvanceSteps(lowNutrientAgar, 16);

            SimulationSnapshot rich = nutrientAgar.Simulation.CreateSnapshot();
            SimulationSnapshot limited = lowNutrientAgar.Simulation.CreateSnapshot();
            Assert.That(
                rich.AverageNutrients,
                Is.GreaterThan(limited.AverageNutrients + 0.30f),
                "The same dose must respect each medium's distinct nutrient capacity.");
            Assert.That(nutrientAgar.NutrientHistory[0].IsComplete, Is.True);
            Assert.That(lowNutrientAgar.NutrientHistory[0].IsComplete, Is.True);
        }

        private static ExperimentController CreateController(ref GameObject owner)
        {
            owner = new GameObject("NutrientInterventionTest");
            return owner.AddComponent<ExperimentController>();
        }

        private static void AdvanceSteps(ExperimentController controller, int count)
        {
            for (int i = 0; i < count; i++)
                Assert.That(controller.AdvanceSimulation(PetriSimulation.FixedStepSeconds), Is.True);
        }

        private static void AssertHistoriesEqual(
            ExperimentController expected,
            ExperimentController actual)
        {
            Assert.That(actual.NutrientHistory, Has.Count.EqualTo(expected.NutrientHistory.Count));
            for (int i = 0; i < expected.NutrientHistory.Count; i++)
            {
                NutrientInterventionRecord expectedRecord = expected.NutrientHistory[i];
                NutrientInterventionRecord actualRecord = actual.NutrientHistory[i];
                Assert.That(actualRecord.RequestTick, Is.EqualTo(expectedRecord.RequestTick));
                Assert.That(actualRecord.DeliveryStartTick, Is.EqualTo(expectedRecord.DeliveryStartTick));
                Assert.That(actualRecord.CompletionTick, Is.EqualTo(expectedRecord.CompletionTick));
                Assert.That(actualRecord.RequestedAmount, Is.EqualTo(expectedRecord.RequestedAmount));
                Assert.That(
                    actualRecord.DeliveredAmount,
                    Is.EqualTo(expectedRecord.DeliveredAmount).Within(FloatTolerance));
            }
        }

        private static void AssertSnapshotsEqual(SimulationSnapshot expected, SimulationSnapshot actual)
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
                Assert.That(actual[i], Is.EqualTo(expected[i]).Within(FloatTolerance), $"Mismatch at cell {i}");
        }

        private static void DeleteIfPresent(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
