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
            var first = CreateDefaultSimulation(12345);
            var second = CreateDefaultSimulation(12345);

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
            var uninterrupted = CreateDefaultSimulation(9876);
            uninterrupted.SetTargetTemperature(30f);

            for (int i = 0; i < 40; i++) uninterrupted.Step();
            uninterrupted.AddMoisture(0.12f);

            SimulationSaveData save = uninterrupted.CaptureSave();
            var resumed = CreateDefaultSimulation(9876);
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
            var simulation = CreateDefaultSimulation(42);
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
            var simulation = CreateDefaultSimulation(1337);
            for (int i = 0; i < 20; i++) simulation.Step();
            SimulationSaveData save = simulation.CaptureSave();

            var restored = CreateDefaultSimulation(1337);
            restored.Restore(save);
            float savedMoisture = save.cells[0].moisture;
            float savedBiomass = save.cells[0].biomass;
            restored.AddMoisture(0.2f);
            restored.Step();

            Assert.That(save.cells[0].moisture, Is.EqualTo(savedMoisture).Within(FloatTolerance));
            Assert.That(save.cells[0].biomass, Is.EqualTo(savedBiomass).Within(FloatTolerance));
        }

        [Test]
        public void RestoreRejectsMismatchedSeedAndUnsupportedSchema()
        {
            var source = CreateDefaultSimulation(10);
            SimulationSaveData save = source.CaptureSave();

            var wrongSeed = CreateDefaultSimulation(11);
            Assert.Throws<ArgumentException>(() => wrongSeed.Restore(save));

            save.schemaVersion = PetriSimulation.CurrentSaveSchemaVersion + 1;
            Assert.Throws<ArgumentException>(() => source.Restore(save));
        }

        [Test]
        public void RestoreRejectsNonFiniteAndOutOfRangeValues()
        {
            var source = CreateDefaultSimulation(10);

            SimulationSaveData invalidTemperature = source.CaptureSave();
            invalidTemperature.temperature = float.NaN;
            Assert.Throws<ArgumentException>(() => source.Restore(invalidTemperature));

            SimulationSaveData invalidCell = source.CaptureSave();
            invalidCell.cells[0].moisture = 1.1f;
            Assert.Throws<ArgumentException>(() => source.Restore(invalidCell));

            SimulationSaveData invalidRandomState = source.CaptureSave();
            invalidRandomState.randomState = 0u;
            Assert.Throws<ArgumentException>(() => source.Restore(invalidRandomState));
        }

        [Test]
        public void TargetTemperatureIsClampedToSupportedRange()
        {
            var simulation = CreateDefaultSimulation(1);

            simulation.SetTargetTemperature(-100f);
            Assert.That(simulation.TargetTemperature, Is.EqualTo(8f));

            simulation.SetTargetTemperature(100f);
            Assert.That(simulation.TargetTemperature, Is.EqualTo(42f));

            Assert.Throws<ArgumentOutOfRangeException>(() => simulation.SetTargetTemperature(float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => simulation.SetTargetTemperature(float.PositiveInfinity));
        }

        [Test]
        public void MoistureInterventionRejectsInvalidAmounts()
        {
            var simulation = CreateDefaultSimulation(1);

            Assert.Throws<ArgumentOutOfRangeException>(() => simulation.AddMoisture(-0.01f));
            Assert.Throws<ArgumentOutOfRangeException>(() => simulation.AddMoisture(float.NaN));
        }

        [Test]
        public void RoundDishExcludesInvisibleCornerCellsFromStateAndMetrics()
        {
            var simulation = CreateDefaultSimulation(5001);
            SimulationSaveData save = simulation.CaptureSave();

            for (int i = 0; i < save.cells.Length; i++)
            {
                save.cells[i].moisture = 0.50f;
                save.cells[i].nutrients = 1f;
                save.cells[i].biomass = 0f;
                save.cells[i].health = 1f;
                save.cells[i].stress = 0f;
            }

            save.cells[0].moisture = 0f;
            save.cells[0].nutrients = 0f;
            save.cells[0].biomass = 1f;
            save.cells[0].health = 0f;
            simulation.Restore(save);

            SimulationSnapshot snapshot = simulation.CreateSnapshot();

            Assert.That(snapshot.Biomass[0], Is.Zero);
            Assert.That(snapshot.Moisture[0], Is.Zero);
            Assert.That(snapshot.Coverage, Is.Zero);
            Assert.That(snapshot.AverageHealth, Is.EqualTo(1f).Within(FloatTolerance));
            Assert.That(snapshot.AverageMoisture, Is.EqualTo(0.50f).Within(FloatTolerance));
        }

        [Test]
        public void SimulationStepsDoNotAllocateManagedMemoryAfterWarmup()
        {
            var simulation = CreateDefaultSimulation(7001);
            for (int i = 0; i < 4; i++) simulation.Step();

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 20; i++) simulation.Step();
            long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocatedBytes, Is.Zero);
        }

        [Test]
        public void ComfortableConditionsProduceClearColonyGrowth()
        {
            PetriSimulation simulation = CreateUniformSimulation(
                seed: 2101,
                temperature: 26f,
                moisture: 0.72f,
                nutrients: 1f,
                biomass: 0.10f,
                health: 1f,
                stress: 0f);

            float initialBiomass = TotalBiomass(simulation.CreateSnapshot());
            for (int i = 0; i < 20; i++) simulation.Step();
            SimulationSnapshot result = simulation.CreateSnapshot();

            Assert.That(TotalBiomass(result), Is.GreaterThan(initialBiomass * 2f));
            Assert.That(result.AverageHealth, Is.GreaterThan(0.95f));
            Assert.That(result.AverageMoisture, Is.GreaterThan(0.60f));
        }

        [Test]
        public void LethalHeatCausesHealthAndBiomassDecline()
        {
            PetriSimulation simulation = CreateUniformSimulation(
                seed: 2102,
                temperature: 42f,
                moisture: 0.72f,
                nutrients: 1f,
                biomass: 0.10f,
                health: 1f,
                stress: 0f);

            float initialBiomass = TotalBiomass(simulation.CreateSnapshot());
            for (int i = 0; i < 20; i++) simulation.Step();
            SimulationSnapshot result = simulation.CreateSnapshot();

            Assert.That(TotalBiomass(result), Is.LessThan(initialBiomass * 0.70f));
            Assert.That(result.AverageHealth, Is.LessThan(0.75f));
            Assert.That(result.Temperature, Is.EqualTo(42f).Within(FloatTolerance));
        }

        [Test]
        public void MoistureInterventionRecoversAStressedColony()
        {
            PetriSimulation simulation = CreateUniformSimulation(
                seed: 2103,
                temperature: 26f,
                moisture: 0.12f,
                nutrients: 1f,
                biomass: 0.20f,
                health: 0.70f,
                stress: 1f);

            for (int i = 0; i < 12; i++) simulation.Step();
            SimulationSnapshot stressed = simulation.CreateSnapshot();

            simulation.AddMoisture(0.75f);
            for (int i = 0; i < 60; i++) simulation.Step();
            SimulationSnapshot recovered = simulation.CreateSnapshot();

            Assert.That(stressed.AverageHealth, Is.LessThan(0.50f));
            Assert.That(recovered.AverageHealth, Is.GreaterThan(stressed.AverageHealth + 0.40f));
            Assert.That(recovered.AverageMoisture, Is.GreaterThan(0.60f));
            Assert.That(TotalBiomass(recovered), Is.GreaterThan(TotalBiomass(stressed)));
        }

        private static PetriSimulation CreateUniformSimulation(int seed, float temperature,
            float moisture, float nutrients, float biomass, float health, float stress)
        {
            var simulation = CreateDefaultSimulation(seed);
            SimulationSaveData save = simulation.CaptureSave();
            save.temperature = temperature;
            save.targetTemperature = temperature;

            for (int i = 0; i < save.cells.Length; i++)
            {
                save.cells[i].moisture = moisture;
                save.cells[i].nutrients = nutrients;
                save.cells[i].biomass = biomass;
                save.cells[i].health = health;
                save.cells[i].stress = stress;
            }

            simulation.Restore(save);
            return simulation;
        }

        private static float TotalBiomass(SimulationSnapshot snapshot)
        {
            float total = 0f;
            for (int i = 0; i < snapshot.Biomass.Length; i++) total += snapshot.Biomass[i];
            return total;
        }

        private static PetriSimulation CreateDefaultSimulation(int seed)
        {
            var catalog = PetriDish.Content.SimulationDefinitionCatalog.LoadDefaultOrThrow();
            return new PetriSimulation(seed, catalog.DefaultOrganism, catalog.DefaultMedium);
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
