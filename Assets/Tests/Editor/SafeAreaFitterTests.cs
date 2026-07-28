using NUnit.Framework;
using PetriDish.Presentation;
using UnityEngine;

namespace PetriDish.Tests.Editor
{
    public sealed class SafeAreaFitterTests
    {
        [Test]
        public void CalculateAnchorsConvertsPhoneInsetsToNormalizedCoordinates()
        {
            var safeArea = new Rect(0f, 96f, 1440f, 2880f);

            SafeAreaFitter.CalculateAnchors(
                safeArea,
                1440f,
                3088f,
                out Vector2 anchorMin,
                out Vector2 anchorMax);

            Assert.That(anchorMin.x, Is.EqualTo(0f));
            Assert.That(anchorMin.y, Is.EqualTo(96f / 3088f).Within(0.000001f));
            Assert.That(anchorMax.x, Is.EqualTo(1f));
            Assert.That(anchorMax.y, Is.EqualTo(2976f / 3088f).Within(0.000001f));
        }

        [Test]
        public void CalculateAnchorsFallsBackToFullScreenForInvalidDimensions()
        {
            SafeAreaFitter.CalculateAnchors(
                new Rect(10f, 10f, 100f, 100f),
                0f,
                0f,
                out Vector2 anchorMin,
                out Vector2 anchorMax);

            Assert.That(anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(anchorMax, Is.EqualTo(Vector2.one));
        }
    }
}
