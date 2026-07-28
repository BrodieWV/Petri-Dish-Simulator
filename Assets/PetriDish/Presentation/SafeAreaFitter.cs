using UnityEngine;

namespace PetriDish.Presentation
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform target;
        private Rect lastSafeArea;
        private int lastScreenWidth;
        private int lastScreenHeight;

        private void Awake()
        {
            target = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            if (Screen.safeArea == lastSafeArea &&
                Screen.width == lastScreenWidth &&
                Screen.height == lastScreenHeight)
                return;

            Apply();
        }

        private void Apply()
        {
            if (target == null) target = GetComponent<RectTransform>();

            Rect safeArea = Screen.safeArea;
            CalculateAnchors(safeArea, Screen.width, Screen.height, out Vector2 anchorMin, out Vector2 anchorMax);
            target.anchorMin = anchorMin;
            target.anchorMax = anchorMax;
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;

            lastSafeArea = safeArea;
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }

        public static void CalculateAnchors(
            Rect safeArea,
            float screenWidth,
            float screenHeight,
            out Vector2 anchorMin,
            out Vector2 anchorMax)
        {
            if (screenWidth <= 0f || screenHeight <= 0f)
            {
                anchorMin = Vector2.zero;
                anchorMax = Vector2.one;
                return;
            }

            anchorMin = new Vector2(
                Mathf.Clamp01(safeArea.xMin / screenWidth),
                Mathf.Clamp01(safeArea.yMin / screenHeight));
            anchorMax = new Vector2(
                Mathf.Clamp01(safeArea.xMax / screenWidth),
                Mathf.Clamp01(safeArea.yMax / screenHeight));
        }
    }
}
