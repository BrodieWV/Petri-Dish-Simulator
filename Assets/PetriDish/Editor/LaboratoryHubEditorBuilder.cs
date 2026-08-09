using System;
using System.Collections.Generic;
using PetriDish.Presentation;
using PetriDish.Presentation.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PetriDish.Editor
{
    public static class LaboratoryHubEditorBuilder
    {
        public const string ScenePath = "Assets/PetriDish/Scenes/LaboratoryHub.unity";
        public const string ThemePath = "Assets/PetriDish/UI/Styles/PetriDishUITheme.asset";
        private const string CommonPath = "Assets/PetriDish/UI/Prefabs/Common/";
        private const string NavigationPath = "Assets/PetriDish/UI/Prefabs/Navigation/";
        private const string ExperimentsPath = "Assets/PetriDish/UI/Prefabs/Experiments/";
        private const string LaboratoryPath = "Assets/PetriDish/UI/Prefabs/Laboratory/";

        [MenuItem("Petri Dish/Build Laboratory Hub")]
        public static void BuildLaboratoryHub()
        {
            if (global::UnityEngine.Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Laboratory Hub", "Stop Play Mode before rebuilding the hub.", "OK");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null &&
                !EditorUtility.DisplayDialog("Rebuild Laboratory Hub?",
                    "This replaces only LaboratoryHub.unity. Reusable theme values and existing prefabs are preserved.",
                    "Rebuild", "Cancel"))
                return;

            BuildLaboratoryHubForAutomation();
            EditorUtility.DisplayDialog("Laboratory Hub",
                "LaboratoryHub.unity and its reusable UI assets are ready for editing.", "OK");
        }

        public static void BuildLaboratoryHubForAutomation()
        {
            EnsureFolders();
            PetriDishUITheme theme = EnsureTheme();
            EnsurePrefabs(theme);
            BuildScene(theme);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/PetriDish", "UI");
            EnsureFolder("Assets/PetriDish/UI", "Runtime");
            EnsureFolder("Assets/PetriDish/UI", "Styles");
            EnsureFolder("Assets/PetriDish/UI", "Prefabs");
            EnsureFolder("Assets/PetriDish/UI/Prefabs", "Common");
            EnsureFolder("Assets/PetriDish/UI/Prefabs", "Navigation");
            EnsureFolder("Assets/PetriDish/UI/Prefabs", "Experiments");
            EnsureFolder("Assets/PetriDish/UI/Prefabs", "Laboratory");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }

        private static PetriDishUITheme EnsureTheme()
        {
            PetriDishUITheme theme = AssetDatabase.LoadAssetAtPath<PetriDishUITheme>(ThemePath);
            if (theme != null) return theme;
            theme = ScriptableObject.CreateInstance<PetriDishUITheme>();
            AssetDatabase.CreateAsset(theme, ThemePath);
            return theme;
        }

        private static void EnsurePrefabs(PetriDishUITheme theme)
        {
            EnsurePrefab(CommonPath + "Panel.prefab", () => CreatePanel("Panel", null, theme.panel));
            EnsurePrefab(CommonPath + "PrimaryButton.prefab", () => CreateButton("PrimaryButton", null, "Primary action", theme, true).gameObject);
            EnsurePrefab(CommonPath + "SecondaryButton.prefab", () => CreateButton("SecondaryButton", null, "Secondary action", theme, false).gameObject);
            EnsurePrefab(CommonPath + "Modal.prefab", () => CreateModalPrefab(theme));
            EnsurePrefab(NavigationPath + "NavigationButton.prefab", () => CreateNavigationButton("NavigationButton", null, "Lab", "L", theme, false).gameObject);
            EnsurePrefab(NavigationPath + "NavigationRail.prefab", () => CreateNavigationRailPrefab(theme));
            EnsurePrefab(ExperimentsPath + "ExperimentStatusBadge.prefab", () => CreateStatusBadge("ExperimentStatusBadge", null, "Growing", theme.green, theme).gameObject);
            EnsurePrefab(ExperimentsPath + "DishCard.prefab", () => CreateDishCard("DishCard", null, "Dish A", "E. coli", "Nutrient Agar", "18h", "Growing", "42%", "", theme.green, theme));
            EnsurePrefab(ExperimentsPath + "FeaturedDishCard.prefab", () => CreateFeaturedDish("FeaturedDishCard", null, theme, null));
            EnsurePrefab(LaboratoryPath + "LaboratoryActivityCard.prefab", () => CreateActivityCard("LaboratoryActivityCard", null, "LAB ACTIVITY", "Activity title", "Mock laboratory update", theme.cyan, theme));
            EnsurePrefab(LaboratoryPath + "DiscoveryCard.prefab", () => CreateActivityCard("DiscoveryCard", null, "LATEST DISCOVERY", "Colony edge pattern", "Observed in Dish A ? 12 min ago", theme.cyan, theme));
            EnsurePrefab(LaboratoryPath + "ChallengeCard.prefab", () => CreateActivityCard("ChallengeCard", null, "CURRENT CHALLENGE", "Maintain stable growth", "18 of 24 hours complete", theme.amber, theme));
        }

        private static void EnsurePrefab(string path, Func<GameObject> factory)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            GameObject temporary = factory();
            PrefabUtility.SaveAsPrefabAsset(temporary, path);
            UnityEngine.Object.DestroyImmediate(temporary);
        }

        private static GameObject CreateModalPrefab(PetriDishUITheme theme)
        {
            GameObject modal = CreatePanel("Modal", null, new Color(0.015f, 0.020f, 0.022f, 0.94f));
            RectTransform card = CreatePanel("Dialog", modal.transform, theme.panelRaised).GetComponent<RectTransform>();
            Anchor(card, new Vector2(0.24f, 0.28f), new Vector2(0.76f, 0.72f), Vector2.zero, Vector2.zero);
            CreateText("Title", card, "Laboratory message", 28, FontStyle.Bold, TextAnchor.MiddleCenter, theme.textPrimary);
            return modal;
        }

        private static GameObject CreateNavigationRailPrefab(PetriDishUITheme theme)
        {
            GameObject rail = CreatePanel("NavigationRail", null, theme.panel);
            VerticalLayoutGroup group = rail.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(12, 12, 18, 18);
            group.spacing = 8f;
            group.childControlHeight = false;
            group.childForceExpandHeight = false;
            string[] names = { "Lab", "New", "Dishes", "Compare", "Journal", "Collection", "Challenges", "Settings" };
            for (int i = 0; i < names.Length; i++)
                CreateNavigationButton(names[i] + "Button", rail.transform, names[i], names[i].Substring(0, 1), theme, i == 0);
            return rail;
        }

        private static void BuildScene(PetriDishUITheme theme)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "LaboratoryHub";

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            GameObject root = CreateObject("LaboratoryHub", null, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            Stretch(root.GetComponent<RectTransform>());

            Image background = CreateImage("LaboratoryBackground", root.transform, theme.background);
            Stretch(background.rectTransform);
            Image bench = CreateImage("BenchSurface", background.transform, theme.bench);
            Anchor(bench.rectTransform, Vector2.zero, new Vector2(1f, 0.32f), Vector2.zero, Vector2.zero);
            bench.raycastTarget = false;

            RectTransform safeArea = CreateRect("SafeArea", root.transform);
            Stretch(safeArea);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            RectTransform header = CreatePanel("Header", safeArea, Color.clear).GetComponent<RectTransform>();
            Anchor(header, new Vector2(0f, 0.91f), Vector2.one, new Vector2(24f, 0f), new Vector2(-24f, -4f));
            CreateTextAnchored("Eyebrow", header, "PETRI DISH LABORATORY", 15, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0f, 0.54f), new Vector2(0.45f, 1f), theme.cyan);
            CreateTextAnchored("Title", header, "Laboratory Hub", 34, FontStyle.Bold, TextAnchor.MiddleLeft,
                Vector2.zero, new Vector2(0.55f, 0.62f), theme.textPrimary);
            CreateTextAnchored("Status", header, "3 active dishes  ?  1 needs attention", 18, FontStyle.Normal, TextAnchor.MiddleRight,
                new Vector2(0.58f, 0f), Vector2.one, theme.textSecondary);

            RectTransform body = CreateRect("Workspace", safeArea);
            Anchor(body, new Vector2(0f, 0.105f), new Vector2(1f, 0.905f), new Vector2(24f, 0f), new Vector2(-24f, 0f));
            HorizontalLayoutGroup columns = body.gameObject.AddComponent<HorizontalLayoutGroup>();
            columns.spacing = theme.standardSpacing;
            columns.childControlWidth = true;
            columns.childControlHeight = true;
            columns.childForceExpandWidth = false;
            columns.childForceExpandHeight = true;

            GameObject nav = InstantiatePrefab(NavigationPath + "NavigationRail.prefab", body);
            LayoutElement navLayout = nav.AddComponent<LayoutElement>();
            navLayout.preferredWidth = theme.navigationWidth;
            navLayout.flexibleWidth = 0f;
            List<GameObject> navLabels = new List<GameObject>();
            foreach (Text text in nav.GetComponentsInChildren<Text>(true))
                if (text.name == "Label") navLabels.Add(text.gameObject);

            GameObject active = CreatePanel("ActiveDishesPanel", body, theme.panel);
            LayoutElement activeLayout = active.AddComponent<LayoutElement>();
            activeLayout.preferredWidth = theme.activeDishesWidth;
            activeLayout.flexibleWidth = 0f;
            BuildActiveDishes(active.transform, theme, out AdaptiveDishCardLayoutGroup dishLayout);

            GameObject featured = CreateFeaturedDish("FeaturedDish", body, theme, null);
            LayoutElement featuredLayout = featured.AddComponent<LayoutElement>();
            featuredLayout.minWidth = 480f;
            featuredLayout.flexibleWidth = 1f;

            GameObject activity = CreatePanel("LabActivityPanel", body, theme.panel);
            LayoutElement activityLayout = activity.AddComponent<LayoutElement>();
            activityLayout.preferredWidth = theme.activityWidth;
            activityLayout.flexibleWidth = 0f;
            BuildActivity(activity.transform, theme);

            RectTransform footer = CreateRect("QuickActions", safeArea);
            Anchor(footer, new Vector2(0f, 0.01f), new Vector2(1f, 0.095f), new Vector2(24f, 0f), new Vector2(-24f, 0f));
            HorizontalLayoutGroup footerLayout = footer.gameObject.AddComponent<HorizontalLayoutGroup>();
            footerLayout.spacing = 12f;
            footerLayout.childControlWidth = true;
            footerLayout.childControlHeight = true;
            footerLayout.childForceExpandWidth = true;
            Button newExperiment = CreateButton("NewExperimentButton", footer, "+  NEW EXPERIMENT", theme, true);
            Button activeButton = CreateButton("ActiveDishesButton", footer, "ACTIVE DISHES", theme, false);
            Button compare = CreateButton("CompareButton", footer, "COMPARE", theme, false);
            Button journal = CreateButton("JournalButton", footer, "JOURNAL", theme, false);

            Button drawerButton = CreateButton("ActivityDrawerButton", safeArea, "ACTIVITY", theme, false);
            Anchor(drawerButton.GetComponent<RectTransform>(), new Vector2(0.82f, 0.82f), new Vector2(0.985f, 0.89f), Vector2.zero, Vector2.zero);
            drawerButton.gameObject.SetActive(false);

            GameObject drawer = CreatePanel("ActivityDrawer", safeArea, new Color(theme.panel.r, theme.panel.g, theme.panel.b, 1f));
            Anchor(drawer.GetComponent<RectTransform>(), new Vector2(0.55f, 0.12f), new Vector2(0.985f, 0.81f), Vector2.zero, Vector2.zero);
            BuildActivity(drawer.transform, theme);
            drawer.SetActive(false);

            GameObject feedback = CreatePanel("PlaceholderFeedback", safeArea, theme.panelRaised);
            Anchor(feedback.GetComponent<RectTransform>(), new Vector2(0.32f, 0.86f), new Vector2(0.68f, 0.915f), Vector2.zero, Vector2.zero);
            Text feedbackText = CreateTextAnchored("Message", feedback.transform, "", 16, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0.03f, 0f), new Vector2(0.97f, 1f), theme.textPrimary);
            feedback.SetActive(false);

            LaboratoryHubResponsiveLayout responsive = body.gameObject.AddComponent<LaboratoryHubResponsiveLayout>();
            responsive.Configure(theme, safeArea, navLayout, navLabels.ToArray(), columns, activeLayout, dishLayout, activityLayout, drawerButton.gameObject, drawer);
            drawerButton.onClick.AddListener(responsive.ToggleActivityDrawer);

            LaboratoryHubPresenter presenter = root.AddComponent<LaboratoryHubPresenter>();
            List<Button> actions = new List<Button> { newExperiment, activeButton, compare, journal };
            actions.AddRange(nav.GetComponentsInChildren<Button>(true));
            actions.Add(featured.transform.Find("OpenDishButton").GetComponent<Button>());
            SerializedObject serializedPresenter = new SerializedObject(presenter);
            SerializedProperty buttons = serializedPresenter.FindProperty("placeholderButtons");
            buttons.arraySize = actions.Count;
            for (int i = 0; i < actions.Count; i++) buttons.GetArrayElementAtIndex(i).objectReferenceValue = actions[i];
            serializedPresenter.FindProperty("feedbackText").objectReferenceValue = feedbackText;
            serializedPresenter.FindProperty("feedbackPanel").objectReferenceValue = feedback;
            serializedPresenter.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureSceneInBuildSettings();
            Selection.activeGameObject = root;
        }

        private static void BuildActiveDishes(Transform parent, PetriDishUITheme theme, out AdaptiveDishCardLayoutGroup cardLayout)
        {
            VerticalLayoutGroup group = parent.gameObject.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(16, 16, 18, 18);
            group.spacing = 12f;
            group.childControlHeight = false;
            group.childControlWidth = true;
            group.childForceExpandWidth = true;
            CreateTextWithLayout("Heading", parent, "ACTIVE DISHES", 18, FontStyle.Bold, theme.textPrimary, 34f);
            CreateTextWithLayout("Subheading", parent, "Live experiment overview", 14, FontStyle.Normal, theme.textSecondary, 24f);
            RectTransform cards = CreateRect("DishCards", parent);
            LayoutElement cardsLayout = cards.gameObject.AddComponent<LayoutElement>();
            cardsLayout.flexibleHeight = 1f;
            cardLayout = cards.gameObject.AddComponent<AdaptiveDishCardLayoutGroup>();
            cardLayout.spacing = 12f;
            cardLayout.childControlHeight = true;
            cardLayout.childControlWidth = true;
            cardLayout.childForceExpandHeight = true;
            cardLayout.childForceExpandWidth = true;
            CreateDishCard("DishA", cards, "Dish A", "E. coli", "Nutrient Agar", "18h", "Growing", "42%", "", theme.green, theme);
            CreateDishCard("DishB", cards, "Dish B", "Yeast", "Low Nutrient Agar", "9h", "Stressed", "26%", "LOW NUTRIENTS", theme.amber, theme);
            CreateDishCard("DishC", cards, "Dish C", "Fungus", "Moist Soil Gel", "31h", "Paused", "63%", "PAUSED", theme.textSecondary, theme);
        }

        private static GameObject CreateFeaturedDish(string name, Transform parent, PetriDishUITheme theme, PetriDishUITheme ignored)
        {
            if (parent != null && AssetDatabase.LoadAssetAtPath<GameObject>(ExperimentsPath + "FeaturedDishCard.prefab") != null)
            {
                GameObject instance = InstantiatePrefab(ExperimentsPath + "FeaturedDishCard.prefab", parent);
                instance.name = name;
                return instance;
            }

            GameObject panel = CreatePanel(name, parent, new Color(0.028f, 0.044f, 0.047f, 1f));
            CreateTextAnchored("LiveLabel", panel.transform, "?  LIVE CULTURE", 16, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.05f, 0.91f), new Vector2(0.48f, 0.98f), theme.cyan);
            CreateTextAnchored("DishName", panel.transform, "Dish A", 34, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.05f, 0.82f), new Vector2(0.52f, 0.91f), theme.textPrimary);
            CreateTextAnchored("Metadata", panel.transform, "E. coli  ?  Nutrient Agar  ?  18h", 17, FontStyle.Normal, TextAnchor.MiddleLeft,
                new Vector2(0.05f, 0.76f), new Vector2(0.80f, 0.83f), theme.textSecondary);
            CreateStatusBadge("StatusBadge", panel.transform, "GROWING", theme.green, theme,
                new Vector2(0.72f, 0.86f), new Vector2(0.94f, 0.94f));

            GameObject preview = CreateObject("DishPreview", panel.transform, typeof(RectTransform), typeof(LaboratoryDishPreviewGraphic));
            Anchor(preview.GetComponent<RectTransform>(), new Vector2(0.12f, 0.25f), new Vector2(0.88f, 0.76f), Vector2.zero, Vector2.zero);
            preview.GetComponent<LaboratoryDishPreviewGraphic>().raycastTarget = false;
            AspectRatioFitter fitter = preview.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 1f;

            CreateTextAnchored("CoverageLabel", panel.transform, "SURFACE COVERAGE", 13, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.05f, 0.17f), new Vector2(0.46f, 0.22f), theme.textSecondary);
            CreateTextAnchored("CoverageValue", panel.transform, "42%", 25, FontStyle.Bold, TextAnchor.MiddleRight,
                new Vector2(0.74f, 0.16f), new Vector2(0.94f, 0.23f), theme.textPrimary);
            Image track = CreateImage("CoverageTrack", panel.transform, theme.panelRaised);
            Anchor(track.rectTransform, new Vector2(0.05f, 0.13f), new Vector2(0.94f, 0.15f), Vector2.zero, Vector2.zero);
            Image fill = CreateImage("CoverageFill", track.transform, theme.green);
            Anchor(fill.rectTransform, Vector2.zero, new Vector2(0.42f, 1f), Vector2.zero, Vector2.zero);
            Button open = CreateButton("OpenDishButton", panel.transform, "OPEN DISH", theme, true);
            Anchor(open.GetComponent<RectTransform>(), new Vector2(0.05f, 0.035f), new Vector2(0.94f, 0.105f), Vector2.zero, Vector2.zero);
            return panel;
        }

        private static GameObject CreateDishCard(string name, Transform parent, string dish, string organism, string medium,
            string age, string status, string progress, string warning, Color statusColor, PetriDishUITheme theme)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ExperimentsPath + "DishCard.prefab");
            if (parent != null && prefab != null)
            {
                GameObject instance = InstantiatePrefab(ExperimentsPath + "DishCard.prefab", parent);
                instance.name = name;
                ConfigureDishCard(instance, dish, organism, medium, age, status, progress, warning, statusColor, theme);
                return instance;
            }

            GameObject card = CreatePanel(name, parent, theme.panelRaised);
            LayoutElement layout = card.AddComponent<LayoutElement>();
            layout.preferredHeight = 154f;
            layout.minWidth = 140f;
            layout.flexibleWidth = 1f;
            CreateTextAnchored("DishName", card.transform, dish, 22, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0.70f), new Vector2(0.58f, 0.94f), theme.textPrimary);
            CreateTextAnchored("Age", card.transform, age, 14, FontStyle.Normal, TextAnchor.MiddleRight,
                new Vector2(0.70f, 0.72f), new Vector2(0.94f, 0.93f), theme.textSecondary);
            CreateTextAnchored("Organism", card.transform, organism, 16, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0.51f), new Vector2(0.65f, 0.72f), theme.textPrimary);
            CreateTextAnchored("Medium", card.transform, medium, 13, FontStyle.Normal, TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0.34f), new Vector2(0.78f, 0.53f), theme.textSecondary);
            CreateStatusBadge("Status", card.transform, status.ToUpperInvariant(), statusColor, theme,
                new Vector2(0.67f, 0.48f), new Vector2(0.94f, 0.69f));
            Image track = CreateImage("ProgressTrack", card.transform, theme.panel);
            Anchor(track.rectTransform, new Vector2(0.06f, 0.18f), new Vector2(0.94f, 0.24f), Vector2.zero, Vector2.zero);
            Image fill = CreateImage("ProgressFill", track.transform, statusColor);
            Anchor(fill.rectTransform, Vector2.zero, new Vector2(0.42f, 1f), Vector2.zero, Vector2.zero);
            CreateTextAnchored("Progress", card.transform, progress, 13, FontStyle.Bold, TextAnchor.MiddleRight,
                new Vector2(0.74f, 0.01f), new Vector2(0.94f, 0.17f), theme.textPrimary);
            Text warningText = CreateTextAnchored("Warning", card.transform, warning, 11, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0.01f), new Vector2(0.70f, 0.17f), statusColor);
            warningText.gameObject.SetActive(!string.IsNullOrEmpty(warning));
            return card;
        }

        private static void ConfigureDishCard(GameObject card, string dish, string organism, string medium,
            string age, string status, string progress, string warning, Color statusColor, PetriDishUITheme theme)
        {
            FindChild(card.transform, "DishName").GetComponent<Text>().text = dish;
            FindChild(card.transform, "Age").GetComponent<Text>().text = age;
            FindChild(card.transform, "Organism").GetComponent<Text>().text = organism;
            FindChild(card.transform, "Medium").GetComponent<Text>().text = medium;
            FindChild(card.transform, "Progress").GetComponent<Text>().text = progress;
            Transform statusOwner = FindChild(card.transform, "Status");
            statusOwner.GetComponent<Image>().color = new Color(statusColor.r, statusColor.g, statusColor.b, 0.14f);
            statusOwner.GetComponentInChildren<Text>(true).text = status.ToUpperInvariant();
            statusOwner.GetComponentInChildren<Text>(true).color = statusColor;
            Transform progressFill = FindChild(card.transform, "ProgressFill");
            progressFill.GetComponent<Image>().color = statusColor;
            RectTransform progressRect = progressFill.GetComponent<RectTransform>();
            progressRect.anchorMax = new Vector2(Mathf.Clamp01(float.Parse(progress.TrimEnd('%')) / 100f), 1f);
            Transform warningOwner = FindChild(card.transform, "Warning");
            warningOwner.GetComponent<Text>().text = warning;
            warningOwner.GetComponent<Text>().color = statusColor;
            warningOwner.gameObject.SetActive(!string.IsNullOrEmpty(warning));
        }

        private static void BuildActivity(Transform parent, PetriDishUITheme theme)
        {
            VerticalLayoutGroup group = parent.gameObject.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(16, 16, 18, 18);
            group.spacing = 12f;
            group.childControlHeight = false;
            group.childControlWidth = true;
            group.childForceExpandWidth = true;
            CreateTextWithLayout("Heading", parent, "LAB ACTIVITY", 18, FontStyle.Bold, theme.textPrimary, 34f);
            CreateTextWithLayout("Subheading", parent, "Discoveries and progress", 14, FontStyle.Normal, theme.textSecondary, 24f);
            CreateActivityCard("LatestDiscovery", parent, "LATEST DISCOVERY", "Colony edge pattern", "Observed in Dish A ? 12 min ago", theme.cyan, theme);
            CreateActivityCard("CurrentChallenge", parent, "CURRENT CHALLENGE", "Maintain stable growth", "18 of 24 hours complete", theme.amber, theme);
            CreateActivityCard("RecentUnlock", parent, "RECENT UNLOCK", "Moist Soil Gel", "New medium available in experiments", theme.green, theme);
        }

        private static GameObject CreateActivityCard(string name, Transform parent, string category, string title, string detail, Color accent, PetriDishUITheme theme)
        {
            string prefabPath = category == "LATEST DISCOVERY" ? LaboratoryPath + "DiscoveryCard.prefab" :
                category == "CURRENT CHALLENGE" ? LaboratoryPath + "ChallengeCard.prefab" :
                LaboratoryPath + "LaboratoryActivityCard.prefab";
            if (parent != null && AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                GameObject instance = InstantiatePrefab(prefabPath, parent);
                instance.name = name;
                FindChild(instance.transform, "Category").GetComponent<Text>().text = category;
                FindChild(instance.transform, "Category").GetComponent<Text>().color = accent;
                FindChild(instance.transform, "Title").GetComponent<Text>().text = title;
                FindChild(instance.transform, "Detail").GetComponent<Text>().text = detail;
                return instance;
            }

            GameObject card = CreatePanel(name, parent, theme.panelRaised);
            LayoutElement layout = card.AddComponent<LayoutElement>();
            layout.preferredHeight = 134f;
            CreateTextAnchored("Category", card.transform, category, 12, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0.68f), new Vector2(0.94f, 0.92f), accent);
            CreateTextAnchored("Title", card.transform, title, 18, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0.38f), new Vector2(0.94f, 0.70f), theme.textPrimary);
            CreateTextAnchored("Detail", card.transform, detail, 13, FontStyle.Normal, TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.40f), theme.textSecondary);
            return card;
        }

        private static Button CreateNavigationButton(string name, Transform parent, string label, string icon, PetriDishUITheme theme, bool selected)
        {
            Button button = CreateButton(name, parent, "", theme, selected);
            LayoutElement layout = button.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 54f;
            CreateTextAnchored("Icon", button.transform, icon, 19, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0.04f, 0f), new Vector2(0.28f, 1f), selected ? theme.cyan : theme.textSecondary);
            CreateTextAnchored("Label", button.transform, label, 15, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.31f, 0f), new Vector2(0.96f, 1f), selected ? theme.cyan : theme.textPrimary);
            return button;
        }

        private static GameObject CreateStatusBadge(string name, Transform parent, string label, Color color, PetriDishUITheme theme,
            Vector2? min = null, Vector2? max = null)
        {
            Image badge = CreateImage(name, parent, new Color(color.r, color.g, color.b, 0.14f));
            if (min.HasValue) Anchor(badge.rectTransform, min.Value, max.Value, Vector2.zero, Vector2.zero);
            CreateTextAnchored("Label", badge.transform, label, 12, FontStyle.Bold, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, color);
            return badge.gameObject;
        }

        private static Button CreateButton(string name, Transform parent, string label, PetriDishUITheme theme, bool primary)
        {
            GameObject owner = CreateObject(name, parent, typeof(RectTransform), typeof(Image), typeof(Button));
            Image image = owner.GetComponent<Image>();
            image.color = primary ? new Color(theme.cyan.r, theme.cyan.g, theme.cyan.b, 0.18f) : theme.panelRaised;
            Button button = owner.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = primary ? new Color(theme.cyan.r, theme.cyan.g, theme.cyan.b, 0.32f) : theme.panelHover;
            colors.pressedColor = primary ? new Color(theme.cyan.r, theme.cyan.g, theme.cyan.b, 0.45f) : theme.panel;
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            if (!string.IsNullOrEmpty(label))
                CreateTextAnchored("Label", owner.transform, label, 15, FontStyle.Bold, TextAnchor.MiddleCenter,
                    new Vector2(0.04f, 0f), new Vector2(0.96f, 1f), primary ? theme.cyan : theme.textPrimary);
            return button;
        }

        private static Text CreateTextWithLayout(string name, Transform parent, string value, int size, FontStyle style, Color color, float height)
        {
            Text text = CreateText(name, parent, value, size, style, TextAnchor.MiddleLeft, color);
            LayoutElement layout = text.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
            return text;
        }

        private static Text CreateTextAnchored(string name, Transform parent, string value, int size, FontStyle style,
            TextAnchor alignment, Vector2 min, Vector2 max, Color color)
        {
            Text text = CreateText(name, parent, value, size, style, alignment, color);
            Anchor(text.rectTransform, min, max, Vector2.zero, Vector2.zero);
            return text;
        }

        private static Text CreateText(string name, Transform parent, string value, int size, FontStyle style, TextAnchor alignment, Color color)
        {
            Text text = CreateObject(name, parent, typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 10;
            text.resizeTextMaxSize = size;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color) => CreateImage(name, parent, color).gameObject;
        private static Image CreateImage(string name, Transform parent, Color color)
        {
            Image image = CreateObject(name, parent, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static RectTransform CreateRect(string name, Transform parent) =>
            CreateObject(name, parent, typeof(RectTransform)).GetComponent<RectTransform>();

        private static GameObject CreateObject(string name, Transform parent, params Type[] components)
        {
            GameObject owner = new GameObject(name, components);
            if (parent != null) owner.transform.SetParent(parent, false);
            return owner;
        }

        private static Transform FindChild(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform result = FindChild(root.GetChild(i), name);
                if (result != null) return result;
            }
            return null;
        }

        private static GameObject InstantiatePrefab(string path, Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetParent(parent, false);
            return instance;
        }

        private static void Stretch(RectTransform rect) => Anchor(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        private static void Anchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
        }

        private static void EnsureSceneInBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (EditorBuildSettingsScene item in scenes)
                if (item.path == ScenePath) return;
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
