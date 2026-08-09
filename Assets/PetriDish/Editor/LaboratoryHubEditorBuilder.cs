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
            EnsurePrefab(CommonPath + "Panel.prefab", () => CreatePanel("Panel", null, theme.panel, theme));
            EnsurePrefab(CommonPath + "PrimaryButton.prefab", () => CreateButton("PrimaryButton", null, "Primary action", theme, true).gameObject);
            EnsurePrefab(CommonPath + "SecondaryButton.prefab", () => CreateButton("SecondaryButton", null, "Secondary action", theme, false).gameObject);
            EnsurePrefab(CommonPath + "Modal.prefab", () => CreateModalPrefab(theme));
            EnsurePrefab(NavigationPath + "NavigationButton.prefab", () => CreateNavigationButton("NavigationButton", null, "Lab", "L", theme, false).gameObject);
            EnsurePrefab(NavigationPath + "NavigationRail.prefab", () => CreateNavigationRailPrefab(theme));
            EnsurePrefab(ExperimentsPath + "ExperimentStatusBadge.prefab", () => CreateStatusBadge("ExperimentStatusBadge", null, "Growing well", theme.green, theme).gameObject);
            EnsurePrefab(ExperimentsPath + "DishCard.prefab", () => CreateDishCard("DishCard", null, "Dish A", "Bacillus subtilis", "Nutrient Agar", "18 h", "Growing well", "42%", "", theme.green, theme));
            EnsurePrefab(ExperimentsPath + "FeaturedDishCard.prefab", () => CreateFeaturedDish("FeaturedDishCard", null, theme, null));
            EnsurePrefab(LaboratoryPath + "LaboratoryActivityCard.prefab", () => CreateActivityCard("LaboratoryActivityCard", null, "CURRENT OBSERVATION", "Colony edge expanding steadily.", "Observation recorded just now.", theme.cyan, theme));
            EnsurePrefab(LaboratoryPath + "DiscoveryCard.prefab", () => CreateActivityCard("DiscoveryCard", null, "LATEST DISCOVERY", "Optimal temperature range observed.", "Growth remains within the simplified model.", theme.green, theme));
            EnsurePrefab(LaboratoryPath + "ChallengeCard.prefab", () => CreateActivityCard("ChallengeCard", null, "CURRENT CHALLENGE", "Maintain stable growth for another 6 hours.", "Mock challenge progress", theme.amber, theme));
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
            GameObject modal = CreatePanel("Modal", null, new Color(0.82f, 0.86f, 0.87f, 0.72f), theme);
            RectTransform card = CreatePanel("Dialog", modal.transform, theme.panel, theme).GetComponent<RectTransform>();
            Anchor(card, new Vector2(0.24f, 0.28f), new Vector2(0.76f, 0.72f), Vector2.zero, Vector2.zero);
            CreateText("Title", card, "Laboratory message", 30, FontStyle.Bold, TextAnchor.MiddleCenter, theme.textPrimary);
            return modal;
        }

        private static GameObject CreateNavigationRailPrefab(PetriDishUITheme theme)
        {
            GameObject rail = CreatePanel("NavigationRail", null, theme.panel, theme);
            VerticalLayoutGroup group = rail.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(14, 14, 22, 18);
            group.spacing = 10f;
            group.childControlHeight = false;
            group.childForceExpandHeight = false;

            CreateTextWithLayout("RailTitle", rail.transform, "PETRI", 15, FontStyle.Bold, theme.cyan, 34f);
            string[] names = { "Lab", "New Experiment", "Compare", "Journal", "Collection", "Challenges" };
            string[] icons = { "L", "+", "C", "J", "O", "!" };
            for (int i = 0; i < names.Length; i++)
                CreateNavigationButton("Nav" + names[i].Replace(" ", string.Empty) + "Button", rail.transform, names[i], icons[i], theme, i == 0);

            RectTransform spacer = CreateRect("NavigationSpacer", rail.transform);
            LayoutElement spacerLayout = spacer.gameObject.AddComponent<LayoutElement>();
            spacerLayout.flexibleHeight = 1f;
            Image divider = CreateImage("SettingsDivider", rail.transform, theme.border);
            LayoutElement dividerLayout = divider.gameObject.AddComponent<LayoutElement>();
            dividerLayout.preferredHeight = 1f;
            dividerLayout.minHeight = 1f;
            CreateNavigationButton("NavSettingsButton", rail.transform, "Settings", "S", theme, false);
            return rail;
        }

        private static void BuildScene(PetriDishUITheme theme)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "LaboratoryHub";

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
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
            Image bench = CreateImage("CoolGreyWorkSurface", background.transform, theme.bench);
            Anchor(bench.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.24f), Vector2.zero, Vector2.zero);
            bench.raycastTarget = false;

            RectTransform safeArea = CreateRect("SafeArea", root.transform);
            Stretch(safeArea);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            RectTransform header = CreateRect("Header", safeArea);
            Anchor(header, new Vector2(0f, 0.91f), Vector2.one, new Vector2(28f, 0f), new Vector2(-28f, -4f));
            CreateTextAnchored("Title", header, "PETRI LAB", 35, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0f, 0f), new Vector2(0.42f, 1f), theme.textPrimary);
            CreateTextAnchored("Subtitle", header, "Selected culture workspace", 17, FontStyle.Normal, TextAnchor.MiddleLeft,
                new Vector2(0.19f, 0.02f), new Vector2(0.51f, 0.52f), theme.textSecondary);
            Button journalHeader = CreateButton("HeaderJournalButton", header, "Journal", theme, false);
            Anchor(journalHeader.GetComponent<RectTransform>(), new Vector2(0.75f, 0.18f), new Vector2(0.86f, 0.82f), Vector2.zero, Vector2.zero);
            Button settingsHeader = CreateButton("HeaderSettingsButton", header, "Settings", theme, false);
            Anchor(settingsHeader.GetComponent<RectTransform>(), new Vector2(0.875f, 0.18f), new Vector2(1f, 0.82f), Vector2.zero, Vector2.zero);

            RectTransform body = CreateRect("Workspace", safeArea);
            Anchor(body, new Vector2(0f, 0.105f), new Vector2(1f, 0.905f), new Vector2(28f, 0f), new Vector2(-28f, 0f));
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

            GameObject featured = CreateFeaturedDish("SelectedDish", body, theme, null);
            LayoutElement featuredLayout = featured.AddComponent<LayoutElement>();
            featuredLayout.minWidth = 520f;
            featuredLayout.flexibleWidth = 1f;

            GameObject notes = CreatePanel("LabNotesPanel", body, theme.panel, theme);
            LayoutElement notesLayout = notes.AddComponent<LayoutElement>();
            notesLayout.preferredWidth = theme.notesWidth;
            notesLayout.flexibleWidth = 0f;
            BuildNotes(notes.transform, theme);

            RectTransform footer = CreateRect("PrimaryActions", safeArea);
            Anchor(footer, new Vector2(0f, 0.012f), new Vector2(1f, 0.095f), new Vector2(28f, 0f), new Vector2(-28f, 0f));
            HorizontalLayoutGroup footerLayout = footer.gameObject.AddComponent<HorizontalLayoutGroup>();
            footerLayout.spacing = 14f;
            footerLayout.childControlWidth = true;
            footerLayout.childControlHeight = true;
            footerLayout.childForceExpandWidth = false;
            footerLayout.childForceExpandHeight = true;
            RectTransform footerSpacer = CreateRect("ActionSpacer", footer);
            LayoutElement footerSpacerLayout = footerSpacer.gameObject.AddComponent<LayoutElement>();
            footerSpacerLayout.flexibleWidth = 1f;
            Button newExperiment = CreateButton("NewExperimentButton", footer, "+  START NEW EXPERIMENT", theme, true);
            LayoutElement newLayout = newExperiment.gameObject.AddComponent<LayoutElement>();
            newLayout.preferredWidth = 390f;
            Button compare = CreateButton("CompareButton", footer, "COMPARE", theme, false);
            LayoutElement compareLayout = compare.gameObject.AddComponent<LayoutElement>();
            compareLayout.preferredWidth = 210f;
            RectTransform footerSpacerRight = CreateRect("ActionSpacerRight", footer);
            LayoutElement footerSpacerRightLayout = footerSpacerRight.gameObject.AddComponent<LayoutElement>();
            footerSpacerRightLayout.flexibleWidth = 1f;

            Button drawerButton = CreateButton("NotesDrawerButton", safeArea, "LAB NOTES", theme, false);
            Anchor(drawerButton.GetComponent<RectTransform>(), new Vector2(0.80f, 0.82f), new Vector2(0.985f, 0.89f), Vector2.zero, Vector2.zero);
            drawerButton.gameObject.SetActive(false);

            GameObject drawer = CreatePanel("NotesDrawer", safeArea, theme.panel, theme);
            Anchor(drawer.GetComponent<RectTransform>(), new Vector2(0.53f, 0.12f), new Vector2(0.985f, 0.81f), Vector2.zero, Vector2.zero);
            BuildNotes(drawer.transform, theme);
            drawer.SetActive(false);

            GameObject feedback = CreatePanel("PlaceholderFeedback", safeArea, theme.panel, theme);
            Anchor(feedback.GetComponent<RectTransform>(), new Vector2(0.31f, 0.855f), new Vector2(0.69f, 0.91f), Vector2.zero, Vector2.zero);
            Text feedbackText = CreateTextAnchored("Message", feedback.transform, "", 17, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0.03f, 0f), new Vector2(0.97f, 1f), theme.textPrimary);
            feedback.SetActive(false);

            LaboratoryHubResponsiveLayout responsive = body.gameObject.AddComponent<LaboratoryHubResponsiveLayout>();
            responsive.Configure(theme, safeArea, navLayout, navLabels.ToArray(), columns, notesLayout,
                drawerButton.gameObject, drawer);
            drawerButton.onClick.AddListener(responsive.ToggleNotesDrawer);

            LaboratoryHubPresenter presenter = root.AddComponent<LaboratoryHubPresenter>();
            List<Button> actions = new List<Button> { newExperiment, compare, journalHeader, settingsHeader };
            actions.AddRange(nav.GetComponentsInChildren<Button>(true));
            actions.Add(FindChild(featured.transform, "OpenDishButton").GetComponent<Button>());
            SerializedObject serializedPresenter = new SerializedObject(presenter);
            SerializedProperty buttons = serializedPresenter.FindProperty("placeholderButtons");
            buttons.arraySize = actions.Count;
            for (int i = 0; i < actions.Count; i++)
                buttons.GetArrayElementAtIndex(i).objectReferenceValue = actions[i];
            serializedPresenter.FindProperty("feedbackText").objectReferenceValue = feedbackText;
            serializedPresenter.FindProperty("feedbackPanel").objectReferenceValue = feedback;
            serializedPresenter.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureSceneInBuildSettings();
            Selection.activeGameObject = root;
        }

        private static GameObject CreateFeaturedDish(string name, Transform parent, PetriDishUITheme theme, PetriDishUITheme ignored)
        {
            if (parent != null && AssetDatabase.LoadAssetAtPath<GameObject>(ExperimentsPath + "FeaturedDishCard.prefab") != null)
            {
                GameObject instance = InstantiatePrefab(ExperimentsPath + "FeaturedDishCard.prefab", parent);
                instance.name = name;
                return instance;
            }

            GameObject panel = CreatePanel(name, parent, theme.panel, theme);
            Image liveStrip = CreateImage("LiveStatusStrip", panel.transform,
                new Color(theme.cyan.r, theme.cyan.g, theme.cyan.b, 0.09f));
            Anchor(liveStrip.rectTransform, new Vector2(0.035f, 0.90f), new Vector2(0.965f, 0.975f), Vector2.zero, Vector2.zero);
            CreateTextAnchored("LiveLabel", liveStrip.transform, "●  Live culture", 17, FontStyle.Bold,
                TextAnchor.MiddleLeft, new Vector2(0.03f, 0f), new Vector2(0.48f, 1f), theme.cyan);
            CreateStatusBadge("StatusBadge", liveStrip.transform, "Growing well", theme.green, theme,
                new Vector2(0.72f, 0.15f), new Vector2(0.97f, 0.85f));

            CreateTextAnchored("DishName", panel.transform, "Dish A", 38, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.055f, 0.825f), new Vector2(0.48f, 0.895f), theme.textPrimary);
            CreateTextAnchored("Organism", panel.transform, "Bacillus subtilis", 23, FontStyle.Italic,
                TextAnchor.MiddleLeft, new Vector2(0.055f, 0.770f), new Vector2(0.53f, 0.825f), theme.textPrimary);
            CreateTextAnchored("Medium", panel.transform, "Nutrient Agar", 17, FontStyle.Normal,
                TextAnchor.MiddleLeft, new Vector2(0.055f, 0.725f), new Vector2(0.50f, 0.770f), theme.textSecondary);

            GameObject previewWell = CreatePanel("DishPreviewWell", panel.transform, theme.panelRaised, theme);
            Anchor(previewWell.GetComponent<RectTransform>(), new Vector2(0.16f, 0.345f), new Vector2(0.84f, 0.715f), Vector2.zero, Vector2.zero);
            GameObject preview = CreateObject("DishPreview", previewWell.transform,
                typeof(RectTransform), typeof(LaboratoryDishPreviewGraphic));
            Anchor(preview.GetComponent<RectTransform>(), new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.96f), Vector2.zero, Vector2.zero);
            preview.GetComponent<LaboratoryDishPreviewGraphic>().raycastTarget = false;
            AspectRatioFitter fitter = preview.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 1f;

            CreateMetricTile("AgeMetric", panel.transform, "Age", "18 h", theme,
                new Vector2(0.055f, 0.255f), new Vector2(0.475f, 0.33f));
            CreateMetricTile("CoverageMetric", panel.transform, "Coverage", "42%", theme,
                new Vector2(0.525f, 0.255f), new Vector2(0.945f, 0.33f));

            GameObject environment = CreatePanel("EnvironmentSummary", panel.transform, theme.panelRaised, theme);
            Anchor(environment.GetComponent<RectTransform>(), new Vector2(0.055f, 0.16f), new Vector2(0.945f, 0.245f), Vector2.zero, Vector2.zero);
            CreateEnvironmentCell("Temperature", environment.transform, "26°C", "Temperature", theme,
                new Vector2(0f, 0f), new Vector2(0.333f, 1f));
            CreateEnvironmentCell("Moisture", environment.transform, "42%", "Moisture", theme,
                new Vector2(0.333f, 0f), new Vector2(0.666f, 1f));
            CreateEnvironmentCell("Nutrients", environment.transform, "OK", "Nutrients", theme,
                new Vector2(0.666f, 0f), Vector2.one);

            Button open = CreateButton("OpenDishButton", panel.transform, "OPEN DISH", theme, true);
            Anchor(open.GetComponent<RectTransform>(), new Vector2(0.19f, 0.075f), new Vector2(0.81f, 0.145f), Vector2.zero, Vector2.zero);

            GameObject navigation = CreatePanel("DishNavigation", panel.transform, theme.panelRaised, theme);
            Anchor(navigation.GetComponent<RectTransform>(), new Vector2(0.19f, 0.012f), new Vector2(0.81f, 0.063f), Vector2.zero, Vector2.zero);
            Button previous = CreateButton("PreviousDishButton", navigation.transform, "‹", theme, false);
            Anchor(previous.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0.16f, 1f), Vector2.zero, Vector2.zero);
            previous.interactable = false;
            CreateTextAnchored("DishNavigationState", navigation.transform, "Dish A     1 / 1", 16,
                FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0.17f, 0f), new Vector2(0.83f, 1f), theme.textPrimary);
            Button next = CreateButton("NextDishButton", navigation.transform, "›", theme, false);
            Anchor(next.GetComponent<RectTransform>(), new Vector2(0.84f, 0f), Vector2.one, Vector2.zero, Vector2.zero);
            next.interactable = false;
            return panel;
        }

        private static void CreateMetricTile(string name, Transform parent, string label, string value,
            PetriDishUITheme theme, Vector2 min, Vector2 max)
        {
            GameObject tile = CreatePanel(name, parent, theme.panelRaised, theme);
            Anchor(tile.GetComponent<RectTransform>(), min, max, Vector2.zero, Vector2.zero);
            CreateTextAnchored("Label", tile.transform, label, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0f), new Vector2(0.55f, 1f), theme.textSecondary);
            CreateTextAnchored("Value", tile.transform, value, 24, FontStyle.Bold, TextAnchor.MiddleRight,
                new Vector2(0.48f, 0f), new Vector2(0.94f, 1f), theme.textPrimary);
        }

        private static void CreateEnvironmentCell(string name, Transform parent, string value, string label,
            PetriDishUITheme theme, Vector2 min, Vector2 max)
        {
            RectTransform cell = CreateRect(name, parent);
            Anchor(cell, min, max, new Vector2(8f, 4f), new Vector2(-8f, -4f));
            CreateTextAnchored("Value", cell, value, 19, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0f, 0.38f), Vector2.one, name == "Nutrients" ? theme.green : theme.textPrimary);
            CreateTextAnchored("Label", cell, label, 12, FontStyle.Normal, TextAnchor.MiddleCenter,
                Vector2.zero, new Vector2(1f, 0.42f), theme.textSecondary);
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

            GameObject card = CreatePanel(name, parent, theme.panelRaised, theme);
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

        private static void BuildNotes(Transform parent, PetriDishUITheme theme)
        {
            VerticalLayoutGroup group = parent.gameObject.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(18, 18, 20, 18);
            group.spacing = 12f;
            group.childControlHeight = true;
            group.childControlWidth = true;
            group.childForceExpandHeight = false;
            group.childForceExpandWidth = true;
            CreateTextWithLayout("Heading", parent, "Lab Notes", 25, FontStyle.Bold, theme.textPrimary, 42f);
            CreateTextWithLayout("Subheading", parent, "Observation notebook", 15, FontStyle.Normal, theme.textSecondary, 28f);
            CreateActivityCard("CurrentObservation", parent, "CURRENT OBSERVATION",
                "Colony edge expanding steadily.", "Recorded from mock culture data.", theme.cyan, theme);
            CreateActivityCard("LatestDiscovery", parent, "LATEST DISCOVERY",
                "Optimal temperature range observed.", "Simplified educational observation.", theme.green, theme);
            CreateActivityCard("CurrentChallenge", parent, "CURRENT CHALLENGE",
                "Maintain stable growth for another 6 hours.", "Mock challenge progress", theme.amber, theme);
            CreateActivityCard("RecentUpdate", parent, "RECENT UPDATE",
                "Nutrient Agar profile updated.", "Journal presentation placeholder", theme.cyan, theme);
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

            GameObject card = CreatePanel(name, parent, theme.panelRaised, theme);
            LayoutElement layout = card.AddComponent<LayoutElement>();
            layout.minHeight = 104f;
            layout.preferredHeight = 142f;
            layout.flexibleHeight = 1f;
            CreateTextAnchored("Category", card.transform, category, 12, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0.72f), new Vector2(0.94f, 0.94f), accent);
            CreateTextAnchored("Title", card.transform, title, 18, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0.35f), new Vector2(0.94f, 0.73f), theme.textPrimary);
            CreateTextAnchored("Detail", card.transform, detail, 13, FontStyle.Normal, TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0.07f), new Vector2(0.94f, 0.36f), theme.textSecondary);
            return card;
        }

        private static Button CreateNavigationButton(string name, Transform parent, string label, string icon,
            PetriDishUITheme theme, bool selected)
        {
            Button button = CreateButton(name, parent, "", theme, false);
            LayoutElement layout = button.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 58f;
            if (selected)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = new Color(theme.cyan.r, theme.cyan.g, theme.cyan.b, 0.10f);
                colors.selectedColor = new Color(theme.cyan.r, theme.cyan.g, theme.cyan.b, 0.16f);
                button.colors = colors;
                Image edge = CreateImage("SelectedEdge", button.transform, theme.cyan);
                Anchor(edge.rectTransform, new Vector2(0f, 0.12f), new Vector2(0.025f, 0.88f), Vector2.zero, Vector2.zero);
            }
            CreateTextAnchored("Icon", button.transform, icon, 18, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0.05f, 0f), new Vector2(0.29f, 1f), selected ? theme.cyan : theme.textSecondary);
            CreateTextAnchored("Label", button.transform, label, 17, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.32f, 0f), new Vector2(0.96f, 1f), selected ? theme.cyan : theme.textPrimary);
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
            image.color = Color.white;
            Button button = owner.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = primary ? theme.cyan : theme.panel;
            colors.highlightedColor = primary
                ? new Color(theme.cyan.r * 0.88f, theme.cyan.g * 0.88f, theme.cyan.b * 0.88f, 1f)
                : theme.panelHover;
            colors.pressedColor = primary
                ? new Color(theme.cyan.r * 0.72f, theme.cyan.g * 0.72f, theme.cyan.b * 0.72f, 1f)
                : theme.bench;
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = theme.panelRaised;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            Outline outline = owner.AddComponent<Outline>();
            outline.effectColor = primary ? new Color(theme.cyan.r, theme.cyan.g, theme.cyan.b, 0.42f) : theme.border;
            outline.effectDistance = new Vector2(1f, -1f);
            if (!string.IsNullOrEmpty(label))
                CreateTextAnchored("Label", owner.transform, label, primary ? 17 : 16, FontStyle.Bold,
                    TextAnchor.MiddleCenter, new Vector2(0.04f, 0f), new Vector2(0.96f, 1f),
                    primary ? theme.textOnAccent : theme.textPrimary);
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
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = size;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color) =>
            CreatePanel(name, parent, color, null);

        private static GameObject CreatePanel(string name, Transform parent, Color color, PetriDishUITheme theme)
        {
            Image image = CreateImage(name, parent, color);
            if (theme != null)
            {
                Outline outline = image.gameObject.AddComponent<Outline>();
                outline.effectColor = theme.border;
                outline.effectDistance = new Vector2(1f, -1f);
                Shadow shadow = image.gameObject.AddComponent<Shadow>();
                shadow.effectColor = theme.shadow;
                shadow.effectDistance = new Vector2(0f, -3f);
            }
            return image.gameObject;
        }

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
