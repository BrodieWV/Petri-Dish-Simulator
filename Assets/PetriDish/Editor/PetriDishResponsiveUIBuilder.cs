using PetriDish.Presentation;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PetriDish.Editor
{
    public static class PetriDishResponsiveUIBuilder
    {
        private const string RootName = "PetriDishResponsiveUI";

        private static readonly Color Background = new Color(0.025f, 0.045f, 0.038f, 1f);
        private static readonly Color Panel = new Color(0.055f, 0.090f, 0.074f, 0.98f);
        private static readonly Color PanelRaised = new Color(0.075f, 0.120f, 0.098f, 0.98f);
        private static readonly Color Accent = new Color(0.33f, 0.72f, 0.48f, 1f);
        private static readonly Color TextPrimary = new Color(0.92f, 0.97f, 0.93f, 1f);
        private static readonly Color TextMuted = new Color(0.62f, 0.74f, 0.66f, 1f);

        [MenuItem("Petri Dish/UI/Build Responsive Interface %#u")]
        public static void BuildResponsiveInterface()
        {
            if (global::UnityEngine.Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Petri Dish UI", "Stop Play Mode before building the interface.", "OK");
                return;
            }

            GameObject existing = GameObject.Find(RootName);
            if (existing != null)
            {
                bool replace = EditorUtility.DisplayDialog(
                    "Replace generated UI?",
                    "A generated responsive UI already exists in this scene. Replace it?",
                    "Replace",
                    "Cancel");

                if (!replace) return;
                Undo.DestroyObjectImmediate(existing);
            }

            EnsureEventSystem();

            GameObject root = CreateObject(RootName, null, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform rootRect = root.GetComponent<RectTransform>();
            Stretch(rootRect);

            Image background = CreateImage("Background", root.transform, Background);
            Stretch(background.rectTransform);

            RectTransform safeArea = CreateRect("SafeArea", root.transform);
            Stretch(safeArea);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            RectTransform header = CreatePanel("Header", safeArea, PanelRaised);
            SetAnchors(header, new Vector2(0f, 0.91f), Vector2.one, new Vector2(18f, 8f), new Vector2(-18f, -10f));
            CreateLabel("Title", header, "PETRI DISH LAB", 28, TextAnchor.MiddleLeft, FontStyle.Bold,
                new Vector2(0.025f, 0f), new Vector2(0.42f, 1f), TextPrimary);
            CreateLabel("ExperimentName", header, "Experiment 01  •  Bacillus subtilis  •  Nutrient Agar", 19,
                TextAnchor.MiddleCenter, FontStyle.Normal, new Vector2(0.34f, 0f), new Vector2(0.76f, 1f), TextMuted);
            CreateButton("PauseButton", header, "Pause", new Vector2(0.78f, 0.16f), new Vector2(0.875f, 0.84f));
            CreateButton("SettingsButton", header, "Settings", new Vector2(0.885f, 0.16f), new Vector2(0.975f, 0.84f));

            RectTransform body = CreateRect("Body", safeArea);
            SetAnchors(body, new Vector2(0.01f, 0.105f), new Vector2(0.99f, 0.90f), Vector2.zero, Vector2.zero);

            RectTransform leftPanel = CreatePanel("SetupPanel", body, Panel);
            BuildSetupPanel(leftPanel);

            RectTransform centrePanel = CreatePanel("DishViewportPanel", body, new Color(0.035f, 0.065f, 0.053f, 1f));
            BuildDishPanel(centrePanel);

            RectTransform rightPanel = CreatePanel("DataPanel", body, Panel);
            BuildDataPanel(rightPanel);

            RectTransform footer = CreatePanel("BottomControls", safeArea, PanelRaised);
            SetAnchors(footer, new Vector2(0.01f, 0.01f), new Vector2(0.99f, 0.095f), Vector2.zero, Vector2.zero);
            BuildFooter(footer);

            RectTransform leftDrawerButton = CreateButton("SetupDrawerButton", safeArea, "Setup",
                new Vector2(0.012f, 0.79f), new Vector2(0.14f, 0.88f)).GetComponent<RectTransform>();
            RectTransform rightDrawerButton = CreateButton("DataDrawerButton", safeArea, "Data",
                new Vector2(0.86f, 0.79f), new Vector2(0.988f, 0.88f)).GetComponent<RectTransform>();

            ResponsivePetriDishLayout layout = body.gameObject.AddComponent<ResponsivePetriDishLayout>();
            layout.Configure(leftPanel, centrePanel, rightPanel, leftDrawerButton, rightDrawerButton);

            UnityEventTools.AddPersistentListener(leftDrawerButton.GetComponent<Button>().onClick, layout.ToggleLeftDrawer);
            UnityEventTools.AddPersistentListener(rightDrawerButton.GetComponent<Button>().onClick, layout.ToggleRightDrawer);

            PetriDishResponsiveUIBinder binder =
                root.AddComponent<PetriDishResponsiveUIBinder>();
            binder.AutoAssignReferences();

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Undo.RegisterCreatedObjectUndo(root, "Build responsive Petri Dish UI");

            EditorUtility.DisplayDialog(
                "Responsive UI created",
                "The editable interface was generated under PetriDishResponsiveUI. Save the scene, then adjust panels, colours, text, and spacing normally in the Inspector.",
                "OK");
        }

        [MenuItem("Petri Dish/UI/Integrate Responsive UI Into Vertical Slice")]
        public static void IntegrateResponsiveUIIntoVerticalSlice()
        {
            const string prototypePath =
                "Assets/PetriDish/Scenes/PetriDishUIPrototype.unity";
            const string verticalSlicePath =
                "Assets/PetriDish/Scenes/PetriDishVerticalSlice.unity";

            Scene prototype = EditorSceneManager.OpenScene(
                prototypePath,
                OpenSceneMode.Single);
            GameObject prototypeRoot = FindSceneRoot(prototype, RootName);
            if (prototypeRoot == null)
                throw new System.InvalidOperationException(
                    "The responsive UI prototype root is missing.");

            PetriDishResponsiveUIBinder prototypeBinder =
                prototypeRoot.GetComponent<PetriDishResponsiveUIBinder>();
            if (prototypeBinder == null)
                prototypeBinder = prototypeRoot.AddComponent<PetriDishResponsiveUIBinder>();
            if (!prototypeBinder.AutoAssignReferences())
                throw new System.InvalidOperationException(
                    "The responsive UI prototype hierarchy is incomplete.");
            EditorSceneManager.MarkSceneDirty(prototype);
            EditorSceneManager.SaveScene(prototype);

            GameObject clone = Object.Instantiate(prototypeRoot);
            clone.name = RootName;
            Scene verticalSlice = EditorSceneManager.OpenScene(
                verticalSlicePath,
                OpenSceneMode.Additive);
            GameObject existing = FindSceneRoot(verticalSlice, RootName);
            if (existing == null)
            {
                SceneManager.MoveGameObjectToScene(clone, verticalSlice);
            }
            else
            {
                Object.DestroyImmediate(clone);
                PetriDishResponsiveUIBinder existingBinder =
                    existing.GetComponent<PetriDishResponsiveUIBinder>();
                if (existingBinder == null)
                    existingBinder = existing.AddComponent<PetriDishResponsiveUIBinder>();
                if (!existingBinder.AutoAssignReferences())
                    throw new System.InvalidOperationException(
                        "The responsive UI hierarchy in the vertical slice is incomplete.");
            }

            EditorSceneManager.MarkSceneDirty(verticalSlice);
            EditorSceneManager.SaveScene(verticalSlice);
            EditorSceneManager.CloseScene(prototype, true);
            SceneManager.SetActiveScene(verticalSlice);
            Selection.activeGameObject = FindSceneRoot(verticalSlice, RootName);
        }

        private static GameObject FindSceneRoot(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
                if (roots[i].name == name) return roots[i];
            return null;
        }
        private static void BuildSetupPanel(RectTransform panel)
        {
            CreateSectionTitle(panel, "EXPERIMENT SETUP", 0.89f, 0.98f);
            CreateLabel("OrganismLabel", panel, "Organism", 18, TextAnchor.MiddleLeft, FontStyle.Bold,
                new Vector2(0.07f, 0.80f), new Vector2(0.93f, 0.87f), TextMuted);
            CreateButton("OrganismButton", panel, "Bacillus subtilis", new Vector2(0.06f, 0.69f), new Vector2(0.94f, 0.80f));

            CreateLabel("MediumLabel", panel, "Growth medium", 18, TextAnchor.MiddleLeft, FontStyle.Bold,
                new Vector2(0.07f, 0.60f), new Vector2(0.93f, 0.67f), TextMuted);
            CreateButton("MediumButton", panel, "Nutrient Agar", new Vector2(0.06f, 0.49f), new Vector2(0.94f, 0.60f));

            CreateLabel("EnvironmentLabel", panel, "Environment", 18, TextAnchor.MiddleLeft, FontStyle.Bold,
                new Vector2(0.07f, 0.40f), new Vector2(0.93f, 0.47f), TextMuted);
            CreateButton("TemperatureButton", panel, "Temperature  30.0°C", new Vector2(0.06f, 0.30f), new Vector2(0.94f, 0.40f));
            CreateButton("MoistureButton", panel, "Moisture  70%", new Vector2(0.06f, 0.19f), new Vector2(0.94f, 0.29f));
            CreateButton("LightingButton", panel, "Lighting  Neutral", new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.18f));
        }

        private static void BuildDishPanel(RectTransform panel)
        {
            CreateLabel("DishTitle", panel, "LIVE CULTURE", 22, TextAnchor.MiddleLeft, FontStyle.Bold,
                new Vector2(0.04f, 0.91f), new Vector2(0.46f, 0.985f), TextPrimary);
            CreateLabel("ConditionLabel", panel, "Condition: Comfortable", 18, TextAnchor.MiddleRight, FontStyle.Normal,
                new Vector2(0.50f, 0.91f), new Vector2(0.96f, 0.985f), Accent);

            Image viewport = CreateImage("DishRenderTarget", panel, new Color(0.015f, 0.025f, 0.021f, 1f));
            SetAnchors(viewport.rectTransform, new Vector2(0.04f, 0.14f), new Vector2(0.96f, 0.90f), Vector2.zero, Vector2.zero);
            viewport.raycastTarget = false;

            CreateLabel("ViewportHint", viewport.transform, "3D PETRI DISH VIEWPORT\n\nConnect the scene camera or render texture here", 21,
                TextAnchor.MiddleCenter, FontStyle.Normal, new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f), TextMuted);

            CreateLabel("InspectionText", panel, "Select a colony on the dish to inspect local growth conditions.", 17,
                TextAnchor.MiddleLeft, FontStyle.Normal, new Vector2(0.04f, 0.035f), new Vector2(0.96f, 0.125f), TextMuted);
        }

        private static void BuildDataPanel(RectTransform panel)
        {
            CreateSectionTitle(panel, "LIVE DATA", 0.89f, 0.98f);
            CreateMetric(panel, "TemperatureMetric", "Temperature", "30.0°C", 0.77f);
            CreateMetric(panel, "CoverageMetric", "Coverage", "12%", 0.64f);
            CreateMetric(panel, "MoistureMetric", "Moisture", "70%", 0.51f);
            CreateMetric(panel, "NutrientsMetric", "Nutrients", "Stable", 0.38f);

            CreateLabel("InterventionTitle", panel, "INTERVENTIONS", 18, TextAnchor.MiddleLeft, FontStyle.Bold,
                new Vector2(0.07f, 0.25f), new Vector2(0.93f, 0.32f), TextMuted);
            CreateButton("AddMoistureButton", panel, "Add moisture", new Vector2(0.06f, 0.14f), new Vector2(0.94f, 0.24f));
            CreateButton("AddNutrientsButton", panel, "Add nutrients  (3)", new Vector2(0.06f, 0.03f), new Vector2(0.94f, 0.13f));
        }

        private static void BuildFooter(RectTransform footer)
        {
            CreateButton("SaveButton", footer, "Save", new Vector2(0.015f, 0.17f), new Vector2(0.13f, 0.83f));
            CreateButton("LoadButton", footer, "Load", new Vector2(0.14f, 0.17f), new Vector2(0.255f, 0.83f));
            CreateButton("RestartButton", footer, "Restart", new Vector2(0.265f, 0.17f), new Vector2(0.39f, 0.83f));
            CreateLabel("SimulationState", footer, "Day 1  •  08:42  •  Simulation running", 18,
                TextAnchor.MiddleCenter, FontStyle.Normal, new Vector2(0.40f, 0.08f), new Vector2(0.72f, 0.92f), TextMuted);
            CreateButton("SpeedButton", footer, "Speed  1×", new Vector2(0.73f, 0.17f), new Vector2(0.855f, 0.83f));
            CreateButton("NewSeedButton", footer, "New seed", new Vector2(0.865f, 0.17f), new Vector2(0.985f, 0.83f));
        }

        private static void CreateMetric(RectTransform parent, string name, string label, string value, float top)
        {
            Image card = CreateImage(name, parent, PanelRaised);
            SetAnchors(card.rectTransform, new Vector2(0.06f, top - 0.10f), new Vector2(0.94f, top), Vector2.zero, Vector2.zero);
            CreateLabel("Label", card.transform, label, 17, TextAnchor.MiddleLeft, FontStyle.Normal,
                new Vector2(0.05f, 0f), new Vector2(0.62f, 1f), TextMuted);
            CreateLabel("Value", card.transform, value, 20, TextAnchor.MiddleRight, FontStyle.Bold,
                new Vector2(0.60f, 0f), new Vector2(0.95f, 1f), TextPrimary);
        }

        private static void CreateSectionTitle(RectTransform parent, string text, float minY, float maxY)
        {
            CreateLabel("SectionTitle", parent, text, 22, TextAnchor.MiddleLeft, FontStyle.Bold,
                new Vector2(0.07f, minY), new Vector2(0.93f, maxY), TextPrimary);
        }

        private static GameObject CreateObject(string name, Transform parent, params System.Type[] components)
        {
            GameObject go = new GameObject(name, components);
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            return CreateObject(name, parent, typeof(RectTransform)).GetComponent<RectTransform>();
        }

        private static RectTransform CreatePanel(string name, Transform parent, Color colour)
        {
            return CreateImage(name, parent, colour).rectTransform;
        }

        private static Image CreateImage(string name, Transform parent, Color colour)
        {
            Image image = CreateObject(name, parent, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            image.color = colour;
            return image;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 min, Vector2 max)
        {
            Button button = CreateObject(name, parent, typeof(RectTransform), typeof(Image), typeof(Button)).GetComponent<Button>();
            button.GetComponent<Image>().color = new Color(0.105f, 0.18f, 0.14f, 1f);
            ColorBlock colours = button.colors;
            colours.highlightedColor = new Color(0.16f, 0.27f, 0.20f, 1f);
            colours.pressedColor = new Color(0.08f, 0.14f, 0.11f, 1f);
            button.colors = colours;
            SetAnchors(button.GetComponent<RectTransform>(), min, max, Vector2.zero, Vector2.zero);
            CreateLabel("Label", button.transform, label, 18, TextAnchor.MiddleCenter, FontStyle.Bold,
                Vector2.zero, Vector2.one, TextPrimary);
            return button;
        }

        private static Button CreateButton(string name, Transform parent, string label)
        {
            return CreateButton(name, parent, label, Vector2.zero, Vector2.one);
        }

        private static Text CreateLabel(string name, Transform parent, string value, int size, TextAnchor anchor,
            FontStyle style, Vector2 min, Vector2 max, Color colour)
        {
            Text text = CreateObject(name, parent, typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = colour;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = size;
            text.raycastTarget = false;
            SetAnchors(text.rectTransform, min, max, Vector2.zero, Vector2.zero);
            return text;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            GameObject eventSystem = CreateObject("EventSystem", null, typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
        }

        private static void Stretch(RectTransform rect)
        {
            SetAnchors(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}


