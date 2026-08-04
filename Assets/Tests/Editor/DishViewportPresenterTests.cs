using System.Reflection;
using NUnit.Framework;
using PetriDish.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace PetriDish.Tests.Editor
{
    public sealed class DishViewportPresenterTests
    {
        private GameObject rootObject;
        private GameObject safeAreaObject;
        private GameObject viewportObject;
        private GameObject fallbackObject;
        private GameObject sourceObject;

        [TearDown]
        public void TearDown()
        {
            if (rootObject != null) Object.DestroyImmediate(rootObject);
            if (safeAreaObject != null) Object.DestroyImmediate(safeAreaObject);
            if (viewportObject != null) Object.DestroyImmediate(viewportObject);
            if (fallbackObject != null) Object.DestroyImmediate(fallbackObject);
            if (sourceObject != null) Object.DestroyImmediate(sourceObject);
        }

        [Test]
        public void BackgroundRegionsCoverOnlyTheAreaOutsideTheViewport()
        {
            var regions = new Rect[4];
            Rect outer = Rect.MinMaxRect(-500f, -1000f, 500f, 1000f);
            Rect viewport = Rect.MinMaxRect(-350f, -420f, 350f, 420f);

            DishViewportPresenter.CalculateRegions(outer, viewport, regions);

            float coveredArea = 0f;
            for (int i = 0; i < regions.Length; i++)
            {
                Assert.That(HasPositiveOverlap(regions[i], viewport), Is.False);
                Assert.That(regions[i].xMin, Is.GreaterThanOrEqualTo(outer.xMin));
                Assert.That(regions[i].xMax, Is.LessThanOrEqualTo(outer.xMax));
                Assert.That(regions[i].yMin, Is.GreaterThanOrEqualTo(outer.yMin));
                Assert.That(regions[i].yMax, Is.LessThanOrEqualTo(outer.yMax));
                coveredArea += regions[i].width * regions[i].height;
            }

            float expectedArea = outer.width * outer.height - viewport.width * viewport.height;
            Assert.That(coveredArea, Is.EqualTo(expectedArea).Within(0.01f));
        }

        [Test]
        public void FlatDishVisibilityControlsFallbackBackdropWithoutDisablingTapSurface()
        {
            DishRenderer source = CreateSource();
            DishViewportPresenter presenter = CreateViewportPresenter(source, out Image fallback);
            RawImage tapSurface = source.GetComponent<RawImage>();

            Assert.That(fallback.color.a, Is.EqualTo(0.8f).Within(0.001f));
            source.SetFlatPresentationVisible(false);

            Assert.That(source.FlatPresentationVisible, Is.False);
            Assert.That(tapSurface.color.a, Is.Zero);
            Assert.That(tapSurface.raycastTarget, Is.True);
            Assert.That(fallback.color.a, Is.Zero);

            source.SetFlatPresentationVisible(true);

            Assert.That(source.FlatPresentationVisible, Is.True);
            Assert.That(tapSurface.color.a, Is.GreaterThan(0f));
            Assert.That(tapSurface.raycastTarget, Is.True);
            Assert.That(fallback.color.a, Is.EqualTo(0.8f).Within(0.001f));
            Assert.That(presenter.TransparentViewportRect.width, Is.GreaterThan(0f));
        }

        [Test]
        public void LayoutRefreshTracksSafeAreaMovement()
        {
            DishRenderer source = CreateSource();
            DishViewportPresenter presenter = CreateViewportPresenter(source, out _);
            Rect firstViewport = presenter.TransparentViewportRect;

            RectTransform safeArea = safeAreaObject.GetComponent<RectTransform>();
            safeArea.anchoredPosition += new Vector2(25f, 140f);
            presenter.RefreshLayout();
            Rect movedViewport = presenter.TransparentViewportRect;

            Assert.That(movedViewport.position, Is.Not.EqualTo(firstViewport.position));
            Assert.That(movedViewport.x, Is.EqualTo(firstViewport.x + 25f).Within(0.01f));
            Assert.That(movedViewport.y, Is.EqualTo(firstViewport.y + 140f).Within(0.01f));
            for (int i = 0; i < 4; i++)
                Assert.That(HasPositiveOverlap(presenter.GetBackgroundRegion(i), movedViewport), Is.False);
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

        private DishViewportPresenter CreateViewportPresenter(DishRenderer source, out Image fallback)
        {
            rootObject = new GameObject(
                "Background",
                typeof(RectTransform),
                typeof(DishViewportPresenter));
            RectTransform root = rootObject.GetComponent<RectTransform>();
            root.sizeDelta = new Vector2(1000f, 2000f);

            safeAreaObject = new GameObject("SafeArea", typeof(RectTransform));
            RectTransform safeArea = safeAreaObject.GetComponent<RectTransform>();
            safeArea.SetParent(root, false);
            safeArea.sizeDelta = new Vector2(900f, 1800f);

            viewportObject = new GameObject("DishPanel", typeof(RectTransform));
            RectTransform viewport = viewportObject.GetComponent<RectTransform>();
            viewport.SetParent(safeArea, false);
            viewport.sizeDelta = new Vector2(760f, 820f);
            viewport.anchoredPosition = new Vector2(0f, 120f);

            fallbackObject = new GameObject(
                "Fallback",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            fallback = fallbackObject.GetComponent<Image>();
            fallback.color = new Color(0.1f, 0.2f, 0.15f, 0.8f);

            DishViewportPresenter presenter = rootObject.GetComponent<DishViewportPresenter>();
            presenter.Configure(viewport, fallback, source, Color.black);
            return presenter;
        }

        private static bool HasPositiveOverlap(Rect left, Rect right)
        {
            return Mathf.Min(left.xMax, right.xMax) > Mathf.Max(left.xMin, right.xMin) &&
                   Mathf.Min(left.yMax, right.yMax) > Mathf.Max(left.yMin, right.yMin);
        }
    }
}
