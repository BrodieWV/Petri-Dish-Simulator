using NUnit.Framework;
using PetriDish.Presentation;
using PetriDish.Simulation;

namespace PetriDish.Tests.Editor
{
    public sealed class DishInspectionTests
    {
        [TestCase(0f, 0f, 0, 0)]
        [TestCase(0.49f, 0.49f, 0, 0)]
        [TestCase(0.50f, 0.50f, 1, 1)]
        [TestCase(1f, 1f, 1, 1)]
        public void NormalizedTapMapsToExpectedCell(float nx, float ny, int expectedX, int expectedY)
        {
            bool mapped = DishInspection.TryMapNormalizedPoint(nx, ny, 2, 2, out int x, out int y);

            Assert.That(mapped, Is.True);
            Assert.That(x, Is.EqualTo(expectedX));
            Assert.That(y, Is.EqualTo(expectedY));
        }

        [TestCase(-0.01f, 0.5f)]
        [TestCase(1.01f, 0.5f)]
        [TestCase(0.5f, -0.01f)]
        [TestCase(0.5f, 1.01f)]
        public void TapOutsideDishIsRejected(float nx, float ny)
        {
            Assert.That(DishInspection.TryMapNormalizedPoint(nx, ny, 2, 2, out _, out _), Is.False);
        }

        [Test]
        public void InspectionReportsLocalCellValuesAndCondition()
        {
            SimulationSnapshot snapshot = Snapshot(
                temperature: 26f,
                biomass: new[] { 0f, 0.4f, 0.2f, 0.1f },
                health: new[] { 1f, 0.9f, 0.8f, 0.7f },
                moisture: new[] { 0.7f, 0.75f, 0.65f, 0.6f });
            SimulationSaveData save = SaveWithNutrients(1f, 0.8f, 0.7f, 0.6f);

            bool found = DishInspection.TryInspect(snapshot, save, 1, 0, out CellInspection inspection);

            Assert.That(found, Is.True);
            Assert.That(inspection.X, Is.EqualTo(1));
            Assert.That(inspection.Y, Is.EqualTo(0));
            Assert.That(inspection.Biomass, Is.EqualTo(0.4f));
            Assert.That(inspection.Health, Is.EqualTo(0.9f));
            Assert.That(inspection.Moisture, Is.EqualTo(0.75f));
            Assert.That(inspection.Nutrients, Is.EqualTo(0.8f));
            Assert.That(inspection.Condition, Is.EqualTo("Healthy growth"));
        }

        [TestCase(26f, 0f, 1f, 0.7f, 1f, "No visible colony")]
        [TestCase(42f, 0.4f, 1f, 0.7f, 1f, "Lethal temperature")]
        [TestCase(26f, 0.4f, 1f, 0.1f, 1f, "Critically dry")]
        [TestCase(26f, 0.4f, 1f, 0.7f, 0.1f, "Nutrients exhausted")]
        [TestCase(26f, 0.4f, 0.3f, 0.7f, 1f, "Colony declining")]
        public void ConditionUsesLocalLimitingFactor(float temperature, float biomass, float health, float moisture, float nutrients, string expected)
        {
            Assert.That(DishInspection.GetCondition(temperature, biomass, health, moisture, nutrients), Is.EqualTo(expected));
        }

        [Test]
        public void InvalidCellDataIsRejected()
        {
            SimulationSnapshot snapshot = Snapshot(26f, new[] { 0.2f }, new[] { 1f }, new[] { 0.7f });
            SimulationSaveData save = SaveWithNutrients(1f);

            Assert.That(DishInspection.TryInspect(snapshot, save, 0, 0, out _), Is.False);
        }

        private static SimulationSnapshot Snapshot(float temperature, float[] biomass, float[] health, float[] moisture)
        {
            return new SimulationSnapshot(2, 2, 0, temperature, 0f, 0f, 0f, 0f, biomass, health, moisture);
        }

        private static SimulationSaveData SaveWithNutrients(params float[] nutrients)
        {
            var cells = new CellState[nutrients.Length];
            for (int i = 0; i < nutrients.Length; i++)
            {
                cells[i] = new CellState { nutrients = nutrients[i] };
            }
            return new SimulationSaveData { cells = cells };
        }
    }
}
