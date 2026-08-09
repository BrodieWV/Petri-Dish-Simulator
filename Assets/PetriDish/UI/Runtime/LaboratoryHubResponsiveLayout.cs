using UnityEngine;
using UnityEngine.UI;

namespace PetriDish.Presentation.UI
{
    [ExecuteAlways]
    public sealed class LaboratoryHubResponsiveLayout : MonoBehaviour
    {
        [SerializeField] private PetriDishUITheme theme;
        [SerializeField] private RectTransform observedRoot;
        [SerializeField] private LayoutElement navigation;
        [SerializeField] private GameObject[] navigationLabels;
        [SerializeField] private HorizontalLayoutGroup columns;
        [SerializeField] private LayoutElement activeDishes;
        [SerializeField] private AdaptiveDishCardLayoutGroup activeDishLayout;
        [SerializeField] private LayoutElement activity;
        [SerializeField] private GameObject activityDrawerButton;
        [SerializeField] private GameObject activityDrawer;
        [SerializeField] private bool forceCompactLandscape;
        private Vector2 lastSize;
        private bool lastForced;
        public bool IsCompact { get; private set; }

        private void OnEnable() { Refresh(); }
        private void Update()
        {
            RectTransform root = observedRoot != null ? observedRoot : transform as RectTransform;
            Vector2 size = root != null ? root.rect.size : new Vector2(Screen.width, Screen.height);
            if (size != lastSize || lastForced != forceCompactLandscape) Refresh();
        }

        public static bool ShouldUseCompactLayout(float width, float height, bool forced, float aspectThreshold = 1.95f, float widthThreshold = 1200f)
        {
            if (forced) return true;
            if (width <= 0f || height <= 0f || width < height) return false;
            return width <= widthThreshold || width / height >= aspectThreshold;
        }

        public void Configure(PetriDishUITheme value, RectTransform root, LayoutElement nav, GameObject[] labels,
            HorizontalLayoutGroup columnGroup, LayoutElement dishes, AdaptiveDishCardLayoutGroup dishLayout,
            LayoutElement activityPanel,
            GameObject drawerButton, GameObject drawer)
        {
            theme = value; observedRoot = root; navigation = nav; navigationLabels = labels; columns = columnGroup;
            activeDishes = dishes; activeDishLayout = dishLayout;
            activity = activityPanel; activityDrawerButton = drawerButton; activityDrawer = drawer; Refresh();
        }

        public void Refresh()
        {
            RectTransform root = observedRoot != null ? observedRoot : transform as RectTransform;
            Vector2 size = root != null ? root.rect.size : new Vector2(Screen.width, Screen.height);
            float aspect = theme != null ? theme.compactLandscapeAspect : 1.95f;
            float widthLimit = theme != null ? theme.compactLandscapeWidth : 1200f;
            IsCompact = ShouldUseCompactLayout(size.x, size.y, forceCompactLandscape, aspect, widthLimit);
            if (columns != null) columns.spacing = theme != null ? (IsCompact ? theme.compactSpacing : theme.standardSpacing) : (IsCompact ? 12f : 18f);
            if (navigation != null) navigation.preferredWidth = theme != null ? (IsCompact ? theme.compactNavigationWidth : theme.navigationWidth) : (IsCompact ? 76f : 184f);
            if (activeDishes != null) activeDishes.preferredWidth = IsCompact ? -1f : (theme != null ? theme.activeDishesWidth : 330f);
            if (activeDishLayout != null) activeDishLayout.IsVertical = !IsCompact;
            if (activity != null) { activity.preferredWidth = theme != null ? theme.activityWidth : 320f; activity.gameObject.SetActive(!IsCompact); }
            if (activityDrawerButton != null) activityDrawerButton.SetActive(IsCompact);
            if (!IsCompact && activityDrawer != null) activityDrawer.SetActive(false);
            if (navigationLabels != null) foreach (GameObject label in navigationLabels) if (label != null) label.SetActive(!IsCompact);
            lastSize = size; lastForced = forceCompactLandscape;
        }

        public void ToggleActivityDrawer() { if (activityDrawer != null) activityDrawer.SetActive(!activityDrawer.activeSelf); }
    }
}
