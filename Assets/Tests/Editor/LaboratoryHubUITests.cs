using System.Collections;
using System.Linq;
using NUnit.Framework;
using PetriDish.Application;
using PetriDish.Content;
using PetriDish.Editor;
using PetriDish.Presentation;
using PetriDish.Presentation.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
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
        public void CompactNotesButtonTogglesDrawerThroughRuntimeBinding()
        {
            GameObject root = new GameObject("CompactHub", typeof(RectTransform));
            GameObject buttonOwner = new GameObject(
                "NotesDrawerButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonOwner.transform.SetParent(root.transform, false);
            GameObject drawer = new GameObject("NotesDrawer", typeof(RectTransform));
            drawer.transform.SetParent(root.transform, false);
            drawer.SetActive(false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(1136f, 640f);
            LaboratoryHubResponsiveLayout layout = root.AddComponent<LaboratoryHubResponsiveLayout>();
            layout.Configure(null, rootRect, null, null, null, null, null, null, buttonOwner, drawer);

            buttonOwner.GetComponent<Button>().onClick.Invoke();
            Assert.That(drawer.activeSelf, Is.True);
            buttonOwner.GetComponent<Button>().onClick.Invoke();
            Assert.That(drawer.activeSelf, Is.False);

            Object.DestroyImmediate(root);
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
                Assert.That(theme.navigationWidth, Is.GreaterThanOrEqualTo(220f));
                Assert.That(theme.notesWidth, Is.GreaterThanOrEqualTo(350f));
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
            Camera hubCamera = scene.GetRootGameObjects().Single(owner => owner.name == "LaboratoryHubCamera")
                .GetComponent<Camera>();
            Assert.That(hubCamera.enabled, Is.True);
            Assert.That(hubCamera.targetTexture, Is.Null);
            Assert.That(hubCamera.cullingMask, Is.Zero);
            Assert.That(hub.GetComponent<LaboratoryHubPresenter>(), Is.Not.Null);
            Assert.That(hub.GetComponent<PetriDishRuntimeScene>(), Is.Not.Null);
            Assert.That(hub.GetComponent<PetriDishRuntimeScene>().Role, Is.EqualTo(PetriDishSceneRole.NonExperiment));
            Assert.That(hub.GetComponentInChildren<LaboratoryHubResponsiveLayout>(true), Is.Not.Null);
            Assert.That(hub.GetComponentInChildren<LaboratoryDishPreviewGraphic>(true), Is.Null);
            Assert.That(hub.GetComponentsInChildren<RawImage>(true).Any(image => image.name == "DishDisplayImage"), Is.True);
            PetriDishDisplayPresenter display = scene.GetRootGameObjects()
                .Single(owner => owner.name == "SelectedDish3DDisplay")
                .GetComponent<PetriDishDisplayPresenter>();
            Assert.That(display, Is.Not.Null);
            Assert.That(PrefabUtility.IsPartOfPrefabInstance(display), Is.True);
            Assert.That(display.RotationPivot, Is.Not.Null);
            Assert.That(display.DisplayCamera.transform.IsChildOf(display.RotationPivot), Is.False);
            Assert.That(display.Output.name, Is.EqualTo("DishDisplayImage"));
            Assert.That(display.Output.raycastTarget, Is.True);
            Assert.That(FindNamed(hub.transform, "ResetDishViewButton"), Is.Not.Null);
            Assert.That(display.FramingScale, Is.EqualTo(1.35f).Within(0.001f));
            Assert.That(display.VerticalFramingOffset, Is.EqualTo(0.06f).Within(0.001f));
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
            string[] visibleText = hub.GetComponentsInChildren<Text>(true).Select(text => text.text).ToArray();
            Assert.That(visibleText.Any(value => value.Contains("No cameras rendering")), Is.False);
            Assert.That(visibleText.Any(value => value.Contains("Display 1")), Is.False);
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
        public void SharedDisplayReusesPhaseTwoModelMaterialsAndColonyPresenter()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LaboratoryHubEditorBuilder.DisplayPrefabPath);
            Assert.That(prefab, Is.Not.Null);
            Transform model = FindNamed(prefab.transform, "PetriDish3D");
            Assert.That(model, Is.Not.Null);
            Assert.That(PrefabUtility.GetCorrespondingObjectFromSource(model.gameObject), Is.Not.Null);

            Transform colony = FindNamed(model, "PetriDish_ColonySurface");
            ColonySurfacePresenter colonyPresenter = colony.GetComponent<ColonySurfacePresenter>();
            Assert.That(colonyPresenter, Is.Not.Null);
            Assert.That(colonyPresenter.StaticTexture, Is.Not.Null);
            Assert.That(colonyPresenter.TextureSource, Is.Null);
            Assert.That(colonyPresenter.TextureScale, Is.EqualTo(new Vector2(1.7f, 1.7f)));
            Assert.That(colonyPresenter.TextureOffset, Is.EqualTo(new Vector2(0.08f, 0.08f)));
            Assert.That(AssetDatabase.GetAssetPath(colony.GetComponent<MeshRenderer>().sharedMaterial),
                Is.EqualTo("Assets/PetriDish/Art/models/PetriDish.fbx"));
        }

        [Test]
        public void SingleDishNavigationIsVisibleAndDisabledAtOneOfOne()
        {
            GameObject hub = OpenHub();
            Button previous = FindNamed(hub.transform, "PreviousDishButton").GetComponent<Button>();
            Button next = FindNamed(hub.transform, "NextDishButton").GetComponent<Button>();
            Assert.That(previous.interactable, Is.False);
            Assert.That(next.interactable, Is.False);
            Assert.That(previous.colors.disabledColor.a, Is.GreaterThanOrEqualTo(0.7f));
            Assert.That(next.colors.disabledColor.a, Is.GreaterThanOrEqualTo(0.7f));
            Assert.That(previous.GetComponentInChildren<Text>(true).fontSize, Is.GreaterThanOrEqualTo(19));
            Assert.That(next.GetComponentInChildren<Text>(true).fontSize, Is.GreaterThanOrEqualTo(19));
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
            ScrollRect scroll = FindNamed(hub.transform, "NavigationRail").GetComponent<ScrollRect>();
            Assert.That(scroll, Is.Not.Null);
            Assert.That(scroll.vertical, Is.True);
            Assert.That(scroll.viewport.GetComponent<RectMask2D>(), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "NavSettingsButton").IsChildOf(scroll.content), Is.True);
        }

        [Test]
        public void PrimaryAndSecondaryActionsHaveVisibleInteractionStates()
        {
            GameObject hub = OpenHub();
            string[] actionNames =
            {
                "NewExperimentButton", "CompareButton", "OpenDishButton"
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
        public void FeaturedDishUsesLargeUnboxedPreviewAndCohesiveSummary()
        {
            GameObject hub = OpenHub();
            RectTransform preview = FindNamed(hub.transform, "DishPreviewWell").GetComponent<RectTransform>();
            RectTransform summary = FindNamed(hub.transform, "CultureSummary").GetComponent<RectTransform>();
            RectTransform metrics = FindNamed(hub.transform, "AgeMetric").GetComponent<RectTransform>();
            RectTransform environment = FindNamed(hub.transform, "EnvironmentSummary").GetComponent<RectTransform>();
            RectTransform open = FindNamed(hub.transform, "OpenDishButton").GetComponent<RectTransform>();
            RectTransform navigation = FindNamed(hub.transform, "DishNavigation").GetComponent<RectTransform>();

            Assert.That(preview.GetComponent<Image>(), Is.Null);
            Assert.That(preview.anchorMax.x - preview.anchorMin.x, Is.GreaterThanOrEqualTo(0.59f));
            Assert.That(preview.anchorMax.y - preview.anchorMin.y, Is.GreaterThanOrEqualTo(0.57f));
            Assert.That(preview.anchorMax.x, Is.LessThan(summary.anchorMin.x));
            Assert.That(summary.GetComponent<Image>(), Is.Not.Null);
            Assert.That(metrics.GetComponent<Image>(), Is.Null);
            Assert.That(environment.GetComponent<Image>(), Is.Null);
            Assert.That(open.anchorMin.x, Is.GreaterThan(summary.anchorMin.x));
            Assert.That(navigation.anchorMax.y, Is.LessThanOrEqualTo(preview.anchorMin.y));
        }

        [Test]
        public void NavigationHeaderNotesAndActionsUseRefinedVisualHierarchy()
        {
            GameObject hub = OpenHub();
            Assert.That(TextOf(hub, "Title"), Is.EqualTo("PETRI LAB"));
            Assert.That(FindNamed(hub.transform, "Title").GetComponent<Text>().fontSize, Is.EqualTo(44));
            Assert.That(FindNamed(hub.transform, "Subtitle"), Is.Null);
            Assert.That(FindNamed(hub.transform, "ActionDock"), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "ActionPrompt"), Is.Null);
            Assert.That(FindNamed(hub.transform, "ActionSpacer"), Is.Not.Null);
            Assert.That(FindNamed(hub.transform, "HeaderSettingsButton"), Is.Null);

            Transform navigation = FindNamed(hub.transform, "NavigationRail");
            Text[] labels = navigation.GetComponentsInChildren<Text>(true)
                .Where(text => text.name == "Label").ToArray();
            Assert.That(labels, Has.Length.EqualTo(7));
            Assert.That(labels.All(label => label.fontSize >= 23), Is.True);
            Assert.That(FindNamed(navigation, "SelectedEdge").GetComponent<RectTransform>().anchorMax.x,
                Is.LessThanOrEqualTo(0.012f));

            Transform observation = FindNamed(hub.transform, "CurrentObservation");
            Assert.That(observation.GetComponent<Outline>(), Is.Null);
            Assert.That(observation.GetComponent<Shadow>(), Is.Null);
            Assert.That(FindNamed(observation, "AccentLine"), Is.Not.Null);
            Assert.That(FindNamed(observation, "Title").GetComponent<Text>().fontSize, Is.GreaterThanOrEqualTo(19));
            Assert.That(FindNamed(hub.transform, "HeaderJournalButton"), Is.Null);
            LayoutElement observationLayout = observation.GetComponent<LayoutElement>();
            LayoutElement discoveryLayout = FindNamed(hub.transform, "LatestDiscovery").GetComponent<LayoutElement>();
            Assert.That(observationLayout.preferredHeight, Is.GreaterThan(discoveryLayout.preferredHeight));
        }

        [Test]
        public void FeaturedDishUsesCompactHealthyStatusAndScaledTypography()
        {
            GameObject hub = OpenHub();
            Transform selectedDish = FindNamed(hub.transform, "SelectedDish");
            Transform status = FindNamed(selectedDish, "StatusBadge");
            Assert.That(status.parent, Is.EqualTo(selectedDish));
            Assert.That(TextOf(status.gameObject, "Label"), Is.EqualTo("\u25CF  Growing well"));
            Assert.That(FindNamed(selectedDish, "LiveStatusStrip"), Is.Null);
            Assert.That(FindNamed(selectedDish, "DishName").GetComponent<Text>().fontSize, Is.EqualTo(50));
            Assert.That(FindNamed(selectedDish, "Organism").GetComponent<Text>().fontSize, Is.EqualTo(30));
            Assert.That(FindNamed(selectedDish, "Medium").GetComponent<Text>().fontSize, Is.EqualTo(22));
            Assert.That(FindNamed(status, "Label").GetComponent<Text>().fontSize, Is.EqualTo(15));
        }

        [UnityTest]
        public IEnumerator LaboratoryHubEntersPlayModeAndPlaceholderActionsRespond()
        {
            EditorSceneManager.OpenScene(LaboratoryHubEditorBuilder.ScenePath, OpenSceneMode.Single);
            yield return new EnterPlayMode();

            LaboratoryHubPresenter presenter = Object.FindAnyObjectByType<LaboratoryHubPresenter>();
            Assert.That(presenter, Is.Not.Null);
            RuntimeBootstrap runtime = Object.FindAnyObjectByType<RuntimeBootstrap>();
            Assert.That(runtime, Is.Not.Null);
            Assert.That(runtime.LegacyRuntimeUiGenerated, Is.False);
            Assert.That(runtime.GetComponentInChildren<Canvas>(true), Is.Null);
            Assert.That(Object.FindAnyObjectByType<DishViewportPresenter>(), Is.Null);
            Assert.That(Object.FindAnyObjectByType<Slider>(), Is.Null);
            PetriDishDisplayPresenter display = Object.FindAnyObjectByType<PetriDishDisplayPresenter>();
            Assert.That(display, Is.Not.Null);
            yield return null;
            Assert.That(display.ActiveRenderTexture, Is.Not.Null);
            Assert.That(display.ActiveRenderTexture.IsCreated(), Is.True);
            Assert.That(display.DisplayCamera.targetTexture, Is.SameAs(display.ActiveRenderTexture));
            Assert.That(display.Output.texture, Is.SameAs(display.ActiveRenderTexture));
            Assert.That(display.Output.raycastTarget, Is.True);
            AssertDisplayRenderersInsideViewport(display);
            Button resetView = FindNamed(presenter.transform, "ResetDishViewButton").GetComponent<Button>();
            Assert.That(resetView, Is.Not.Null);
            display.OrbitBy(new Vector2(120f, -60f));
            Assert.That(display.YawDegrees, Is.Not.Zero);
            resetView.onClick.Invoke();
            Assert.That(display.YawDegrees, Is.Zero.Within(0.001f));
            Assert.That(display.PitchDegrees, Is.Zero.Within(0.001f));
            GameObject feedback = FindNamed(presenter.transform, "PlaceholderFeedback").gameObject;
            Assert.That(feedback.activeSelf, Is.False);

            presenter.ShowPlaceholder("New Experiment");
            yield return null;

            Assert.That(feedback.activeSelf, Is.True);
            Text message = feedback.GetComponentInChildren<Text>(true);
            Assert.That(message.text, Does.Contain("mock data"));
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator LegacyExperimentPresentationDoesNotLeakWhenLoadingLaboratoryHub()
        {
            EditorSceneManager.OpenScene("Assets/PetriDish/Scenes/PetriDishVerticalSlice.unity", OpenSceneMode.Single);
            yield return new EnterPlayMode();
            yield return null;

            RuntimeBootstrap runtime = Object.FindAnyObjectByType<RuntimeBootstrap>();
            Assert.That(runtime, Is.Not.Null);
            Assert.That(runtime.ColonyTextureSource, Is.Not.Null);
            Assert.That(
                runtime.LegacyRuntimeUiGenerated ||
                Object.FindAnyObjectByType<PetriDishResponsiveUIBinder>() != null,
                Is.True);
            Assert.That(Object.FindAnyObjectByType<DishViewportPresenter>(), Is.Not.Null);

            SceneManager.LoadScene("LaboratoryHub");
            yield return null;
            yield return null;

            Assert.That(Object.FindAnyObjectByType<RuntimeBootstrap>(), Is.SameAs(runtime));
            Assert.That(runtime.LegacyRuntimeUiGenerated, Is.False);
            Assert.That(runtime.ColonyTextureSource, Is.Null);
            Assert.That(runtime.GetComponentInChildren<Canvas>(true), Is.Null);
            Assert.That(Object.FindAnyObjectByType<DishViewportPresenter>(), Is.Null);
            Assert.That(Object.FindAnyObjectByType<PetriDishDisplayPresenter>(), Is.Not.Null);
            Assert.That(Object.FindAnyObjectByType<LaboratoryHubPresenter>(), Is.Not.Null);
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator HubExperimentEntriesAndReturnPreserveAuthoritativeRuntimeState()
        {
            EditorSceneManager.OpenScene(LaboratoryHubEditorBuilder.ScenePath, OpenSceneMode.Single);
            yield return new EnterPlayMode();
            yield return null;

            RuntimeBootstrap runtime = Object.FindAnyObjectByType<RuntimeBootstrap>();
            ExperimentController controller = runtime.GetComponent<ExperimentController>();
            LaboratoryHubPresenter hub = Object.FindAnyObjectByType<LaboratoryHubPresenter>();
            object initialSimulation = controller.Simulation;
            int initialSeed = controller.Simulation.Seed;

            hub.Execute(LaboratoryHubAction.NewExperiment);
            yield return null;
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name,
                Is.EqualTo(UnityLaboratoryHubNavigator.ExperimentSceneName));
            Assert.That(Object.FindAnyObjectByType<RuntimeBootstrap>(), Is.SameAs(runtime));
            Assert.That(controller.Simulation, Is.SameAs(initialSimulation),
                "Opening setup must not restart the current experiment before confirmation.");
            Assert.That(controller.Simulation.Seed, Is.EqualTo(initialSeed));
            Assert.That(LaboratoryHubExperimentEntry.HasPendingRequest, Is.False);
            PetriDishResponsiveUIBinder binder = Object.FindAnyObjectByType<PetriDishResponsiveUIBinder>();
            Assert.That(binder, Is.Not.Null);
            Text inspection = FindNamed(binder.transform, "InspectionText").GetComponent<Text>();
            Assert.That(inspection.text, Does.Contain("Choose an organism and medium"));
            Button lab = FindNamed(binder.transform, "LabHubButton").GetComponent<Button>();
            Assert.That(lab.GetComponentInChildren<Text>(true).text, Is.EqualTo("Lab"));
            controller.StartNew(
                987654,
                SimulationDefinitionCatalog.SaccharomycesCerevisiaeId,
                SimulationDefinitionCatalog.LowNutrientAgarId);

            lab.onClick.Invoke();
            yield return null;
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name,
                Is.EqualTo(UnityLaboratoryHubNavigator.HubSceneName));
            Assert.That(Object.FindAnyObjectByType<RuntimeBootstrap>(), Is.SameAs(runtime));
            Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            hub = Object.FindAnyObjectByType<LaboratoryHubPresenter>();
            Assert.That(FindNamed(hub.transform, "Organism").GetComponent<Text>().text,
                Is.EqualTo("Saccharomyces cerevisiae"));
            Assert.That(FindNamed(hub.transform, "Medium").GetComponent<Text>().text,
                Is.EqualTo("Low-Nutrient Agar"));

            PetriDishDisplayPresenter previousHubDisplay = Object.FindAnyObjectByType<PetriDishDisplayPresenter>();
            RenderTexture previousHubTexture = previousHubDisplay.ActiveRenderTexture;
            object selectedSimulation = controller.Simulation;
            long selectedTick = controller.Simulation.Tick;
            hub.Execute(LaboratoryHubAction.OpenDish);
            yield return null;
            yield return null;

            Assert.That(controller.Simulation, Is.SameAs(selectedSimulation));
            Assert.That(controller.Simulation.Seed, Is.EqualTo(987654));
            Assert.That(controller.Simulation.Tick, Is.EqualTo(selectedTick));
            Assert.That(LaboratoryHubExperimentEntry.HasPendingRequest, Is.False);
            Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(previousHubTexture == null, Is.True,
                "The previous Hub RenderTexture must be released when its display is destroyed.");

            binder = Object.FindAnyObjectByType<PetriDishResponsiveUIBinder>();
            lab = FindNamed(binder.transform, "LabHubButton").GetComponent<Button>();
            lab.onClick.Invoke();
            yield return null;
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name,
                Is.EqualTo(UnityLaboratoryHubNavigator.HubSceneName));
            Assert.That(Object.FindObjectsByType<RuntimeBootstrap>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<PetriDishDisplayPresenter>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindAnyObjectByType<PetriDishResponsiveUIBinder>(), Is.Null,
                "Experiment presentation must not remain over the Laboratory Hub.");
            PetriDishDisplayPresenter returnedDisplay = Object.FindAnyObjectByType<PetriDishDisplayPresenter>();
            Assert.That(returnedDisplay.DisplayCamera, Is.Not.Null);
            Assert.That(returnedDisplay.ActiveRenderTexture, Is.Not.Null);
            Assert.That(returnedDisplay.ActiveRenderTexture.IsCreated(), Is.True);
            Assert.That(FindNamed(Object.FindAnyObjectByType<LaboratoryHubPresenter>().transform, "Organism")
                .GetComponent<Text>().text, Is.EqualTo("Saccharomyces cerevisiae"));
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

        private static void AssertDisplayRenderersInsideViewport(PetriDishDisplayPresenter display)
        {
            Renderer[] renderers = display.RotationPivot.GetComponentsInChildren<Renderer>(true);
            Assert.That(renderers, Is.Not.Empty);
            foreach (Renderer renderer in renderers)
            {
                Bounds bounds = renderer.bounds;
                for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 corner = bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z));
                    Vector3 viewport = display.DisplayCamera.WorldToViewportPoint(corner);
                    Assert.That(viewport.z, Is.GreaterThan(0f), renderer.name);
                    Assert.That(viewport.x, Is.InRange(0f, 1f), renderer.name);
                    Assert.That(viewport.y, Is.InRange(0f, 1f), renderer.name);
                }
            }
        }
    }
}
