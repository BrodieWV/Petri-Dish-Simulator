using System.Reflection;
using NUnit.Framework;
using PetriDish.Application;
using PetriDish.Content;
using PetriDish.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace PetriDish.Tests.Editor
{
    public sealed class PetriDishResponsiveUIBinderTests
    {
        private GameObject uiRoot;
        private GameObject controllerRoot;

        [TearDown]
        public void TearDown()
        {
            if (uiRoot != null) Object.DestroyImmediate(uiRoot);
            if (controllerRoot != null) Object.DestroyImmediate(controllerRoot);
        }

        [TestCase(0, false, 1f, "T+00:00 ? SIMULATION RUNNING ? 1?")]
        [TestCase(40, true, 4f, "T+00:10 ? SIMULATION PAUSED ? 4?")]
        [TestCase(14400, false, 2f, "T+01:00:00 ? SIMULATION RUNNING ? 2?")]
        public void SimulationStateFormattingIncludesElapsedPauseAndSpeed(
            long tick,
            bool paused,
            float speed,
            string expected)
        {
            Assert.That(
                PetriDishResponsiveUIBinder.FormatSimulationState(tick, paused, speed),
                Is.EqualTo(expected));
        }

        [Test]
        public void NutrientAvailabilityAndLabelsDescribeEveryInterventionState()
        {
            Assert.That(
                PetriDishResponsiveUIBinder.IsNutrientDoseAvailable(3, false, 0),
                Is.True);
            Assert.That(
                PetriDishResponsiveUIBinder.IsNutrientDoseAvailable(2, true, 0),
                Is.False);
            Assert.That(
                PetriDishResponsiveUIBinder.IsNutrientDoseAvailable(2, false, 4),
                Is.False);
            Assert.That(
                PetriDishResponsiveUIBinder.FormatNutrientButtonLabel(2, true, 3, 12, 0),
                Does.Contain("delivering 3/12"));
            Assert.That(
                PetriDishResponsiveUIBinder.FormatNutrientButtonLabel(2, false, 0, 12, 4),
                Does.Contain("ready in 1s"));
            Assert.That(
                PetriDishResponsiveUIBinder.FormatNutrientButtonLabel(0, false, 0, 12, 0),
                Does.Contain("none left"));
        }

        [Test]
        public void ResponsiveUiPresenceSuppressesOnlyLegacyCanvasGeneration()
        {
            Assert.That(
                RuntimeBootstrap.ShouldGenerateLegacyRuntimeUi(true, false),
                Is.True);
            Assert.That(
                RuntimeBootstrap.ShouldGenerateLegacyRuntimeUi(true, true),
                Is.False);
            Assert.That(
                RuntimeBootstrap.ShouldGenerateLegacyRuntimeUi(false, false),
                Is.False);
        }

        [Test]
        public void OrganismAndMediumButtonsCycleStableDefinitionsWithoutDuplicateRenderer()
        {
            PetriDishResponsiveUIBinder binder = CreateResponsiveHierarchy();
            controllerRoot = new GameObject("ExperimentController");
            ExperimentController controller =
                controllerRoot.AddComponent<ExperimentController>();
            controller.StartNew(ExperimentController.TutorialSeed);

            Assert.That(binder.AutoAssignReferences(), Is.True);
            SetPrivateField(binder, "controller", controller);
            SetPrivateField(binder, "initialized", true);
            DishRenderer renderer =
                uiRoot.GetComponentInChildren<DishRenderer>(true);
            SetPrivateField(binder, "dishRenderer", renderer);
            binder.SelectNextOrganism();
            Assert.That(
                controller.Simulation.OrganismId,
                Is.EqualTo(SimulationDefinitionCatalog.SaccharomycesCerevisiaeId));

            binder.SelectNextMedium();
            Assert.That(
                controller.Simulation.MediumId,
                Is.EqualTo(SimulationDefinitionCatalog.LowNutrientAgarId));
            Assert.That(renderer, Is.SameAs(binder.ColonyTextureSource));
            Assert.That(
                uiRoot.GetComponentsInChildren<DishRenderer>(true),
                Has.Length.EqualTo(1));
        }

        private PetriDishResponsiveUIBinder CreateResponsiveHierarchy()
        {
            uiRoot = new GameObject(
                "PetriDishResponsiveUI",
                typeof(RectTransform),
                typeof(Canvas));
            CreateImage(uiRoot.transform, "Background");
            string[] buttons =
            {
                "OrganismButton", "MediumButton", "TemperatureButton", "MoistureButton",
                "AddMoistureButton", "AddNutrientsButton", "PauseButton", "SpeedButton",
                "SaveButton", "LoadButton", "RestartButton", "NewSeedButton"
            };
            for (int i = 0; i < buttons.Length; i++)
                CreateButton(uiRoot.transform, buttons[i]);

            CreateText(uiRoot.transform, "ExperimentName");
            CreateText(uiRoot.transform, "ConditionLabel");
            CreateText(uiRoot.transform, "SimulationState");
            CreateText(uiRoot.transform, "InspectionText");
            CreateMetric(uiRoot.transform, "TemperatureMetric");
            CreateMetric(uiRoot.transform, "CoverageMetric");
            CreateMetric(uiRoot.transform, "MoistureMetric");
            CreateMetric(uiRoot.transform, "NutrientsMetric");

            Image panel = CreateImage(uiRoot.transform, "DishViewportPanel");
            Image target = CreateImage(panel.transform, "DishRenderTarget");
            CreateText(target.transform, "ViewportHint");
            GameObject surface = new GameObject(
                "DishInteractionSurface",
                typeof(RectTransform),
                typeof(RawImage),
                typeof(DishRenderer));
            surface.transform.SetParent(target.transform, false);
            return uiRoot.AddComponent<PetriDishResponsiveUIBinder>();
        }

        private static void SetPrivateField(
            object target,
            string name,
            object value)
        {
            FieldInfo field = target.GetType().GetField(
                name,
                BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(target, value);
        }

        private static Button CreateButton(Transform parent, string name)
        {
            GameObject owner = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));
            owner.transform.SetParent(parent, false);
            CreateText(owner.transform, "Label");
            return owner.GetComponent<Button>();
        }

        private static Image CreateImage(Transform parent, string name)
        {
            GameObject owner = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image));
            owner.transform.SetParent(parent, false);
            return owner.GetComponent<Image>();
        }

        private static Text CreateText(Transform parent, string name)
        {
            GameObject owner = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Text));
            owner.transform.SetParent(parent, false);
            return owner.GetComponent<Text>();
        }

        private static void CreateMetric(Transform parent, string name)
        {
            GameObject card = new GameObject(name, typeof(RectTransform));
            card.transform.SetParent(parent, false);
            CreateText(card.transform, "Value");
        }
    }
}
