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
        public void ThemeMapsStatusesToAccessibleSignalColours()
        {
            PetriDishUITheme theme = ScriptableObject.CreateInstance<PetriDishUITheme>();
            try
            {
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
        public void BuilderIsIdempotentAndSceneIsFullySerialized()
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
            Assert.That(hub.GetComponentInChildren<AdaptiveDishCardLayoutGroup>(true), Is.Not.Null);
            Assert.That(hub.GetComponentInChildren<LaboratoryDishPreviewGraphic>(true), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "DishA"), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "DishB"), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "DishC"), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "ActivityDrawer"), Is.Not.Null);
            Assert.That(PrefabUtility.IsPartOfPrefabInstance(FindNamed(hub.transform, "DishA")), Is.True);
            Assert.That(PrefabUtility.IsPartOfPrefabInstance(FindNamed(hub.transform, "DishB")), Is.True);
            Assert.That(PrefabUtility.IsPartOfPrefabInstance(FindNamed(hub.transform, "DishC")), Is.True);
            Assert.That(PrefabUtility.IsPartOfPrefabInstance(FindNamed(hub.transform, "LatestDiscovery")), Is.True);
            Assert.That(CountMissingScripts(hub), Is.Zero);
        }

        [Test]
        public void PrimaryAndSecondaryActionsHaveVisibleInteractionStates()
        {
            Scene scene = EditorSceneManager.OpenScene(LaboratoryHubEditorBuilder.ScenePath, OpenSceneMode.Single);
            GameObject hub = scene.GetRootGameObjects().Single(owner => owner.name == "LaboratoryHub");
            string[] actionNames = { "NewExperimentButton", "ActiveDishesButton", "CompareButton", "JournalButton", "OpenDishButton" };

            foreach (string actionName in actionNames)
            {
                Button button = FindNamed(hub.transform, actionName).GetComponent<Button>();
                Assert.That(button.interactable, Is.True, actionName);
                Assert.That(button.colors.highlightedColor, Is.Not.EqualTo(button.colors.normalColor), actionName);
                Assert.That(button.colors.pressedColor, Is.Not.EqualTo(button.colors.normalColor), actionName);
            }
        }

        [UnityTest]
        public IEnumerator LaboratoryHubEntersPlayModeAndPlaceholderActionsRespond()
        {
            EditorSceneManager.OpenScene(LaboratoryHubEditorBuilder.ScenePath, OpenSceneMode.Single);
            yield return new EnterPlayMode();

            LaboratoryHubPresenter presenter = Object.FindAnyObjectByType<LaboratoryHubPresenter>();
            GameObject feedback = FindNamed(presenter.transform, "PlaceholderFeedback").gameObject;
            Assert.That(presenter, Is.Not.Null);
            Assert.That(feedback, Is.Not.Null);
            Assert.That(feedback.activeSelf, Is.False);

            presenter.ShowPlaceholder("New Experiment");
            yield return null;

            Assert.That(feedback.activeSelf, Is.True);
            Text message = feedback.GetComponentInChildren<Text>(true);
            Assert.That(message.text, Does.Contain("mock data"));
            yield return new ExitPlayMode();
        }

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
