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
        [SerializeField] private LayoutElement notes;
        [SerializeField] private LayoutElement actionsLeftInset;
        [SerializeField] private LayoutElement actionsRightInset;
        [SerializeField] private GameObject notesDrawerButton;
        [SerializeField] private GameObject notesDrawer;
        [SerializeField] private bool forceCompactLandscape;
        private Vector2 lastSize;
        private bool lastForced;
        private Button notesDrawerToggle;
        private bool notesDrawerListenerBound;

        public bool IsCompact { get; private set; }

        private void OnEnable()
        {
            BindNotesDrawerButton();
            Refresh();
        }

        private void OnDisable() => UnbindNotesDrawerButton();

        private void Update()
        {
            RectTransform root = observedRoot != null ? observedRoot : transform as RectTransform;
            Vector2 size = root != null ? root.rect.size : new Vector2(Screen.width, Screen.height);
            if (size != lastSize || lastForced != forceCompactLandscape) Refresh();
        }

        public static bool ShouldUseCompactLayout(float width, float height, bool forced,
            float aspectThreshold = 1.95f, float widthThreshold = 1200f)
        {
            if (forced) return true;
            if (width <= 0f || height <= 0f || width < height) return false;
            return width <= widthThreshold || width / height >= aspectThreshold;
        }

        public void Configure(PetriDishUITheme value, RectTransform root, LayoutElement nav,
            GameObject[] labels, HorizontalLayoutGroup columnGroup, LayoutElement notesPanel,
            LayoutElement actionLeft, LayoutElement actionRight, GameObject drawerButton, GameObject drawer)
        {
            theme = value;
            observedRoot = root;
            navigation = nav;
            navigationLabels = labels;
            columns = columnGroup;
            notes = notesPanel;
            actionsLeftInset = actionLeft;
            actionsRightInset = actionRight;
            notesDrawerButton = drawerButton;
            notesDrawer = drawer;
            BindNotesDrawerButton();
            Refresh();
        }

        public void Refresh()
        {
            RectTransform root = observedRoot != null ? observedRoot : transform as RectTransform;
            Vector2 size = root != null ? root.rect.size : new Vector2(Screen.width, Screen.height);
            float aspect = theme != null ? theme.compactLandscapeAspect : 1.95f;
            float widthLimit = theme != null ? theme.compactLandscapeWidth : 1200f;
            IsCompact = ShouldUseCompactLayout(size.x, size.y, forceCompactLandscape, aspect, widthLimit);

            if (columns != null)
                columns.spacing = theme != null
                    ? (IsCompact ? theme.compactSpacing : theme.standardSpacing)
                    : (IsCompact ? 12f : 18f);
            if (navigation != null)
                navigation.preferredWidth = theme != null
                    ? (IsCompact ? theme.compactNavigationWidth : theme.navigationWidth)
                    : (IsCompact ? 72f : 194f);
            if (notes != null)
            {
                notes.preferredWidth = theme != null ? theme.notesWidth : 340f;
                notes.gameObject.SetActive(!IsCompact);
            }
            if (actionsLeftInset != null)
                actionsLeftInset.preferredWidth = theme != null
                    ? (IsCompact ? theme.compactNavigationWidth : theme.navigationWidth) +
                      (IsCompact ? theme.compactSpacing : theme.standardSpacing)
                    : (IsCompact ? 84f : 244f);
            if (actionsRightInset != null)
                actionsRightInset.preferredWidth = IsCompact
                    ? 0f
                    : (theme != null ? theme.notesWidth + theme.standardSpacing : 380f);
            if (notesDrawerButton != null) notesDrawerButton.SetActive(IsCompact);
            if (!IsCompact && notesDrawer != null) notesDrawer.SetActive(false);
            if (navigationLabels != null)
                foreach (GameObject label in navigationLabels)
                    if (label != null) label.SetActive(!IsCompact);

            lastSize = size;
            lastForced = forceCompactLandscape;
        }

        public void ToggleNotesDrawer()
        {
            if (notesDrawer != null) notesDrawer.SetActive(!notesDrawer.activeSelf);
        }

        private void BindNotesDrawerButton()
        {
            Button resolved = notesDrawerButton != null ? notesDrawerButton.GetComponent<Button>() : null;
            if (notesDrawerListenerBound && notesDrawerToggle == resolved) return;
            UnbindNotesDrawerButton();
            notesDrawerToggle = resolved;
            if (notesDrawerToggle == null) return;
            notesDrawerToggle.onClick.AddListener(ToggleNotesDrawer);
            notesDrawerListenerBound = true;
        }

        private void UnbindNotesDrawerButton()
        {
            if (notesDrawerListenerBound && notesDrawerToggle != null)
                notesDrawerToggle.onClick.RemoveListener(ToggleNotesDrawer);
            notesDrawerListenerBound = false;
            notesDrawerToggle = null;
        }
    }
}
