using System;
using System.Collections.Generic;
using System.Reflection;
using PetriDish.Presentation;
using PetriDish.Presentation.UI;
using PetriDish.Simulation;
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
        public const string DisplayPrefabPath = "Assets/PetriDish/Presentation/Prefabs/PetriDishDisplay.prefab";
        public const string MockColonyTexturePath = "Assets/PetriDish/Presentation/Textures/LaboratoryHubMockColony.asset";
        private const string DishModelPath = "Assets/PetriDish/Art/models/PetriDish.fbx";
        private const float TypographyScale = 1.25f;
        private const float HubDishFramingScale = 1.35f;
        private const float HubDishVerticalOffset = 0.06f;

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
            EnsureFolder("Assets/PetriDish", "Presentation");
            EnsureFolder("Assets/PetriDish/Presentation", "Prefabs");
            EnsureFolder("Assets/PetriDish/Presentation", "Textures");
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
            EnsureMockColonyTexture();
            EnsureDishDisplayPrefab();
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

        private static void EnsureMockColonyTexture()
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(MockColonyTexturePath) != null) return;

            GameObject temporary = new GameObject("MockDishRenderer", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            DishRenderer renderer = temporary.AddComponent<DishRenderer>();
            typeof(DishRenderer).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(renderer, null);
            const int size = 48;
            float[] biomass = new float[size * size];
            float[] health = new float[size * size];
            float[] moisture = new float[size * size];
            float[] nutrients = new float[size * size];
            Vector2[] centres = { new Vector2(-0.28f, 0.18f), new Vector2(0.22f, 0.25f), new Vector2(0.13f, -0.22f), new Vector2(-0.18f, -0.28f) };
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int index = y * size + x;
                Vector2 point = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f);
                float growth = 0f;
                foreach (Vector2 centre in centres)
                    growth = Mathf.Max(growth, Mathf.Clamp01(1f - Vector2.Distance(point, centre) / 0.29f));
                biomass[index] = growth;
                health[index] = 0.92f;
                moisture[index] = 0.42f;
                nutrients[index] = 0.76f;
            }
            renderer.Render(new SimulationSnapshot(size, size, 18, 26f, 0.42f, 0.92f, 0.42f, 0.76f,
                biomass, health, moisture, nutrients));
            Texture2D saved = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "LaboratoryHubMockColony" };
            saved.SetPixels32(renderer.ColonyTexture.GetPixels32());
            saved.Apply(false, false);
            saved.filterMode = FilterMode.Bilinear;
            saved.wrapMode = TextureWrapMode.Clamp;
            AssetDatabase.CreateAsset(saved, MockColonyTexturePath);
            UnityEngine.Object.DestroyImmediate(temporary);
        }

        private static void EnsureDishDisplayPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(DisplayPrefabPath) != null) return;

            GameObject root = new GameObject("PetriDishDisplay", typeof(PetriDishDisplayPresenter));
            Transform pivot = new GameObject("RotationPivot").transform;
            pivot.SetParent(root.transform, false);
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(DishModelPath);
            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(source);
            model.name = "PetriDish3D";
            model.transform.SetParent(pivot, false);
            model.transform.localScale = Vector3.one * 5f;
            Transform lid = FindChild(model.transform, "PetriDish_Lid");
            if (lid != null) lid.localRotation = Quaternion.Euler(270f, 0f, 0f);

            Transform colony = FindChild(model.transform, "PetriDish_ColonySurface");
            MeshRenderer colonyRenderer = colony.GetComponent<MeshRenderer>();
            ColonySurfacePresenter colonyPresenter = colony.gameObject.AddComponent<ColonySurfacePresenter>();
            colonyPresenter.ConfigureStatic(colonyRenderer, "_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(MockColonyTexturePath));
            colonyPresenter.SetTextureAlignment(new Vector2(1.7f, 1.7f), new Vector2(0.08f, 0.08f));

            GameObject cameraObject = new GameObject("DishDisplayCamera", typeof(Camera));
            cameraObject.transform.SetParent(root.transform, false);
            Camera displayCamera = cameraObject.GetComponent<Camera>();
            displayCamera.enabled = false;
            displayCamera.clearFlags = CameraClearFlags.SolidColor;
            displayCamera.backgroundColor = Color.clear;
            displayCamera.fieldOfView = 32f;
            displayCamera.allowHDR = true;
            displayCamera.allowMSAA = true;

            Light key = CreatePresentationLight("NeutralKeyLight", root.transform, new Vector3(-35f, 25f, 0f), 1.05f);
            Light fill = CreatePresentationLight("SoftFillLight", root.transform, new Vector3(45f, -20f, 0f), 0.52f);
            PetriDishDisplayPresenter presenter = root.GetComponent<PetriDishDisplayPresenter>();
            presenter.ConfigureRig(pivot, displayCamera, new[] { key, fill });
            PrefabUtility.SaveAsPrefabAsset(root, DisplayPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static Light CreatePresentationLight(string name, Transform parent, Vector3 euler, float intensity)
        {
            GameObject owner = new GameObject(name, typeof(Light));
            owner.transform.SetParent(parent, false);
            owner.transform.localRotation = Quaternion.Euler(euler);
            Light light = owner.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.96f, 0.98f, 1f);
            light.intensity = intensity;
            light.shadows = LightShadows.Soft;
            light.enabled = false;
            return light;
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
            ScrollRect scroll = rail.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;
            RectTransform viewport = CreateRect("NavigationViewport", rail.transform);
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            RectTransform content = CreateRect("NavigationContent", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            VerticalLayoutGroup group = content.gameObject.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(16, 16, 22, 18);
            group.spacing = 8f;
            group.childControlHeight = false;
            group.childForceExpandHeight = false;
            ContentSizeFitter contentFitter = content.gameObject.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport;
            scroll.content = content;

            CreateTextWithLayout("RailTitle", content, "PETRI LAB", 20, FontStyle.Bold, theme.textPrimary, 44f);
            CreateTextWithLayout("RailSectionLabel", content, "WORKSPACE", 12, FontStyle.Bold,
                theme.textSecondary, 28f);
            string[] names = { "Lab", "New Experiment", "Compare", "Journal", "Collection", "Challenges" };
            string[] icons = { "\u2302", "+", "\u21C4", "\u2261", "\u25CE", "!" };
            for (int i = 0; i < names.Length; i++)
                CreateNavigationButton("Nav" + names[i].Replace(" ", string.Empty) + "Button", content, names[i], icons[i], theme, i == 0);

            RectTransform spacer = CreateRect("NavigationSpacer", content);
            LayoutElement spacerLayout = spacer.gameObject.AddComponent<LayoutElement>();
            spacerLayout.preferredHeight = 120f;
            Image divider = CreateImage("SettingsDivider", content, theme.border);
            LayoutElement dividerLayout = divider.gameObject.AddComponent<LayoutElement>();
            dividerLayout.preferredHeight = 1f;
            dividerLayout.minHeight = 1f;
            CreateNavigationButton("NavSettingsButton", content, "Settings", "\u2699", theme, false);
            return rail;
        }

        private static void BuildScene(PetriDishUITheme theme)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "LaboratoryHub";

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            GameObject cameraObject = new GameObject("LaboratoryHubCamera", typeof(Camera));
            Camera hubCamera = cameraObject.GetComponent<Camera>();
            hubCamera.clearFlags = CameraClearFlags.SolidColor;
            hubCamera.backgroundColor = theme.background;
            hubCamera.cullingMask = 0;
            hubCamera.depth = -100f;
            hubCamera.orthographic = true;
            hubCamera.targetTexture = null;
            GameObject root = CreateObject("LaboratoryHub", null, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.AddComponent<PetriDishRuntimeScene>().Configure(PetriDishSceneRole.NonExperiment);
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
                new Vector2(0f, 0f), new Vector2(0.60f, 1f), theme.textPrimary);
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
            GameObject dishDisplay = InstantiatePrefab(DisplayPrefabPath, null);
            dishDisplay.name = "SelectedDish3DDisplay";
            RawImage dishOutput = FindChild(featured.transform, "DishDisplayImage").GetComponent<RawImage>();
            PetriDishDisplayPresenter dishPresenter = dishDisplay.GetComponent<PetriDishDisplayPresenter>();
            dishPresenter.ConfigureOutput(dishOutput);
            dishPresenter.ConfigureFraming(HubDishFramingScale, HubDishVerticalOffset);

            GameObject notes = CreatePanel("LabNotesPanel", body, theme.panel, theme);
            LayoutElement notesLayout = notes.AddComponent<LayoutElement>();
            notesLayout.preferredWidth = theme.notesWidth;
            notesLayout.flexibleWidth = 0f;
            BuildNotes(notes.transform, theme);

            RectTransform footer = CreateRect("PrimaryActions", safeArea);
            Anchor(footer, new Vector2(0f, 0.012f), new Vector2(1f, 0.095f), new Vector2(28f, 0f), new Vector2(-28f, 0f));
            HorizontalLayoutGroup footerLayout = footer.gameObject.AddComponent<HorizontalLayoutGroup>();
            footerLayout.spacing = 0f;
            footerLayout.childControlWidth = true;
            footerLayout.childControlHeight = true;
            footerLayout.childForceExpandWidth = false;
            footerLayout.childForceExpandHeight = true;
            RectTransform footerSpacer = CreateRect("ActionLeftInset", footer);
            LayoutElement footerSpacerLayout = footerSpacer.gameObject.AddComponent<LayoutElement>();
            footerSpacerLayout.preferredWidth = theme.navigationWidth + theme.standardSpacing;

            Image actionDock = CreateImage("ActionDock", footer, new Color(theme.panel.r, theme.panel.g, theme.panel.b, 0.96f));
            LayoutElement actionDockLayout = actionDock.gameObject.AddComponent<LayoutElement>();
            actionDockLayout.flexibleWidth = 1f;
            HorizontalLayoutGroup actionLayout = actionDock.gameObject.AddComponent<HorizontalLayoutGroup>();
            actionLayout.padding = new RectOffset(18, 12, 10, 10);
            actionLayout.spacing = 12f;
            actionLayout.childControlWidth = true;
            actionLayout.childControlHeight = true;
            actionLayout.childForceExpandWidth = false;
            actionLayout.childForceExpandHeight = true;
            RectTransform actionSpacer = CreateRect("ActionSpacer", actionDock.transform);
            LayoutElement promptLayout = actionSpacer.gameObject.AddComponent<LayoutElement>();
            promptLayout.flexibleWidth = 1f;
            Button compare = CreateButton("CompareButton", actionDock.transform, "COMPARE", theme, false);
            LayoutElement compareLayout = compare.gameObject.AddComponent<LayoutElement>();
            compareLayout.preferredWidth = 170f;
            Button newExperiment = CreateButton("NewExperimentButton", actionDock.transform, "+  START NEW EXPERIMENT", theme, true);
            LayoutElement newLayout = newExperiment.gameObject.AddComponent<LayoutElement>();
            newLayout.preferredWidth = 300f;
            RectTransform footerSpacerRight = CreateRect("ActionRightInset", footer);
            LayoutElement footerSpacerRightLayout = footerSpacerRight.gameObject.AddComponent<LayoutElement>();
            footerSpacerRightLayout.preferredWidth = theme.notesWidth + theme.standardSpacing;

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
                footerSpacerLayout, footerSpacerRightLayout, drawerButton.gameObject, drawer);
            drawerButton.onClick.AddListener(responsive.ToggleNotesDrawer);

            LaboratoryHubPresenter presenter = root.AddComponent<LaboratoryHubPresenter>();
            List<Button> actions = new List<Button> { newExperiment, compare };
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
            CreateTextAnchored("DishName", panel.transform, "Dish A", 40, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.05f, 0.835f), new Vector2(0.34f, 0.925f), theme.textPrimary);
            CreateStatusBadge("StatusBadge", panel.transform, "\u25CF  Growing well", theme.green, theme,
                new Vector2(0.35f, 0.855f), new Vector2(0.58f, 0.91f));
            CreateTextAnchored("Organism", panel.transform, "Bacillus subtilis", 24, FontStyle.Italic,
                TextAnchor.MiddleLeft, new Vector2(0.05f, 0.775f), new Vector2(0.58f, 0.84f), theme.textPrimary);
            CreateTextAnchored("Medium", panel.transform, "Nutrient Agar", 17, FontStyle.Normal,
                TextAnchor.MiddleLeft, new Vector2(0.05f, 0.72f), new Vector2(0.58f, 0.78f), theme.textSecondary);

            RectTransform previewWell = CreateRect("DishPreviewWell", panel.transform);
            Anchor(previewWell, new Vector2(0.035f, 0.145f), new Vector2(0.635f, 0.720f), Vector2.zero, Vector2.zero);
            GameObject preview = CreateObject("DishDisplayImage", previewWell.transform,
                typeof(RectTransform), typeof(RawImage));
            Anchor(preview.GetComponent<RectTransform>(), new Vector2(0.01f, 0.01f), new Vector2(0.99f, 0.99f), Vector2.zero, Vector2.zero);
            RawImage previewImage = preview.GetComponent<RawImage>();
            previewImage.color = Color.white;
            previewImage.raycastTarget = false;
            AspectRatioFitter fitter = preview.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 1f;

            Image summary = CreateImage("CultureSummary", panel.transform,
                new Color(theme.panelRaised.r, theme.panelRaised.g, theme.panelRaised.b, 0.82f));
            Anchor(summary.rectTransform, new Vector2(0.65f, 0.285f), new Vector2(0.955f, 0.720f), Vector2.zero, Vector2.zero);
            CreateTextAnchored("SummaryHeading", summary.transform, "Culture summary", 19, FontStyle.Bold,
                TextAnchor.MiddleLeft, new Vector2(0.07f, 0.84f), new Vector2(0.93f, 0.97f), theme.textPrimary);
            CreateSummaryRow("AgeMetric", summary.transform, "Age", "18 h", theme,
                new Vector2(0.07f, 0.69f), new Vector2(0.93f, 0.83f));
            CreateSummaryRow("CoverageMetric", summary.transform, "Coverage", "42%", theme,
                new Vector2(0.07f, 0.55f), new Vector2(0.93f, 0.69f));
            Image summaryDivider = CreateImage("SummaryDivider", summary.transform, theme.border);
            Anchor(summaryDivider.rectTransform, new Vector2(0.07f, 0.525f), new Vector2(0.93f, 0.529f), Vector2.zero, Vector2.zero);
            CreateTextAnchored("EnvironmentHeading", summary.transform, "Environment", 14, FontStyle.Bold,
                TextAnchor.MiddleLeft, new Vector2(0.07f, 0.43f), new Vector2(0.93f, 0.52f), theme.textSecondary);
            RectTransform environment = CreateRect("EnvironmentSummary", summary.transform);
            Anchor(environment, new Vector2(0.07f, 0.06f), new Vector2(0.93f, 0.42f), Vector2.zero, Vector2.zero);
            CreateEnvironmentRow("Temperature", environment, "26\u00B0C", "Temperature", theme,
                new Vector2(0f, 0.67f), Vector2.one);
            CreateEnvironmentRow("Moisture", environment, "42%", "Moisture", theme,
                new Vector2(0f, 0.34f), new Vector2(1f, 0.67f));
            CreateEnvironmentRow("Nutrients", environment, "OK", "Nutrients", theme,
                Vector2.zero, new Vector2(1f, 0.34f));

            Button open = CreateButton("OpenDishButton", panel.transform, "OPEN DISH", theme, true);
            Anchor(open.GetComponent<RectTransform>(), new Vector2(0.68f, 0.17f), new Vector2(0.925f, 0.255f), Vector2.zero, Vector2.zero);

            RectTransform navigation = CreateRect("DishNavigation", panel.transform);
            Anchor(navigation, new Vector2(0.14f, 0.045f), new Vector2(0.60f, 0.125f), Vector2.zero, Vector2.zero);
            Button previous = CreateQuietButton("PreviousDishButton", navigation, "\u2039", theme);
            Anchor(previous.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0.16f, 1f), Vector2.zero, Vector2.zero);
            previous.interactable = false;
            ConfigureDisabledDishNavigationButton(previous, theme);
            CreateTextAnchored("DishNavigationState", navigation.transform, "Dish A     1 / 1", 16,
                FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0.17f, 0f), new Vector2(0.83f, 1f), theme.textPrimary);
            Button next = CreateQuietButton("NextDishButton", navigation, "\u203A", theme);
            Anchor(next.GetComponent<RectTransform>(), new Vector2(0.84f, 0f), Vector2.one, Vector2.zero, Vector2.zero);
            next.interactable = false;
            ConfigureDisabledDishNavigationButton(next, theme);
            return panel;
        }

        private static void CreateSummaryRow(string name, Transform parent, string label, string value,
            PetriDishUITheme theme, Vector2 min, Vector2 max)
        {
            RectTransform row = CreateRect(name, parent);
            Anchor(row, min, max, Vector2.zero, Vector2.zero);
            CreateTextAnchored("Label", row, label, 15, FontStyle.Normal, TextAnchor.MiddleLeft,
                Vector2.zero, new Vector2(0.56f, 1f), theme.textSecondary);
            CreateTextAnchored("Value", row, value, 21, FontStyle.Bold, TextAnchor.MiddleRight,
                new Vector2(0.48f, 0f), Vector2.one, theme.textPrimary);
        }

        private static void CreateEnvironmentRow(string name, Transform parent, string value, string label,
            PetriDishUITheme theme, Vector2 min, Vector2 max)
        {
            RectTransform row = CreateRect(name, parent);
            Anchor(row, min, max, Vector2.zero, Vector2.zero);
            CreateTextAnchored("Label", row, label, 13, FontStyle.Normal, TextAnchor.MiddleLeft,
                Vector2.zero, new Vector2(0.62f, 1f), theme.textSecondary);
            CreateTextAnchored("Value", row, value, 16, FontStyle.Bold, TextAnchor.MiddleRight,
                new Vector2(0.58f, 0f), Vector2.one, name == "Nutrients" ? theme.green : theme.textPrimary);
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
            group.padding = new RectOffset(24, 24, 24, 20);
            group.spacing = 6f;
            group.childControlHeight = true;
            group.childControlWidth = true;
            group.childForceExpandHeight = false;
            group.childForceExpandWidth = true;
            CreateTextWithLayout("Heading", parent, "Lab Notes", 29, FontStyle.Bold, theme.textPrimary, 48f);
            CreateTextWithLayout("Subheading", parent, "Observations from the selected culture", 16,
                FontStyle.Normal, theme.textSecondary, 36f);
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
                Transform instanceAccent = FindChild(instance.transform, "AccentLine");
                if (instanceAccent != null) instanceAccent.GetComponent<Image>().color = accent;
                if (name == "CurrentObservation")
                {
                    LayoutElement observationLayout = instance.GetComponent<LayoutElement>();
                    observationLayout.minHeight = 128f;
                    observationLayout.preferredHeight = 162f;
                    observationLayout.flexibleHeight = 1.2f;
                    instance.GetComponent<Image>().color = theme.panelRaised;
                    Anchor(instanceAccent.GetComponent<RectTransform>(),
                        new Vector2(0f, 0.10f), new Vector2(0.018f, 0.90f), Vector2.zero, Vector2.zero);
                }
                return instance;
            }

            GameObject card = CreateImage(name, parent, theme.panel).gameObject;
            LayoutElement layout = card.AddComponent<LayoutElement>();
            layout.minHeight = 116f;
            layout.preferredHeight = 146f;
            layout.flexibleHeight = 1f;
            Image accentLine = CreateImage("AccentLine", card.transform, accent);
            Anchor(accentLine.rectTransform, new Vector2(0f, 0.12f), new Vector2(0.012f, 0.88f), Vector2.zero, Vector2.zero);
            Image divider = CreateImage("NoteDivider", card.transform, theme.border);
            Anchor(divider.rectTransform, Vector2.zero, new Vector2(1f, 0.006f), Vector2.zero, Vector2.zero);
            CreateTextAnchored("Category", card.transform, category, 13, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0.76f), new Vector2(0.96f, 0.96f), accent);
            CreateTextAnchored("Title", card.transform, title, 19, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0.34f), new Vector2(0.96f, 0.77f), theme.textPrimary);
            CreateTextAnchored("Detail", card.transform, detail, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0.06f), new Vector2(0.96f, 0.35f), theme.textSecondary);
            return card;
        }

        private static Button CreateNavigationButton(string name, Transform parent, string label, string icon,
            PetriDishUITheme theme, bool selected)
        {
            Button button = CreateButton(name, parent, "", theme, false);
            LayoutElement layout = button.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 68f;
            if (selected)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = new Color(theme.cyan.r, theme.cyan.g, theme.cyan.b, 0.055f);
                colors.selectedColor = new Color(theme.cyan.r, theme.cyan.g, theme.cyan.b, 0.09f);
                button.colors = colors;
                Image edge = CreateImage("SelectedEdge", button.transform, theme.cyan);
                Anchor(edge.rectTransform, new Vector2(0f, 0.18f), new Vector2(0.012f, 0.82f), Vector2.zero, Vector2.zero);
            }
            Image iconPlate = CreateImage("IconPlate", button.transform,
                selected ? new Color(theme.cyan.r, theme.cyan.g, theme.cyan.b, 0.10f) : theme.panelRaised);
            Anchor(iconPlate.rectTransform, new Vector2(0.04f, 0.17f), new Vector2(0.25f, 0.83f), Vector2.zero, Vector2.zero);
            CreateTextAnchored("Icon", iconPlate.transform, icon, 16, FontStyle.Bold, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, selected ? theme.cyan : theme.textSecondary);
            CreateTextAnchored("Label", button.transform, label, 18, selected ? FontStyle.Bold : FontStyle.Normal,
                TextAnchor.MiddleLeft, new Vector2(0.30f, 0f), new Vector2(0.97f, 1f),
                selected ? theme.cyan : theme.textPrimary);
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

        private static Button CreateQuietButton(string name, Transform parent, string label, PetriDishUITheme theme)
        {
            GameObject owner = CreateObject(name, parent, typeof(RectTransform), typeof(Image), typeof(Button));
            Image image = owner.GetComponent<Image>();
            image.color = Color.white;
            Button button = owner.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = new Color(theme.panelHover.r, theme.panelHover.g, theme.panelHover.b, 0.72f);
            colors.pressedColor = theme.bench;
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(theme.panelRaised.r, theme.panelRaised.g, theme.panelRaised.b, 0.35f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            CreateTextAnchored("Label", owner.transform, label, 15, FontStyle.Normal, TextAnchor.MiddleCenter,
                new Vector2(0.04f, 0f), new Vector2(0.96f, 1f), theme.textSecondary);
            return button;
        }

        private static void ConfigureDisabledDishNavigationButton(Button button, PetriDishUITheme theme)
        {
            ColorBlock colors = button.colors;
            colors.disabledColor = new Color(theme.panelRaised.r, theme.panelRaised.g, theme.panelRaised.b, 0.72f);
            button.colors = colors;
            Text label = button.GetComponentInChildren<Text>(true);
            label.color = new Color(theme.textSecondary.r, theme.textSecondary.g, theme.textSecondary.b, 0.88f);
            label.fontStyle = FontStyle.Bold;
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
            int scaledSize = Mathf.CeilToInt(size * TypographyScale);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = scaledSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.CeilToInt(12f * TypographyScale);
            text.resizeTextMaxSize = scaledSize;
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
