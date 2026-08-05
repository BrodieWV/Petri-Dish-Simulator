using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace PetriDish.Editor
{
    public static class PetriDishScienceUIStyler
    {
        private static readonly Color Background = new Color(0.012f, 0.024f, 0.040f, 1f);
        private static readonly Color Panel = new Color(0.025f, 0.055f, 0.082f, 0.98f);
        private static readonly Color PanelRaised = new Color(0.035f, 0.080f, 0.112f, 0.98f);
        private static readonly Color Cyan = new Color(0.18f, 0.88f, 0.95f, 1f);
        private static readonly Color Teal = new Color(0.18f, 0.68f, 0.72f, 1f);
        private static readonly Color TextPrimary = new Color(0.90f, 0.97f, 1f, 1f);
        private static readonly Color TextMuted = new Color(0.55f, 0.72f, 0.80f, 1f);
        private static readonly Color Grid = new Color(0.18f, 0.88f, 0.95f, 0.16f);

        [MenuItem("Petri Dish/UI/Apply Science Laboratory Style %#l")]
        public static void ApplyScienceStyle()
        {
            GameObject root = GameObject.Find("PetriDishResponsiveUI");
            if (root == null)
            {
                EditorUtility.DisplayDialog("Science UI", "Build the responsive interface first.", "OK");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(root, "Apply science laboratory UI style");

            SetImage(root.transform, "Background", Background);
            SetImage(root.transform, "Header", PanelRaised);
            SetImage(root.transform, "SetupPanel", Panel);
            SetImage(root.transform, "DishViewportPanel", new Color(0.018f, 0.038f, 0.060f, 1f));
            SetImage(root.transform, "DataPanel", Panel);
            SetImage(root.transform, "BottomControls", PanelRaised);
            SetImage(root.transform, "DishRenderTarget", new Color(0.006f, 0.014f, 0.024f, 1f));

            foreach (Button button in root.GetComponentsInChildren<Button>(true))
            {
                Image image = button.GetComponent<Image>();
                if (image != null) image.color = new Color(0.035f, 0.115f, 0.145f, 1f);
                ColorBlock colours = button.colors;
                colours.highlightedColor = new Color(0.06f, 0.22f, 0.27f, 1f);
                colours.pressedColor = new Color(0.02f, 0.08f, 0.11f, 1f);
                colours.selectedColor = colours.highlightedColor;
                button.colors = colours;
                AddOutline(button.gameObject, Cyan, new Vector2(1f, -1f));
            }

            foreach (Text text in root.GetComponentsInChildren<Text>(true))
            {
                text.color = TextPrimary;
                if (text.name.Contains("Label") || text.name.Contains("State") || text.name.Contains("Hint"))
                    text.color = TextMuted;
            }

            RenameText(root.transform, "Title", "MICROBIOLOGY CULTURE STATION");
            RenameText(root.transform, "ExperimentName", "RUN ID: PD-001  •  SAMPLE: B. subtilis  •  MEDIUM: NUTRIENT AGAR");
            RenameText(root.transform, "SectionTitle", "CULTURE PARAMETERS");
            RenameText(root.transform, "DishTitle", "CULTURE CHAMBER // LIVE FEED");
            RenameText(root.transform, "ConditionLabel", "STATUS: STABLE");
            RenameText(root.transform, "ViewportHint", "LIVE 3D CULTURE VIEWPORT\n\nOPTICAL OVERLAY ACTIVE");
            RenameText(root.transform, "InspectionText", "SELECT A COLONY TO INSPECT LOCAL GROWTH CONDITIONS");
            RenameText(root.transform, "SimulationState", "T+00:08:42  •  CYCLE 001  •  SIMULATION RUNNING");

            Transform viewport = FindDeep(root.transform, "DishRenderTarget");
            if (viewport != null)
            {
                Transform oldOverlay = viewport.Find("ScienceOverlay");
                if (oldOverlay != null) Undo.DestroyObjectImmediate(oldOverlay.gameObject);
                BuildScienceOverlay(viewport);
                AddOutline(viewport.gameObject, Cyan, new Vector2(2f, -2f));
            }

            AddStatusLight(root.transform, "TemperatureMetric", Cyan);
            AddStatusLight(root.transform, "CoverageMetric", Teal);
            AddStatusLight(root.transform, "MoistureMetric", Cyan);
            AddStatusLight(root.transform, "NutrientsMetric", new Color(0.95f, 0.72f, 0.20f, 1f));

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("Science UI", "Science laboratory styling applied. Save the scene with Ctrl+S.", "OK");
        }

        private static void BuildScienceOverlay(Transform parent)
        {
            GameObject overlay = new GameObject("ScienceOverlay", typeof(RectTransform));
            overlay.transform.SetParent(parent, false);
            Stretch(overlay.GetComponent<RectTransform>());

            for (int i = 1; i < 10; i++)
            {
                float t = i / 10f;
                CreateLine(overlay.transform, "GridV" + i, new Vector2(t, 0f), new Vector2(t, 1f), new Vector2(1f, 0f));
                CreateLine(overlay.transform, "GridH" + i, new Vector2(0f, t), new Vector2(1f, t), new Vector2(0f, 1f));
            }

            CreateLine(overlay.transform, "CrosshairV", new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.56f), new Vector2(2f, 0f));
            CreateLine(overlay.transform, "CrosshairH", new Vector2(0.44f, 0.5f), new Vector2(0.56f, 0.5f), new Vector2(0f, 2f));

            Text readout = CreateText(overlay.transform, "InstrumentReadout", "MAG  1.0×\nGRID  10 mm\nFEED  LIVE", 14, TextAnchor.UpperLeft, TextMuted);
            RectTransform rect = readout.rectTransform;
            rect.anchorMin = new Vector2(0.025f, 0.79f);
            rect.anchorMax = new Vector2(0.24f, 0.97f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void AddStatusLight(Transform root, string cardName, Color colour)
        {
            Transform card = FindDeep(root, cardName);
            if (card == null || card.Find("StatusLight") != null) return;
            GameObject light = new GameObject("StatusLight", typeof(RectTransform), typeof(Image));
            light.transform.SetParent(card, false);
            Image image = light.GetComponent<Image>();
            image.color = colour;
            RectTransform rect = light.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.025f, 0.35f);
            rect.anchorMax = new Vector2(0.045f, 0.65f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void CreateLine(Transform parent, string name, Vector2 min, Vector2 max, Vector2 size)
        {
            GameObject line = new GameObject(name, typeof(RectTransform), typeof(Image));
            line.transform.SetParent(parent, false);
            Image image = line.GetComponent<Image>();
            image.color = Grid;
            image.raycastTarget = false;
            RectTransform rect = line.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
        }

        private static void SetImage(Transform root, string name, Color colour)
        {
            Transform target = FindDeep(root, name);
            if (target == null) return;
            Image image = target.GetComponent<Image>();
            if (image != null) image.color = colour;
        }

        private static void RenameText(Transform root, string name, string value)
        {
            Transform target = FindDeep(root, name);
            if (target == null) return;
            Text text = target.GetComponent<Text>();
            if (text != null) text.text = value;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == name) return child;
            return null;
        }

        private static void AddOutline(GameObject target, Color colour, Vector2 distance)
        {
            Outline outline = target.GetComponent<Outline>();
            if (outline == null) outline = target.AddComponent<Outline>();
            outline.effectColor = new Color(colour.r, colour.g, colour.b, 0.45f);
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        private static Text CreateText(Transform parent, string name, string value, int size, TextAnchor anchor, Color colour)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = colour;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
