using NUnit.Framework;
using PetriDish.Presentation;
using PetriDish.Simulation;

namespace PetriDish.Tests.Editor
{
    public sealed class AccessibilityPresentationTests
    {
        [TestCase(26f, 0.8f, 0.7f, 0.8f, SimulationCondition.Stable, "OK — Growing well")]
        [TestCase(20f, 0.8f, 0.7f, 0.8f, SimulationCondition.SlowGrowth, "INFO — Growing slowly")]
        [TestCase(36f, 0.8f, 0.7f, 0.8f, SimulationCondition.HeatStress, "WARNING — Heat stressed")]
        [TestCase(26f, 0.8f, 0.2f, 0.8f, SimulationCondition.Dry, "WARNING — Too dry")]
        [TestCase(26f, 0.8f, 0.7f, 0.1f, SimulationCondition.NutrientLimited, "WARNING — Nutrient limited")]
        [TestCase(26f, 0.3f, 0.7f, 0.8f, SimulationCondition.Declining, "ALERT — Colony declining")]
        public void ConditionIncludesNonColourSeverityCue(
            float temperature,
            float health,
            float moisture,
            float nutrients,
            SimulationCondition expectedCondition,
            string expectedLabel)
        {
            SimulationSnapshot snapshot = Snapshot(temperature, health, moisture, nutrients);

            SimulationCondition condition = AccessibilityPresentation.GetCondition(snapshot);

            Assert.That(condition, Is.EqualTo(expectedCondition));
            Assert.That(AccessibilityPresentation.ConditionLabel(condition), Is.EqualTo(expectedLabel));
        }

        [TestCase(false, "Pause")]
        [TestCase(true, "Resume")]
        public void PauseButtonStatesItsNextAction(bool isPaused, string expected)
        {
            Assert.That(AccessibilityPresentation.PauseButtonLabel(isPaused), Is.EqualTo(expected));
        }

        [TestCase(false, 1f, "Simulation running at 1×")]
        [TestCase(false, 2f, "Simulation running at 2×")]
        [TestCase(true, 4f, "Simulation paused")]
        public void SimulationStateIsExplicit(bool isPaused, float speed, string expected)
        {
            Assert.That(AccessibilityPresentation.SimulationStateLabel(isPaused, speed), Is.EqualTo(expected));
        }

        private static SimulationSnapshot Snapshot(float temperature, float health, float moisture, float nutrients)
        {
            return new SimulationSnapshot(
                1,
                1,
                0,
                temperature,
                0.5f,
                health,
                moisture,
                nutrients,
                new[] { 0.5f },
                new[] { health },
                new[] { moisture });
        }
    }
}
