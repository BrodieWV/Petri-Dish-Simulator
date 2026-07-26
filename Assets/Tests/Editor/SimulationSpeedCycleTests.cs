using NUnit.Framework;
using PetriDish.Application;

namespace PetriDish.Tests.Editor
{
    public sealed class SimulationSpeedCycleTests
    {
        [TestCase(0f, 2f)]
        [TestCase(1f, 2f)]
        [TestCase(2f, 4f)]
        [TestCase(4f, 1f)]
        [TestCase(8f, 1f)]
        public void NextCyclesThroughSupportedSpeeds(float current, float expected)
        {
            Assert.That(SimulationSpeedCycle.Next(current), Is.EqualTo(expected));
        }

        [TestCase(1f, "Speed 1×")]
        [TestCase(2f, "Speed 2×")]
        [TestCase(4f, "Speed 4×")]
        public void LabelShowsTheActiveSpeed(float speed, string expected)
        {
            Assert.That(SimulationSpeedCycle.Label(speed), Is.EqualTo(expected));
        }
    }
}
