using System;
using NUnit.Framework;
using PetriDish.Simulation;

namespace PetriDish.Tests.Editor
{
    public sealed class PetriSimulationTests
    {
        private const float FloatTolerance = 0.000001f;

        [Test]
        public void SameSeedAndInputsProduceIdenticalSnapshots()
        {
            var first = new PetriSimulation(12345);
            var second = new PetriSimulation(12345);

            first.SetTargetTemperature(27f);
            second.SetTargetTemperature(27f);

            for (int i = 0; i < 80; i++)
            {
                if (i == 20 || i == 55)
                {
                    first.AddMoisture(0.08f);
                    second.AddMoisture(0.08f);
                }

                first.Step();
                second.Step();
            }

            AssertSnapshotsEqual(first.CreateSnapshot(), second.CreateSnapshot());
        }

        [Test]
        public void SaveRestoreContinuesTheExactRandomSequence()
        {
            var uninterrupted = new PetriSimulation(9876);
            uninterrupted.SetTargetTemperature(30f);

            for (int i = 0; i < 40; i++) uninterrupted.Step();
            uninterrupted.AddMoisture(0.12f);

            SimulationSaveData save = uninterrupted.CaptureSave();
            var resumed = new PetriSimulation(9876);
            resumed.Restore(save);

            for (int i = 0; i < 60; i++)
            {
                if (i == 15)
                {
                    uninterrupted.AddMoisture(0.07f);
                    resumed.AddMoisture(0.07f);
                }

                uninterrupted.Step();
                resumed.Step();
            }

            AssertSnapshotsEqual(uninterrupted.CreateSnapshot(), resumed.CreateSnapshot());
        }

        [Test]
        public void CapturedSaveIsAnIndependentSnapshot()
        {
            var simulation = new PetriSimulation(42);
            for (int i = 0; i < 12; i++) simulation.Step();

            SimulationSaveData save = simulation.CaptureSave();
            float savedBiomass = save.cells[0].biomass;
            float savedMoisture = save.cells[0].moisture;

            simulation.AddMoisture(0.2f);
            for (int i = 0; i < 10; i++) simulation.Step();

            Assert.That(save.cells[0].biomass, Is.EqualTo(savedBiomass).Within(FloatTolerance));
            Assert.That(save.cells[0].moisture, Is.EqualTo(savedMoisture).Within(FloatTolerance));
        }

        [Test]
        public void RestoreCopiesCellsInsteadOfSharingSaveObjects()
        {
            var simulation = new PetriSimulation(1337);
            for (int i = 0; i < 20; i++) simulation.Step();
            SimulationSaveData save = simulation.CaptureSave();

            var restored = new PetriSimulation(1337);
            restored.Restore(save);
            restored.AddMoisture(0.2f);
            restored.Step();

            SimulationSaveData restoredState = restored.CaptureSave();
            Assert.That(restoredState.cells[0], Is.Not.SameAs(save.cells[0]));
        }

        [Test]
        public void RestoreRejectsMismatchedSeedAndUnsupportedSchema()
        {
            var source = new PetriSimulation(10);
            SimulationSaveData save = source.CaptureSave();

            var wrongSeed = new PetriSimulation(11);
            Assert.Throws<ArgumentException>(() => wrongSeed.Restore(save));

            save.schemaVersion = PetriSimulation.CurrentSaveSchemaVersion + 1;
            Assert.Throws<ArgumentException>(() => source.Restore(save));
        }

        [Test]
        public void TargetTemperatureIsClampedToSupportedRange()
        {
            var simulation = new PetriSimulation(1);

            simulation.SetTargetTemperature(-100f);
            Assert.That(simulation.TargetTemperature, Is.EqualTo(8f));

            simulation.SetTargetTemperature(100f);
            Assert.That(simulation.TargetTemperature, Is.EqualTo(42f));
        }

        private static void AssertSnapshotsEqual(SimulationSnapshot expected, SimulationSnapshot actual)
        {
            Assert.That(actual.Width, Is.EqualTo(expected.Width));
            Assert.That(actual.Height, Is.EqualTo(expected.Height));
            Assert.That(actual.Tick, Is.EqualTo(expected.Tick));
            Assert.That(actual.Temperature, Is.EqualTo(expected.Temperature).Within(FloatTolerance));
            Assert.That(actual.Coverage, Is.EqualTo(expected.Coverage).Within(FloatTolerance));
            Assert.That(actual.AverageHealth, Is.EqualTo(expected.AverageHealth).Within(FloatTolerance));
            Assert.That(actual.AverageMoisture, Is.EqualTo(expected.AverageMoisture).Within(FloatTolerance));
            Assert.That(actual.AverageNutrients, Is.EqualTo(expected.AverageNutrients).Within(FloatTolerance));
            AssertArraysEqual(expected.Biomass, actual.Biomass);
            AssertArraysEqual(expected.Health, actual.Health);
            AssertArraysEqual(expected.Moisture, actual.Moisture);
        }

        private static void AssertArraysEqual(float[] expected, float[] actual)
        {
            Assert.That(actual.Length, Is.EqualTo(expected.Length));
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(actual[i], Is.EqualTo(expected[i]).Within(FloatTolerance), $"Mismatch at cell {i}");
            }
        }
    }
}
