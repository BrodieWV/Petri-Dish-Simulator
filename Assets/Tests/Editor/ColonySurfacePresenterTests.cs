using System.Reflection;
using NUnit.Framework;
using PetriDish.Content;
using PetriDish.Presentation;
using PetriDish.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PetriDish.Tests.Editor
{
    public sealed class ColonySurfacePresenterTests
    {
        private GameObject sourceObject;
        private GameObject targetObject;
        private Material material;
        private Mesh mesh;

        [TearDown]
        public void TearDown()
        {
            if (sourceObject != null) Object.DestroyImmediate(sourceObject);
            if (targetObject != null) Object.DestroyImmediate(targetObject);
            if (material != null) Object.DestroyImmediate(material);
            if (mesh != null) Object.DestroyImmediate(mesh);
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
        public void SavedPreviewTextureUsesSamePropertyBlockPathWithoutMutatingMaterial()
        {
            ColonySurfacePresenter presenter = CreatePresenter(out MeshRenderer target);
            Texture2D preview = new Texture2D(4, 4) { name = "SavedPreview" };
            try
            {
                Assert.That(presenter.ConfigureStatic(target, "_MainTex", preview), Is.True, presenter.LastValidationError);
                Assert.That(presenter.StaticTexture, Is.SameAs(preview));
                Assert.That(presenter.TextureSource, Is.Null);
                var block = new MaterialPropertyBlock();
                target.GetPropertyBlock(block);
                Assert.That(block.GetTexture(Shader.PropertyToID("_MainTex")), Is.SameAs(preview));
                Assert.That(material.GetTexture("_MainTex"), Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(preview);
            }
        }

        [Test]
        public void DefaultAlignmentPreservesCurrentTextureTransform()
        {
            DishRenderer source = CreateSource();
            ColonySurfacePresenter presenter = CreatePresenter(out MeshRenderer target);

            Assert.That(presenter.Bind(source), Is.True, presenter.LastValidationError);

            var block = new MaterialPropertyBlock();
            target.GetPropertyBlock(block);
            AssertVectorApproximately(
                new Vector4(1f, 1f, 0f, 0f),
                block.GetVector(Shader.PropertyToID("_MainTex_ST")));
        }

        [Test]
        public void ScaleOffsetAndFlipsAreAppliedThroughTextureTransformProperty()
        {
            DishRenderer source = CreateSource();
            ColonySurfacePresenter presenter = CreatePresenter(out MeshRenderer target);
            Assert.That(
                presenter.SetTextureAlignment(
                    new Vector2(0.8f, 0.6f),
                    new Vector2(0.1f, 0.2f),
                    horizontalFlip: true,
                    verticalFlip: true),
                Is.True,
                presenter.LastValidationError);

            Assert.That(presenter.Bind(source), Is.True, presenter.LastValidationError);

            var block = new MaterialPropertyBlock();
            target.GetPropertyBlock(block);
            AssertVectorApproximately(
                new Vector4(-0.8f, -0.6f, 0.9f, 0.8f),
                block.GetVector(Shader.PropertyToID("_MainTex_ST")));
            Assert.That(block.GetTexture(Shader.PropertyToID("_MainTex")), Is.SameAs(source.ColonyTexture));
            Assert.That(material.GetTextureScale("_MainTex"), Is.EqualTo(Vector2.one));
            Assert.That(material.GetTextureOffset("_MainTex"), Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void RuntimeAlignmentChangesReuseCachedPropertyBlocksAndLiveTexture()
        {
            DishRenderer source = CreateSource();
            ColonySurfacePresenter presenter = CreatePresenter(out MeshRenderer target);
            Assert.That(presenter.Bind(source), Is.True, presenter.LastValidationError);
            FieldInfo propertyBlockField = typeof(ColonySurfacePresenter).GetField(
                "propertyBlock",
                BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo originalPropertyBlockField = typeof(ColonySurfacePresenter).GetField(
                "originalPropertyBlock",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(propertyBlockField, Is.Not.Null);
            Assert.That(originalPropertyBlockField, Is.Not.Null);
            object appliedBlock = propertyBlockField.GetValue(presenter);
            object originalBlock = originalPropertyBlockField.GetValue(presenter);

            Assert.That(
                presenter.SetTextureAlignment(
                    new Vector2(1.25f, 0.75f),
                    new Vector2(-0.1f, 0.15f),
                    verticalFlip: true),
                Is.True,
                presenter.LastValidationError);

            var block = new MaterialPropertyBlock();
            target.GetPropertyBlock(block);
            AssertVectorApproximately(
                new Vector4(1.25f, -0.75f, -0.1f, 0.9f),
                block.GetVector(Shader.PropertyToID("_MainTex_ST")));
            Assert.That(block.GetTexture(Shader.PropertyToID("_MainTex")), Is.SameAs(source.ColonyTexture));
            Assert.That(propertyBlockField.GetValue(presenter), Is.SameAs(appliedBlock));
            Assert.That(originalPropertyBlockField.GetValue(presenter), Is.SameAs(originalBlock));
            Assert.That(target.sharedMaterial, Is.SameAs(material));
        }

        [Test]
        public void InspectorAlignmentChangesRefreshAnExistingBinding()
        {
            DishRenderer source = CreateSource();
            ColonySurfacePresenter presenter = CreatePresenter(out MeshRenderer target);
            Assert.That(presenter.Bind(source), Is.True, presenter.LastValidationError);
            var serializedPresenter = new SerializedObject(presenter);
            serializedPresenter.FindProperty("textureScale").vector2Value = new Vector2(0.9f, 0.7f);
            serializedPresenter.FindProperty("textureOffset").vector2Value = new Vector2(0.05f, 0.15f);
            serializedPresenter.FindProperty("flipX").boolValue = true;
            serializedPresenter.ApplyModifiedPropertiesWithoutUndo();

            MethodInfo onValidate = typeof(ColonySurfacePresenter).GetMethod(
                "OnValidate",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(onValidate, Is.Not.Null);
            onValidate.Invoke(presenter, null);

            var block = new MaterialPropertyBlock();
            target.GetPropertyBlock(block);
            AssertVectorApproximately(
                new Vector4(-0.9f, 0.7f, 0.95f, 0.15f),
                block.GetVector(Shader.PropertyToID("_MainTex_ST")));
            Assert.That(block.GetTexture(Shader.PropertyToID("_MainTex")), Is.SameAs(source.ColonyTexture));
        }

        [Test]
        public void AutoCentrePreservesScaleAndFlipsWhileCenteringUvBounds()
        {
            DishRenderer source = CreateSource();
            ColonySurfacePresenter presenter = CreatePresenter(out MeshRenderer target);
            AssignUvMesh(
                target,
                new Vector2(0.2f, 0.1f),
                new Vector2(0.6f, 0.1f),
                new Vector2(0.6f, 0.7f),
                new Vector2(0.2f, 0.7f));
            Assert.That(
                presenter.SetTextureAlignment(
                    new Vector2(0.8f, 0.6f),
                    new Vector2(0.3f, -0.2f),
                    horizontalFlip: true),
                Is.True,
                presenter.LastValidationError);
            Assert.That(presenter.Bind(source), Is.True, presenter.LastValidationError);

            Assert.That(presenter.AutoCentre(), Is.True, presenter.LastValidationError);

            Assert.That(presenter.TextureScale, Is.EqualTo(new Vector2(0.8f, 0.6f)));
            Assert.That(presenter.FlipX, Is.True);
            Assert.That(presenter.FlipY, Is.False);
            Assert.That(Vector2.Distance(presenter.TextureOffset, new Vector2(0.02f, 0.26f)), Is.LessThan(0.000001f));
            var block = new MaterialPropertyBlock();
            target.GetPropertyBlock(block);
            AssertVectorApproximately(
                new Vector4(-0.8f, 0.6f, 0.82f, 0.26f),
                block.GetVector(Shader.PropertyToID("_MainTex_ST")));
        }

        [Test]
        public void AutoFitUsesUniformScaleToContainAndCenterUvBounds()
        {
            DishRenderer source = CreateSource();
            ColonySurfacePresenter presenter = CreatePresenter(out MeshRenderer target);
            AssignUvMesh(
                target,
                new Vector2(0.2f, 0.1f),
                new Vector2(0.6f, 0.1f),
                new Vector2(0.6f, 0.9f),
                new Vector2(0.2f, 0.9f));
            Assert.That(presenter.Bind(source), Is.True, presenter.LastValidationError);

            Assert.That(presenter.AutoFit(), Is.True, presenter.LastValidationError);

            Assert.That(Vector2.Distance(presenter.TextureScale, new Vector2(1.25f, 1.25f)), Is.LessThan(0.000001f));
            Assert.That(Vector2.Distance(presenter.TextureOffset, new Vector2(0f, -0.125f)), Is.LessThan(0.000001f));
            var block = new MaterialPropertyBlock();
            target.GetPropertyBlock(block);
            AssertVectorApproximately(
                new Vector4(1.25f, 1.25f, 0f, -0.125f),
                block.GetVector(Shader.PropertyToID("_MainTex_ST")));
            Assert.That(block.GetTexture(Shader.PropertyToID("_MainTex")), Is.SameAs(source.ColonyTexture));
        }

        [Test]
        public void ResetAlignmentRestoresDefaultsWithoutReplacingLiveTexture()
        {
            DishRenderer source = CreateSource();
            ColonySurfacePresenter presenter = CreatePresenter(out MeshRenderer target);
            Assert.That(presenter.Bind(source), Is.True, presenter.LastValidationError);
            Assert.That(
                presenter.SetTextureAlignment(
                    new Vector2(1.4f, 0.7f),
                    new Vector2(0.2f, -0.1f),
                    horizontalFlip: true,
                    verticalFlip: true),
                Is.True,
                presenter.LastValidationError);

            Assert.That(presenter.ResetAlignment(), Is.True, presenter.LastValidationError);

            Assert.That(presenter.TextureScale, Is.EqualTo(Vector2.one));
            Assert.That(presenter.TextureOffset, Is.EqualTo(Vector2.zero));
            Assert.That(presenter.FlipX, Is.False);
            Assert.That(presenter.FlipY, Is.False);
            var block = new MaterialPropertyBlock();
            target.GetPropertyBlock(block);
            AssertVectorApproximately(
                new Vector4(1f, 1f, 0f, 0f),
                block.GetVector(Shader.PropertyToID("_MainTex_ST")));
            Assert.That(block.GetTexture(Shader.PropertyToID("_MainTex")), Is.SameAs(source.ColonyTexture));
        }

        [Test]
        public void AutoFitFailureKeepsExistingBindingAndAlignment()
        {
            DishRenderer source = CreateSource();
            ColonySurfacePresenter presenter = CreatePresenter(out MeshRenderer target);
            Assert.That(
                presenter.SetTextureAlignment(
                    new Vector2(0.8f, 0.6f),
                    new Vector2(0.1f, 0.2f)),
                Is.True,
                presenter.LastValidationError);
            Assert.That(presenter.Bind(source), Is.True, presenter.LastValidationError);
            LogAssert.Expect(
                LogType.Error,
                "ColonySurfacePresenter: MeshRenderer 'PetriDish_ColonySurface' requires a MeshFilter with a shared Mesh before calculating texture alignment.");

            Assert.That(presenter.AutoFit(), Is.False);

            Assert.That(presenter.HasAppliedTexture, Is.True);
            Assert.That(presenter.TextureScale, Is.EqualTo(new Vector2(0.8f, 0.6f)));
            Assert.That(presenter.TextureOffset, Is.EqualTo(new Vector2(0.1f, 0.2f)));
            var block = new MaterialPropertyBlock();
            target.GetPropertyBlock(block);
            Assert.That(block.GetTexture(Shader.PropertyToID("_MainTex")), Is.SameAs(source.ColonyTexture));
            AssertVectorApproximately(
                new Vector4(0.8f, 0.6f, 0.1f, 0.2f),
                block.GetVector(Shader.PropertyToID("_MainTex_ST")));
        }

        [Test]
        public void CustomInspectorProvidesAlignmentActions()
        {
            ColonySurfacePresenter presenter = CreatePresenter(out _);
            UnityEditor.Editor inspector = UnityEditor.Editor.CreateEditor(presenter);
            try
            {
                Assert.That(
                    inspector.GetType().FullName,
                    Is.EqualTo("PetriDish.Editor.ColonySurfacePresenterEditor"));
            }
            finally
            {
                Object.DestroyImmediate(inspector);
            }
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
            presenter.Configure(target, "_NotARealTextureProperty", hideFlatDishImage: true);
            source.SetFlatPresentationVisible(false);
            LogAssert.Expect(
                LogType.Error,
                "ColonySurfacePresenter: Material 'ColonySurfaceTestMaterial' using shader 'Standard' does not expose texture property '_NotARealTextureProperty'.");

            Assert.That(presenter.Bind(source), Is.False);
            Assert.That(presenter.LastValidationError, Does.Contain("_NotARealTextureProperty"));
            Assert.That(source.FlatPresentationVisible, Is.True);
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

            Vector2? tappedPoint = null;
            source.DishTapped += point => tappedPoint = point;
            source.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 100f);
            source.OnPointerClick(new PointerEventData(null) { position = Vector2.zero });
            Assert.That(tappedPoint.HasValue, Is.True);
            Assert.That(tappedPoint.Value.x, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(tappedPoint.Value.y, Is.EqualTo(0.5f).Within(0.001f));

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

        private void AssignUvMesh(MeshRenderer target, params Vector2[] uvs)
        {
            Assert.That(uvs, Has.Length.EqualTo(4));
            mesh = new Mesh { name = "ColonySurfaceTestMesh" };
            mesh.vertices = new[]
            {
                new Vector3(-1f, 0f, -1f),
                new Vector3(1f, 0f, -1f),
                new Vector3(1f, 0f, 1f),
                new Vector3(-1f, 0f, 1f)
            };
            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            mesh.uv = uvs;
            target.GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        private static void AssertVectorApproximately(Vector4 expected, Vector4 actual)
        {
            Assert.That(Vector4.Distance(actual, expected), Is.LessThan(0.000001f));
        }
    }
}
