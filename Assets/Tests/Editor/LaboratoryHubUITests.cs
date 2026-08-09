using System.Collections;
using System.Linq;
using NUnit.Framework;
using PetriDish.Editor;
using PetriDish.Presentation.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PetriDish.Tests.Editor
{
    public sealed class LaboratoryHubUITests
    {
        [TestCase(1920f, 1080f, false)]
        [TestCase(1366f, 768f, false)]
        [TestCase(1136f, 640f, true)]
        [TestCase(2532f, 1170f, true)]
        [TestCase(1080f, 1920f, false)]
        public void CompactLayoutSelectionMatchesSupportedLandscapeClasses(float width, float height, bool expected)
        {
            Assert.That(LaboratoryHubResponsiveLayout.ShouldUseCompactLayout(width, height, false), Is.EqualTo(expected));
        }

        [Test]
        public void ForcedCompactLayoutOverridesDesktopClassification()
        {
            Assert.That(LaboratoryHubResponsiveLayout.ShouldUseCompactLayout(1920f, 1080f, true), Is.True);
        }

        [Test]
        public void ThemeUsesLightLaboratorySurfacesAndSemanticSignals()
        {
            PetriDishUITheme theme = ScriptableObject.CreateInstance<PetriDishUITheme>();
            try
            {
                Assert.That(theme.background.grayscale, Is.GreaterThan(0.8f));
                Assert.That(theme.panel.grayscale, Is.GreaterThan(0.9f));
                Assert.That(theme.textPrimary.grayscale, Is.LessThan(0.25f));
                Assert.That(theme.border, Is.Not.EqualTo(theme.panel));
                Assert.That(theme.GetStatusColor(LaboratoryDishStatus.Growing), Is.EqualTo(theme.green));
                Assert.That(theme.GetStatusColor(LaboratoryDishStatus.Stressed), Is.EqualTo(theme.amber));
                Assert.That(theme.GetStatusColor(LaboratoryDishStatus.Severe), Is.EqualTo(theme.red));
                Assert.That(theme.GetStatusColor(LaboratoryDishStatus.Paused), Is.EqualTo(theme.textSecondary));
            }
            finally
            {
                Object.DestroyImmediate(theme);
            }
        }

        [Test]
        public void BuilderIsIdempotentAndSerializesSingleDishWorkspace()
        {
            LaboratoryHubEditorBuilder.BuildLaboratoryHubForAutomation();
            string themeGuid = AssetDatabase.AssetPathToGUID(LaboratoryHubEditorBuilder.ThemePath);
            string[] prefabGuidsBefore = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/PetriDish/UI/Prefabs" });

            LaboratoryHubEditorBuilder.BuildLaboratoryHubForAutomation();
            string[] prefabGuidsAfter = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/PetriDish/UI/Prefabs" });
            Scene scene = EditorSceneManager.OpenScene(LaboratoryHubEditorBuilder.ScenePath, OpenSceneMode.Single);
            GameObject[] hubRoots = scene.GetRootGameObjects().Where(owner => owner.name == "LaboratoryHub").ToArray();

            Assert.That(themeGuid, Is.Not.Empty);
            Assert.That(AssetDatabase.AssetPathToGUID(LaboratoryHubEditorBuilder.ThemePath), Is.EqualTo(themeGuid));
            Assert.That(prefabGuidsBefore, Has.Length.EqualTo(12));
            Assert.That(prefabGuidsAfter.OrderBy(value => value), Is.EqualTo(prefabGuidsBefore.OrderBy(value => value)));
            Assert.That(hubRoots, Has.Length.EqualTo(1));

            GameObject hub = hubRoots[0];
            Assert.That(hub.GetComponent<LaboratoryHubPresenter>(), Is.Not.Null);
            Assert.That(hub.GetComponentInChildren<LaboratoryHubResponsiveLayout>(true), Is.Not.Null);
            Assert.That(hub.GetComponentInChildren<LaboratoryDishPreviewGraphic>(true), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "SelectedDish"), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "LabNotesPanel"), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "DishNavigation"), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "NotesDrawer"), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "ActiveDishesPanel"), Is.Null);
            Assert.That(FindNamed(hub.transform, "DishB"), Is.Null);
            Assert.That(FindNamed(hub.transform, "DishC"), Is.Null);
            Assert.That(PrefabUtility.IsPartOfPrefabInstance(FindNamed(hub.transform, "SelectedDish")), Is.True);
            Assert.That(PrefabUtility.IsPartOfPrefabInstance(FindNamed(hub.transform, "LatestDiscovery")), Is.True);
            Assert.That(CountMissingScripts(hub), Is.Zero);
        }

        [Test]
        public void SelectedDishUsesApprovedMockDataAndEnvironmentSummary()
        {
            GameObject hub = OpenHub();
            Assert.That(TextOf(hub, "DishName"), Is.EqualTo("Dish A"));
            Assert.That(TextOf(hub, "Organism"), Is.EqualTo("Bacillus subtilis"));
            Assert.That(TextOf(hub, "Medium"), Is.EqualTo("Nutrient Agar"));
            Assert.That(TextOf(hub, "DishNavigationState"), Does.Contain("1 / 1"));
            Assert.That(TextOf(FindNamed(hub.transform, "Temperature").gameObject, "Value"), Is.EqualTo("26°C"));
            Assert.That(TextOf(FindNamed(hub.transform, "Moisture").gameObject, "Value"), Is.EqualTo("42%"));
            Assert.That(TextOf(FindNamed(hub.transform, "Nutrients").gameObject, "Value"), Is.EqualTo("OK"));
        }

        [Test]
        public void SingleDishNavigationIsVisibleAndDisabledAtOneOfOne()
        {
            GameObject hub = OpenHub();
            Button previous = FindNamed(hub.transform, "PreviousDishButton").GetComponent<Button>();
            Button next = FindNamed(hub.transform, "NextDishButton").GetComponent<Button>();
            Assert.That(previous.interactable, Is.False);
            Assert.That(next.interactable, Is.False);
            Assert.That(TextOf(hub, "DishNavigationState"), Is.EqualTo("Dish A     1 / 1"));
        }

        [Test]
        public void NavigationOmitsDishesAndKeepsSettingsSeparated()
        {
            GameObject hub = OpenHub();
            Assert.That(FindNamed(hub.transform, "NavLabButton"), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "NavNewExperimentButton"), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "NavCompareButton"), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "NavJournalButton"), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "NavCollectionButton"), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "NavChallengesButton"), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "NavSettingsButton"), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "NavDishesButton"), Is.Null);
            Assert.That(FindNamed(hub.transform, "NavigationSpacer"), Is.Not.Null);
        }

        [Test]
        public void PrimaryAndSecondaryActionsHaveVisibleInteractionStates()
        {
            GameObject hub = OpenHub();
            string[] actionNames =
            {
                "NewExperimentButton", "CompareButton", "OpenDishButton",
                "HeaderJournalButton", "HeaderSettingsButton"
            };

            foreach (string actionName in actionNames)
            {
                Button button = FindNamed(hub.transform, actionName).GetComponent<Button>();
                Assert.That(button.interactable, Is.True, actionName);
                Assert.That(button.colors.highlightedColor, Is.Not.EqualTo(button.colors.normalColor), actionName);
                Assert.That(button.colors.pressedColor, Is.Not.EqualTo(button.colors.normalColor), actionName);
            }

            Button newExperiment = FindNamed(hub.transform, "NewExperimentButton").GetComponent<Button>();
            Assert.That(newExperiment.GetComponentInChildren<Text>(true).text,
                Is.EqualTo("+  START NEW EXPERIMENT"));
        }

        [Test]
        public void FeaturedDishSectionsAreContainedAndDoNotOverlapVertically()
        {
            GameObject hub = OpenHub();
            RectTransform dishName = FindNamed(hub.transform, "DishName").GetComponent<RectTransform>();
            RectTransform organism = FindNamed(hub.transform, "Organism").GetComponent<RectTransform>();
            RectTransform medium = FindNamed(hub.transform, "Medium").GetComponent<RectTransform>();
            RectTransform preview = FindNamed(hub.transform, "DishPreviewWell").GetComponent<RectTransform>();
            RectTransform metrics = FindNamed(hub.transform, "AgeMetric").GetComponent<RectTransform>();
            RectTransform environment = FindNamed(hub.transform, "EnvironmentSummary").GetComponent<RectTransform>();
            RectTransform open = FindNamed(hub.transform, "OpenDishButton").GetComponent<RectTransform>();
            RectTransform navigation = FindNamed(hub.transform, "DishNavigation").GetComponent<RectTransform>();

            Assert.That(organism.anchorMax.y, Is.LessThanOrEqualTo(dishName.anchorMin.y));
            Assert.That(medium.anchorMax.y, Is.LessThanOrEqualTo(organism.anchorMin.y));
            Assert.That(preview.anchorMax.y, Is.LessThan(medium.anchorMin.y));
            Assert.That(metrics.anchorMax.y, Is.LessThan(preview.anchorMin.y));
            Assert.That(environment.anchorMax.y, Is.LessThan(metrics.anchorMin.y));
            Assert.That(open.anchorMax.y, Is.LessThan(environment.anchorMin.y));
            Assert.That(navigation.anchorMax.y, Is.LessThan(open.anchorMin.y));
            Assert.That(preview.anchorMin.x, Is.GreaterThan(0f));
            Assert.That(preview.anchorMax.x, Is.LessThan(1f));
        }

        [UnityTest]        public IEnumerator LaboratoryHubEntersPlayModeAndPlaceholderActionsRespond()
        {
            EditorSceneManager.OpenScene(LaboratoryHubEditorBuilder.ScenePath, OpenSceneMode.Single);
            yield return new EnterPlayMode();

            LaboratoryHubPresenter presenter = Object.FindAnyObjectByType<LaboratoryHubPresenter>();
            Assert.That(presenter, Is.Not.Null);
            GameObject feedback = FindNamed(presenter.transform, "PlaceholderFeedback").gameObject;
            Assert.That(feedback.activeSelf, Is.False);

            presenter.ShowPlaceholder("New Experiment");
            yield return null;

            Assert.That(feedback.activeSelf, Is.True);
            Text message = feedback.GetComponentInChildren<Text>(true);
            Assert.That(message.text, Does.Contain("mock data"));
            yield return new ExitPlayMode();
        }

        private static GameObject OpenHub()
        {
            Scene scene = EditorSceneManager.OpenScene(LaboratoryHubEditorBuilder.ScenePath, OpenSceneMode.Single);
            return scene.GetRootGameObjects().Single(owner => owner.name == "LaboratoryHub");
        }

        private static string TextOf(GameObject root, string name) =>
            FindNamed(root.transform, name).GetComponent<Text>().text;

        private static Transform FindNamed(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform result = FindNamed(root.GetChild(i), name);
                if (result != null) return result;
            }
            return null;
        }

        private static int CountMissingScripts(GameObject root)
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root);
            foreach (Transform child in root.transform) count += CountMissingScripts(child.gameObject);
            return count;
        }
    }
}
