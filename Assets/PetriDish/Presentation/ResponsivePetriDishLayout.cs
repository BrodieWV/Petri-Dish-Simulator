using UnityEngine;

namespace PetriDish.Presentation
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class ResponsivePetriDishLayout : MonoBehaviour
    {
        [Header("Layout References")]
        [SerializeField] private RectTransform leftPanel;
        [SerializeField] private RectTransform centrePanel;
        [SerializeField] private RectTransform rightPanel;
        [SerializeField] private RectTransform leftDrawerButton;
        [SerializeField] private RectTransform rightDrawerButton;

        [Header("Responsive Breakpoint")]
        [SerializeField, Min(1f)] private float compactAspectThreshold = 1.65f;
        [SerializeField, Range(0.18f, 0.36f)] private float desktopSidePanelWidth = 0.23f;
        [SerializeField, Range(0.50f, 0.82f)] private float compactDrawerWidth = 0.72f;

        [Header("Preview")]
        [SerializeField] private bool forceCompactPreview;

        private bool leftDrawerOpen;
        private bool rightDrawerOpen;
        private Vector2Int lastScreenSize;

        public bool IsCompact { get; private set; }

        private void OnEnable()
        {
            ApplyLayout();
        }

        private void Update()
        {
            var currentSize = new Vector2Int(Screen.width, Screen.height);
            if (currentSize == lastScreenSize && Application.isPlaying)
                return;

            lastScreenSize = currentSize;
            ApplyLayout();
        }

        public void Configure(RectTransform left, RectTransform centre, RectTransform right, RectTransform leftButton, RectTransform rightButton)
        {
            leftPanel = left;
            centrePanel = centre;
            rightPanel = right;
            leftDrawerButton = leftButton;
            rightDrawerButton = rightButton;
            ApplyLayout();
        }

        public void ToggleLeftDrawer()
        {
            leftDrawerOpen = !leftDrawerOpen;
            if (leftDrawerOpen) rightDrawerOpen = false;
            ApplyLayout();
        }

        public void ToggleRightDrawer()
        {
            rightDrawerOpen = !rightDrawerOpen;
            if (rightDrawerOpen) leftDrawerOpen = false;
            ApplyLayout();
        }

        public void CloseDrawers()
        {
            leftDrawerOpen = false;
            rightDrawerOpen = false;
            ApplyLayout();
        }

        [ContextMenu("Apply Responsive Layout")]
        public void ApplyLayout()
        {
            if (leftPanel == null || centrePanel == null || rightPanel == null) return;

            float aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 1.777f;
            IsCompact = forceCompactPreview || aspect < compactAspectThreshold;

            if (IsCompact) ApplyCompactLayout();
            else ApplyDesktopLayout();
        }

        private void ApplyDesktopLayout()
        {
            leftDrawerOpen = false;
            rightDrawerOpen = false;

            SetActive(leftPanel, true);
            SetActive(rightPanel, true);
            SetActive(leftDrawerButton, false);
            SetActive(rightDrawerButton, false);

            SetRect(leftPanel, new Vector2(0f, 0f), new Vector2(desktopSidePanelWidth, 1f));
            SetRect(centrePanel, new Vector2(desktopSidePanelWidth, 0f), new Vector2(1f - desktopSidePanelWidth, 1f));
            SetRect(rightPanel, new Vector2(1f - desktopSidePanelWidth, 0f), Vector2.one);
        }

        private void ApplyCompactLayout()
        {
            SetActive(leftDrawerButton, true);
            SetActive(rightDrawerButton, true);
            SetRect(centrePanel, Vector2.zero, Vector2.one);

            SetActive(leftPanel, leftDrawerOpen);
            SetActive(rightPanel, rightDrawerOpen);

            if (leftDrawerOpen) SetRect(leftPanel, Vector2.zero, new Vector2(compactDrawerWidth, 1f));
            if (rightDrawerOpen) SetRect(rightPanel, new Vector2(1f - compactDrawerWidth, 0f), Vector2.one);
        }

        private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
        {
            if (rect == null) return;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetActive(RectTransform rect, bool active)
        {
            if (rect != null && rect.gameObject.activeSelf != active) rect.gameObject.SetActive(active);
        }
    }
}
