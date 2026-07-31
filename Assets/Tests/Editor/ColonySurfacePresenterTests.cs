using System.Reflection;
using NUnit.Framework;
using PetriDish.Content;
using PetriDish.Presentation;
using PetriDish.Simulation;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PetriDish.Tests.Editor
{
    public sealed class ColonySurfacePresenterTests
    {
        private GameObject sourceObject;
        private GameObject targetObject;
        private Material material;

        [TearDown]
        public void TearDown()
        {
            if (sourceObject != null) Object.DestroyImmediate(sourceObject);
            if (targetObject != null) Object.DestroyImmediate(targetObject);
            if (material != null) Object.DestroyImmediate(material);
        }

        [Test]
        public void GeneratedTextureIsAssignedWithoutMutatingSharedMaterial()
        {
            DishRenderer source = CreateSource();
            ColonySurfacePresenter presenter = CreatePresenter(out MeshRenderer target);

            Assert.That(presenter.Bind(source), Is.True, presenter.LastValidationError);

            var block = new MaterialPropertyBlock();
            target.GetPropertyBlock(block);
            Assert.That(block.GetTexture(Shader.PropertyToID("_MainTex")), Is.SameAs(source.ColonyTexture));
            Assert.That(target.sharedMaterial, Is.SameAs(material));
            Assert.That(material.GetTexture("_MainTex"), Is.Null);
        }

        [Test]
        public void RecreatedTextureIsReassignedToTheSameTarget()
        {
            DishRenderer source = CreateSource();
            ColonySurfacePresenter presenter = CreatePresenter(out MeshRenderer target);
            Assert.That(presenter.Bind(source), Is.True, presenter.LastValidationError);
            Texture2D firstTexture = source.ColonyTexture;

            Object.DestroyImmediate(firstTexture);
            source.Render(CreateSnapshot());

            Assert.That(source.ColonyTexture, Is.Not.Null);
            Assert.That(source.ColonyTexture, Is.Not.SameAs(firstTexture));
            var block = new MaterialPropertyBlock();
            target.GetPropertyBlock(block);
            Assert.That(block.GetTexture(Shader.PropertyToID("_MainTex")), Is.SameAs(source.ColonyTexture));
        }

        [Test]
        public void MissingRendererFailsWithClearError()
        {
            DishRenderer source = CreateSource();
            ColonySurfacePresenter presenter = CreatePresenter(out _);
            presenter.Configure(null, "_MainTex");
            LogAssert.Expect(LogType.Error, "ColonySurfacePresenter: A target MeshRenderer is required.");

            Assert.That(presenter.Bind(source), Is.False);
            Assert.That(presenter.LastValidationError, Does.Contain("MeshRenderer"));
        }

        [Test]
        public void MissingShaderPropertyFailsWithClearError()
        {
            DishRenderer source = CreateSource();
            ColonySurfacePresenter presenter = CreatePresenter(out MeshRenderer target);
            presenter.Configure(target, "_NotARealTextureProperty");
            LogAssert.Expect(
                LogType.Error,
                "ColonySurfacePresenter: Material 'ColonySurfaceTestMaterial' using shader 'Standard' does not expose texture property '_NotARealTextureProperty'.");

            Assert.That(presenter.Bind(source), Is.False);
            Assert.That(presenter.LastValidationError, Does.Contain("_NotARealTextureProperty"));
        }

        [Test]
        public void HidingFlatImagePreservesItsInspectionRaycastSurface()
        {
            DishRenderer source = CreateSource();
            ColonySurfacePresenter presenter = CreatePresenter(out MeshRenderer target);
            RawImage sourceImage = source.GetComponent<RawImage>();
            presenter.Configure(target, "_MainTex", hideFlatDishImage: true);

            Assert.That(presenter.Bind(source), Is.True, presenter.LastValidationError);
            Assert.That(source.FlatPresentationVisible, Is.False);
            Assert.That(sourceImage.color.a, Is.Zero);
            Assert.That(sourceImage.raycastTarget, Is.True);

            MethodInfo onDisable = typeof(ColonySurfacePresenter).GetMethod(
                "OnDisable",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(onDisable, Is.Not.Null);
            onDisable.Invoke(presenter, null);
            Assert.That(source.FlatPresentationVisible, Is.True);
            Assert.That(sourceImage.color.a, Is.GreaterThan(0f));
            Assert.That(sourceImage.raycastTarget, Is.True);
        }

        [Test]
        public void PresenterDoesNotOwnSimulationState()
        {
            FieldInfo[] fields = typeof(ColonySurfacePresenter).GetFields(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                Assert.That(
                    typeof(PetriSimulation).IsAssignableFrom(field.FieldType),
                    Is.False,
                    $"Presenter field '{field.Name}' must not own a simulation.");
                Assert.That(
                    field.FieldType == typeof(SimulationSnapshot),
                    Is.False,
                    $"Presenter field '{field.Name}' must not duplicate snapshot state.");
            }
        }

        private DishRenderer CreateSource()
        {
            sourceObject = new GameObject(
                "DishRendererSource",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(RawImage),
                typeof(DishRenderer));
            DishRenderer source = sourceObject.GetComponent<DishRenderer>();
            MethodInfo awake = typeof(DishRenderer).GetMethod(
                "Awake",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(awake, Is.Not.Null);
            awake.Invoke(source, null);
            return source;
        }

        private ColonySurfacePresenter CreatePresenter(out MeshRenderer target)
        {
            targetObject = new GameObject(
                "PetriDish_ColonySurface",
                typeof(MeshFilter),
                typeof(MeshRenderer),
                typeof(ColonySurfacePresenter));
            target = targetObject.GetComponent<MeshRenderer>();
            Shader shader = Shader.Find("Standard");
            Assert.That(shader, Is.Not.Null, "Built-in Standard shader is required by this test.");
            material = new Material(shader) { name = "ColonySurfaceTestMaterial" };
            target.sharedMaterial = material;
            ColonySurfacePresenter presenter = targetObject.GetComponent<ColonySurfacePresenter>();
            presenter.Configure(target, "_MainTex");
            return presenter;
        }

        private static SimulationSnapshot CreateSnapshot()
        {
            SimulationDefinitionCatalog catalog = SimulationDefinitionCatalog.LoadDefaultOrThrow();
            return new PetriSimulation(
                2468,
                catalog.DefaultOrganism,
                catalog.DefaultMedium).CreateSnapshot();
        }
    }
}
