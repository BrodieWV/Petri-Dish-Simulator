using System;
using NUnit.Framework;
using PetriDish.Presentation;

namespace PetriDish.Tests.Editor
{
    public sealed class TextScalePolicyTests
    {
        [TestCase(TextScaleMode.Standard, TextScaleMode.Large)]
        [TestCase(TextScaleMode.Large, TextScaleMode.Standard)]
        public void NextCyclesBetweenSupportedModes(TextScaleMode current, TextScaleMode expected)
        {
            Assert.That(TextScalePolicy.Next(current), Is.EqualTo(expected));
        }

        [TestCase(TextScaleMode.Standard, "Text: Standard")]
        [TestCase(TextScaleMode.Large, "Text: Large")]
        public void ButtonLabelStatesActiveMode(TextScaleMode mode, string expected)
        {
            Assert.That(TextScalePolicy.ButtonLabel(mode), Is.EqualTo(expected));
        }

        [TestCase(20, TextScaleMode.Standard, 20)]
        [TestCase(20, TextScaleMode.Large, 25)]
        [TestCase(27, TextScaleMode.Large, 34)]
        public void ScaleFontSizeUsesStableRoundedValues(int baseSize, TextScaleMode mode, int expected)
        {
            Assert.That(TextScalePolicy.ScaleFontSize(baseSize, mode), Is.EqualTo(expected));
        }

        [Test]
        public void ScaleFontSizeRejectsInvalidBaseSize()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TextScalePolicy.ScaleFontSize(0, TextScaleMode.Standard));
        }

        [TestCase(-1, TextScaleMode.Standard)]
        [TestCase(0, TextScaleMode.Standard)]
        [TestCase(1, TextScaleMode.Large)]
        [TestCase(99, TextScaleMode.Standard)]
        public void StoredValueFallsBackSafely(int storedValue, TextScaleMode expected)
        {
            Assert.That(TextScalePolicy.FromStoredValue(storedValue), Is.EqualTo(expected));
        }
    }
}
