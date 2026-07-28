using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using PetriDish.Application;
using PetriDish.Simulation;
using UnityEngine;

namespace PetriDish.Tests.Editor
{
    public sealed class ExperimentControllerTests
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
                $"petri-controller-test-{Guid.NewGuid():N}.json");
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
        public void RestartSameSeedRetainsTheActiveExperimentSeed()
        {
            ExperimentController controller = CreateController(ref firstObject);
            controller.StartNew(918273);
            for (int i = 0; i < 8; i++) controller.Simulation.Step();
            SimulationSnapshot expected = new PetriSimulation(918273).CreateSnapshot();

            controller.RestartSameSeed();
            SimulationSnapshot actual = controller.Simulation.CreateSnapshot();

            Assert.That(controller.Simulation.Seed, Is.EqualTo(918273));
            AssertSnapshotsEqual(expected, actual);
        }

        [Test]
        public void SaveLoadRestoresFractionalClockSpeedAndExactContinuation()
        {
            ExperimentController uninterrupted = CreateController(ref firstObject);
            uninterrupted.StartNew(112233);
            uninterrupted.SetSpeed(2f);
            Assert.That(uninterrupted.AdvanceSimulation(0.06f), Is.False);
            Assert.That(uninterrupted.SaveToPath(savePath), Is.True, uninterrupted.LastPersistenceError);

            Assert.That(uninterrupted.AdvanceSimulation(0.07f), Is.True);
            SimulationSnapshot expected = uninterrupted.Simulation.CreateSnapshot();

            ExperimentController resumed = CreateController(ref secondObject);
            Assert.That(resumed.LoadFromPath(savePath), Is.True, resumed.LastPersistenceError);
            Assert.That(resumed.SimulationSpeed, Is.EqualTo(2f));
            Assert.That(resumed.AdvanceSimulation(0.07f), Is.True);

            AssertSnapshotsEqual(expected, resumed.Simulation.CreateSnapshot());
        }

        [Test]
        public void SnapshotsPublishOnlyWhenSimulationStateChanges()
        {
            ExperimentController controller = CreateController(ref firstObject);
            controller.StartNew(303);
            int publishedSnapshots = 0;
            controller.SnapshotUpdated += _ => publishedSnapshots++;

            Assert.That(controller.AdvanceSimulation(0.10f), Is.False);
            Assert.That(publishedSnapshots, Is.Zero);

            Assert.That(controller.AdvanceSimulation(0.15f), Is.True);
            Assert.That(publishedSnapshots, Is.EqualTo(1));

            controller.TogglePause();
            controller.AddMoisture();
            Assert.That(publishedSnapshots, Is.EqualTo(2));
        }

        [Test]
        public void SaveLoadRestoresPauseState()
        {
            ExperimentController writer = CreateController(ref firstObject);
            writer.StartNew(404);
            writer.TogglePause();
            Assert.That(writer.SaveToPath(savePath), Is.True, writer.LastPersistenceError);

            ExperimentController reader = CreateController(ref secondObject);
            Assert.That(reader.LoadFromPath(savePath), Is.True, reader.LastPersistenceError);
            Assert.That(reader.Paused, Is.True);
            Assert.That(reader.AdvanceSimulation(1f), Is.False);
            Assert.That(reader.Simulation.Tick, Is.Zero);
        }

        [Test]
        public void InvalidSaveDoesNotReplaceTheRunningExperiment()
        {
            ExperimentController controller = CreateController(ref firstObject);
            controller.StartNew(445566);
            PetriSimulation original = controller.Simulation;
            File.WriteAllText(savePath, "{\"schemaVersion\":2,\"simulation\":null}");

            Assert.That(controller.LoadFromPath(savePath), Is.False);
            Assert.That(controller.Simulation, Is.SameAs(original));
            Assert.That(controller.Simulation.Seed, Is.EqualTo(445566));
            Assert.That(controller.LastPersistenceError, Is.Not.Empty);
        }

        [Test]
        public void LoadRecoversPreviousAtomicBackupWhenPrimaryIsCorrupt()
        {
            ExperimentController writer = CreateController(ref firstObject);
            writer.StartNew(101);
            Assert.That(writer.SaveToPath(savePath), Is.True, writer.LastPersistenceError);
            writer.StartNew(202);
            Assert.That(writer.SaveToPath(savePath), Is.True, writer.LastPersistenceError);
            File.WriteAllText(savePath, "not-json");

            ExperimentController reader = CreateController(ref secondObject);
            Assert.That(reader.LoadFromPath(savePath), Is.True, reader.LastPersistenceError);
            Assert.That(reader.Simulation.Seed, Is.EqualTo(101));
        }

        [Test]
        public void InitialStateIsPublishedAfterListenersCanSubscribe()
        {
            ExperimentController controller = CreateController(ref firstObject);
            GuidedStage publishedStage = GuidedStage.Failed;
            SimulationSnapshot publishedSnapshot = default;
            bool receivedSnapshot = false;
            controller.StageChanged += (stage, _) => publishedStage = stage;
            controller.SnapshotUpdated += snapshot =>
            {
                publishedSnapshot = snapshot;
                receivedSnapshot = true;
            };

            MethodInfo start = typeof(ExperimentController).GetMethod(
                "Start",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(start, Is.Not.Null);
            start.Invoke(controller, null);

            Assert.That(publishedStage, Is.EqualTo(GuidedStage.ObserveCoolStart));
            Assert.That(receivedSnapshot, Is.True);
            Assert.That(publishedSnapshot.Tick, Is.Zero);
        }

        private static ExperimentController CreateController(ref GameObject owner)
        {
            owner = new GameObject("ExperimentControllerTest");
            return owner.AddComponent<ExperimentController>();
        }

        private static void AssertSnapshotsEqual(SimulationSnapshot expected, SimulationSnapshot actual)
        {
            Assert.That(actual.Tick, Is.EqualTo(expected.Tick));
            Assert.That(actual.Temperature, Is.EqualTo(expected.Temperature).Within(FloatTolerance));
            Assert.That(actual.Coverage, Is.EqualTo(expected.Coverage).Within(FloatTolerance));
            Assert.That(actual.AverageHealth, Is.EqualTo(expected.AverageHealth).Within(FloatTolerance));
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
