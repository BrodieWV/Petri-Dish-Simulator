using System;
using UnityEngine;
using UnityEngine.UI;

namespace PetriDish.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class DishViewportPresenter : MonoBehaviour
    {
        private const int RegionCount = 4;

        private readonly Vector3[] viewportWorldCorners = new Vector3[4];
        private readonly Rect[] regionRects = new Rect[RegionCount];
        private readonly Image[] backgroundPanels = new Image[RegionCount];

        private RectTransform root;
        private RectTransform viewport;
        private Graphic fallbackBackdrop;
        private DishRenderer textureSource;
        private Color backgroundColor;
        private float fallbackVisibleAlpha = 1f;
        private Rect transparentViewportRect;
        private Rect lastOuterRect;
        private bool configured;
        private bool subscribed;

        public Rect TransparentViewportRect => transparentViewportRect;

        public void Configure(
            RectTransform viewportRect,
            Graphic flatDishBackdrop,
            DishRenderer source,
            Color outerBackgroundColor)
        {
            if (viewportRect == null) throw new ArgumentNullException(nameof(viewportRect));
            if (flatDishBackdrop == null) throw new ArgumentNullException(nameof(flatDishBackdrop));
            if (source == null) throw new ArgumentNullException(nameof(source));

            Unsubscribe();
            root = GetComponent<RectTransform>();
            viewport = viewportRect;
            fallbackBackdrop = flatDishBackdrop;
            textureSource = source;
            backgroundColor = outerBackgroundColor;
            if (fallbackBackdrop.color.a > 0f)
                fallbackVisibleAlpha = fallbackBackdrop.color.a;

            EnsureBackgroundPanels();
            configured = true;
            Subscribe();
            ApplyFlatPresentationVisibility(textureSource.FlatPresentationVisible);
            RefreshLayout(true);
        }

        private void OnEnable()
        {
            if (!configured) return;
            Subscribe();
            ApplyFlatPresentationVisibility(textureSource.FlatPresentationVisible);
            RefreshLayout(true);
        }

        private void OnDisable()
        {
            Unsubscribe();
            ApplyFlatPresentationVisibility(true);
        }

        private void LateUpdate()
        {
            if (configured) RefreshLayout(false);
        }

        public void RefreshLayout(bool force = false)
        {
            if (!configured || root == null || viewport == null) return;

            Rect outer = root.rect;
            Rect hole = CalculateViewportRect(root, viewport, viewportWorldCorners, outer);
            if (!force && outer == lastOuterRect && hole == transparentViewportRect) return;

            CalculateRegions(outer, hole, regionRects);
            for (int i = 0; i < RegionCount; i++)
                ApplyRect(backgroundPanels[i].rectTransform, regionRects[i]);

            lastOuterRect = outer;
            transparentViewportRect = hole;
        }

        public Rect GetBackgroundRegion(int index)
        {
            if (index < 0 || index >= RegionCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return regionRects[index];
        }

        public static void CalculateRegions(Rect outer, Rect viewportRect, Rect[] destination)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (destination.Length < RegionCount)
                throw new ArgumentException("Four destination regions are required.", nameof(destination));

            float left = Mathf.Clamp(viewportRect.xMin, outer.xMin, outer.xMax);
            float right = Mathf.Clamp(viewportRect.xMax, left, outer.xMax);
            float bottom = Mathf.Clamp(viewportRect.yMin, outer.yMin, outer.yMax);
            float top = Mathf.Clamp(viewportRect.yMax, bottom, outer.yMax);

            destination[0] = Rect.MinMaxRect(outer.xMin, outer.yMin, left, outer.yMax);
            destination[1] = Rect.MinMaxRect(right, outer.yMin, outer.xMax, outer.yMax);
            destination[2] = Rect.MinMaxRect(left, outer.yMin, right, bottom);
            destination[3] = Rect.MinMaxRect(left, top, right, outer.yMax);
        }

        private static Rect CalculateViewportRect(
            RectTransform rootTransform,
            RectTransform viewportTransform,
            Vector3[] worldCorners,
            Rect outer)
        {
            viewportTransform.GetWorldCorners(worldCorners);
            Vector3 first = rootTransform.InverseTransformPoint(worldCorners[0]);
            float xMin = first.x;
            float xMax = first.x;
            float yMin = first.y;
            float yMax = first.y;
            for (int i = 1; i < worldCorners.Length; i++)
            {
                Vector3 local = rootTransform.InverseTransformPoint(worldCorners[i]);
                xMin = Mathf.Min(xMin, local.x);
                xMax = Mathf.Max(xMax, local.x);
                yMin = Mathf.Min(yMin, local.y);
                yMax = Mathf.Max(yMax, local.y);
            }

            return Rect.MinMaxRect(
                Mathf.Clamp(xMin, outer.xMin, outer.xMax),
                Mathf.Clamp(yMin, outer.yMin, outer.yMax),
                Mathf.Clamp(xMax, outer.xMin, outer.xMax),
                Mathf.Clamp(yMax, outer.yMin, outer.yMax));
        }

        private void EnsureBackgroundPanels()
        {
            for (int i = 0; i < RegionCount; i++)
            {
                if (backgroundPanels[i] == null)
                {
                    var panel = new GameObject(
                        $"OuterBackground{i + 1}",
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Image));
                    panel.transform.SetParent(root, false);
                    backgroundPanels[i] = panel.GetComponent<Image>();
                }

                backgroundPanels[i].color = backgroundColor;
                backgroundPanels[i].raycastTarget = false;
            }
        }

        private void Subscribe()
        {
            if (subscribed || textureSource == null || !isActiveAndEnabled) return;
            textureSource.FlatPresentationVisibilityChanged += ApplyFlatPresentationVisibility;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed) return;
            if (textureSource != null)
                textureSource.FlatPresentationVisibilityChanged -= ApplyFlatPresentationVisibility;
            subscribed = false;
        }

        private void ApplyFlatPresentationVisibility(bool visible)
        {
            if (fallbackBackdrop == null) return;
            Color color = fallbackBackdrop.color;
            if (visible)
                color.a = fallbackVisibleAlpha;
            else
            {
                if (color.a > 0f) fallbackVisibleAlpha = color.a;
                color.a = 0f;
            }

            fallbackBackdrop.color = color;
        }

        private static void ApplyRect(RectTransform target, Rect rect)
        {
            target.anchorMin = new Vector2(0.5f, 0.5f);
            target.anchorMax = new Vector2(0.5f, 0.5f);
            target.pivot = new Vector2(0.5f, 0.5f);
            target.anchoredPosition = rect.center;
            target.sizeDelta = rect.size;
            target.localScale = Vector3.one;
        }
    }
}
