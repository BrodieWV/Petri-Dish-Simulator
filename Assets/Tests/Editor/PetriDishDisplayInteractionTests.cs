using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PetriDish.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PetriDish.Tests.Editor
{
    public sealed class PetriDishDisplayInteractionTests
    {
        private GameObject root;
        private GameObject canvasOwner;
        private GameObject eventSystemOwner;

        [TearDown]
        public void TearDown()
        {
            if (root != null) Object.DestroyImmediate(root);
            if (canvasOwner != null) Object.DestroyImmediate(canvasOwner);
            if (eventSystemOwner != null) Object.DestroyImmediate(eventSystemOwner);
        }

        [Test]
        public void OrbitIsConstrainedAndResetRestoresAuthoredView()
        {
            PetriDishDisplayPresenter presenter = CreatePresenter(out Transform pivot, out _, out _);
            Quaternion authoredRotation = Quaternion.Euler(4f, 7f, 0f);
            pivot.localRotation = authoredRotation;
            presenter.ConfigureRig(pivot, presenter.DisplayCamera, null);

            presenter.OrbitBy(new Vector2(10000f, -10000f));

            Assert.That(presenter.YawDegrees, Is.EqualTo(55f).Within(0.001f));
            Assert.That(presenter.PitchDegrees, Is.EqualTo(32f).Within(0.001f));
            Assert.That(Quaternion.Angle(pivot.localRotation, authoredRotation), Is.GreaterThan(1f));

            presenter.ResetView();

            Assert.That(presenter.YawDegrees, Is.Zero.Within(0.001f));
            Assert.That(presenter.PitchDegrees, Is.Zero.Within(0.001f));
            Assert.That(presenter.Zoom, Is.EqualTo(1f).Within(0.001f));
            Assert.That(Quaternion.Angle(pivot.localRotation, authoredRotation), Is.LessThan(0.001f));
        }

        [Test]
        public void ZoomIsBoundedAndMaintainsValidCameraClipping()
        {
            PetriDishDisplayPresenter presenter = CreatePresenter(out _, out Camera camera, out _);

            presenter.ZoomBy(100f);
            float nearDistance = camera.transform.position.magnitude;

            Assert.That(presenter.Zoom, Is.EqualTo(1.18f).Within(0.001f));
            Assert.That(camera.nearClipPlane, Is.GreaterThan(0f));
            Assert.That(camera.farClipPlane, Is.GreaterThan(camera.nearClipPlane));

            presenter.ZoomBy(-100f);
            float farDistance = camera.transform.position.magnitude;

            Assert.That(presenter.Zoom, Is.EqualTo(0.78f).Within(0.001f));
            Assert.That(farDistance, Is.GreaterThan(nearDistance));
            Assert.That(camera.nearClipPlane, Is.GreaterThan(0f));
            Assert.That(camera.farClipPlane, Is.GreaterThan(camera.nearClipPlane));
        }

        [Test]
        public void GestureMustBeginInsideDishOutput()
        {
            PetriDishDisplayPresenter presenter = CreatePresenter(out _, out _, out RawImage output);
            Vector2 centre = RectTransformUtility.WorldToScreenPoint(null, output.rectTransform.position);

            Assert.That(presenter.CanBeginInteraction(centre), Is.True);
            Assert.That(presenter.CanBeginInteraction(centre + Vector2.right * 1000f), Is.False);
        }

        [Test]
        public void InteractiveUiOverDishBlocksGestureStart()
        {
            PetriDishDisplayPresenter presenter = CreatePresenter(out _, out _, out RawImage output);
            eventSystemOwner = new GameObject("EventSystem", typeof(EventSystem));
            GameObject buttonOwner = new GameObject(
                "OverlayButton",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            RectTransform buttonRect = buttonOwner.GetComponent<RectTransform>();
            buttonRect.SetParent(output.transform.parent, false);
            buttonRect.sizeDelta = new Vector2(120f, 80f);
            buttonRect.anchoredPosition = Vector2.zero;
            Canvas.ForceUpdateCanvases();
            Vector2 centre = RectTransformUtility.WorldToScreenPoint(null, buttonRect.position);

            Assert.That(presenter.CanBeginInteraction(centre), Is.False);

            Object.DestroyImmediate(buttonOwner);
        }

        [Test]
        public void PlainRaycastableOverlayOverDishBlocksGestureStart()
        {
            PetriDishDisplayPresenter presenter = CreatePresenter(out _, out _, out RawImage output);
            eventSystemOwner = new GameObject("EventSystem", typeof(EventSystem));
            GameObject overlayOwner = new GameObject(
                "NotesDrawerOverlay",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            RectTransform overlayRect = overlayOwner.GetComponent<RectTransform>();
            overlayRect.SetParent(output.transform.parent, false);
            overlayRect.sizeDelta = new Vector2(180f, 180f);
            overlayRect.anchoredPosition = Vector2.zero;
            overlayOwner.GetComponent<Image>().raycastTarget = true;
            Canvas.ForceUpdateCanvases();
            Vector2 centre = RectTransformUtility.WorldToScreenPoint(null, overlayRect.position);

            Assert.That(presenter.CanBeginInteraction(centre), Is.False);

            Object.DestroyImmediate(overlayOwner);
        }

        [Test]
        public void PresenterRemainsPresentationOnly()
        {
            FieldInfo[] fields = typeof(PetriDishDisplayPresenter).GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            Assert.That(fields.Any(field =>
                field.FieldType.FullName != null &&
                (field.FieldType.FullName.Contains("PetriDish.Simulation") ||
                 field.FieldType.FullName.Contains("PetriDish.Application"))), Is.False);
        }

        private PetriDishDisplayPresenter CreatePresenter(
            out Transform pivot,
            out Camera displayCamera,
            out RawImage output)
        {
            root = new GameObject("DishDisplay", typeof(PetriDishDisplayPresenter));
            pivot = new GameObject("RotationPivot").transform;
            pivot.SetParent(root.transform, false);
            GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cube);
            model.name = "DishBounds";
            model.transform.SetParent(pivot, false);
            model.transform.localScale = new Vector3(4f, 0.5f, 4f);

            GameObject cameraOwner = new GameObject("DisplayCamera", typeof(Camera));
            cameraOwner.transform.SetParent(root.transform, false);
            displayCamera = cameraOwner.GetComponent<Camera>();
            displayCamera.fieldOfView = 32f;

            canvasOwner = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(GraphicRaycaster));
            Canvas canvas = canvasOwner.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            RectTransform canvasRect = canvasOwner.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(800f, 600f);

            GameObject outputOwner = new GameObject(
                "DishDisplayImage",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(RawImage));
            outputOwner.transform.SetParent(canvasOwner.transform, false);
            RectTransform outputRect = outputOwner.GetComponent<RectTransform>();
            outputRect.sizeDelta = new Vector2(400f, 400f);
            output = outputOwner.GetComponent<RawImage>();
            output.raycastTarget = true;

            PetriDishDisplayPresenter presenter = root.GetComponent<PetriDishDisplayPresenter>();
            presenter.ConfigureRig(pivot, displayCamera, null);
            presenter.ConfigureOutput(output);
            Canvas.ForceUpdateCanvases();
            return presenter;
        }
    }
}
