using System.Collections.Generic;
using NUnit.Framework;
using PetriDish.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PetriDish.Tests.Editor
{
    public sealed class LaboratoryHubNavigationTests
    {
        private readonly List<GameObject> owners = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            LaboratoryHubExperimentEntry.Clear();
            foreach (GameObject owner in owners)
                if (owner != null) Object.DestroyImmediate(owner);
            owners.Clear();
        }

        [Test]
        public void DuplicateNavigationButtonsResolveToTheSameAction()
        {
            RecordingNavigator navigator = new RecordingNavigator();
            LaboratoryHubPresenter presenter = BuildPresenter(navigator);

            Find<Button>(presenter.transform, "NavNewExperimentButton").onClick.Invoke();
            Find<Button>(presenter.transform, "NewExperimentButton").onClick.Invoke();
            Find<Button>(presenter.transform, "NavCompareButton").onClick.Invoke();
            Find<Button>(presenter.transform, "CompareButton").onClick.Invoke();

            Assert.That(navigator.Actions, Is.EqualTo(new[]
            {
                LaboratoryHubAction.NewExperiment,
                LaboratoryHubAction.NewExperiment,
                LaboratoryHubAction.Compare,
                LaboratoryHubAction.Compare
            }));
        }

        [Test]
        public void AllVisibleDestinationButtonsHaveIntentionalActions()
        {
            RecordingNavigator navigator = new RecordingNavigator();
            LaboratoryHubPresenter presenter = BuildPresenter(navigator);

            string[] names =
            {
                "NavLabButton", "NavNewExperimentButton", "OpenDishButton", "NavCompareButton",
                "NavJournalButton", "NavCollectionButton", "NavChallengesButton", "NavSettingsButton"
            };
            foreach (string name in names) Find<Button>(presenter.transform, name).onClick.Invoke();

            Assert.That(navigator.Actions, Is.EqualTo(new[]
            {
                LaboratoryHubAction.Lab, LaboratoryHubAction.NewExperiment, LaboratoryHubAction.OpenDish,
                LaboratoryHubAction.Compare, LaboratoryHubAction.Journal, LaboratoryHubAction.Collection,
                LaboratoryHubAction.Challenges, LaboratoryHubAction.Settings
            }));
        }

        [Test]
        public void SingleDishSelectionIsTruthfulAndCannotMove()
        {
            LaboratoryHubDishSelection selection = new LaboratoryHubDishSelection(
                new SingleLaboratoryDishProvider());

            Assert.That(selection.Count, Is.EqualTo(1));
            Assert.That(selection.PositionLabel, Is.EqualTo("Dish A     1 / 1"));
            Assert.That(selection.CanSelectPrevious, Is.False);
            Assert.That(selection.CanSelectNext, Is.False);
            Assert.That(selection.SelectPrevious(), Is.False);
            Assert.That(selection.SelectNext(), Is.False);
            Assert.That(selection.SelectedDish.Id, Is.EqualTo("current-experiment"));
        }

        [Test]
        public void ReplaceableProviderCyclesRealEntriesWithoutInventingDishes()
        {
            LaboratoryHubDishSelection selection = new LaboratoryHubDishSelection(
                new ListDishProvider(
                    new LaboratoryDishViewData("one", "Dish One", "Organism One", "Medium One"),
                    new LaboratoryDishViewData("two", "Dish Two", "Organism Two", "Medium Two")));

            Assert.That(selection.SelectNext(), Is.True);
            Assert.That(selection.SelectedDish.Id, Is.EqualTo("two"));
            Assert.That(selection.PositionLabel, Is.EqualTo("Dish Two     2 / 2"));
            Assert.That(selection.CanSelectNext, Is.False);
            Assert.That(selection.SelectPrevious(), Is.True);
            Assert.That(selection.SelectedDish.Id, Is.EqualTo("one"));
        }

        [Test]
        public void NewAndOpenDishRequestsCarryDistinctPresentationIntent()
        {
            LaboratoryHubExperimentEntry.RequestNewExperiment();
            Assert.That(LaboratoryHubExperimentEntry.TryConsume(out ExperimentEntryRequest setup), Is.True);
            Assert.That(setup.Intent, Is.EqualTo(ExperimentEntryIntent.NewExperimentSetup));
            Assert.That(setup.DishId, Is.Null);

            LaboratoryHubExperimentEntry.RequestOpenDish("current-experiment");
            Assert.That(LaboratoryHubExperimentEntry.TryConsume(out ExperimentEntryRequest open), Is.True);
            Assert.That(open.Intent, Is.EqualTo(ExperimentEntryIntent.OpenSelectedDish));
            Assert.That(open.DishId, Is.EqualTo("current-experiment"));
            Assert.That(LaboratoryHubExperimentEntry.HasPendingRequest, Is.False);
        }

        [Test]
        public void DeferredDestinationsReturnExplicitUnavailableMessages()
        {
            UnityLaboratoryHubNavigator navigator = new UnityLaboratoryHubNavigator();
            LaboratoryHubNavigationContext context = new LaboratoryHubNavigationContext(
                new SingleLaboratoryDishProvider().GetDish(0), 1);

            Assert.That(navigator.Navigate(LaboratoryHubAction.Compare, context).Message,
                Is.EqualTo("Comparison requires at least two dishes."));
            Assert.That(navigator.Navigate(LaboratoryHubAction.Journal, context).Message, Does.Contain("later milestone"));
            Assert.That(navigator.Navigate(LaboratoryHubAction.Collection, context).Message, Does.Contain("later milestone"));
            Assert.That(navigator.Navigate(LaboratoryHubAction.Challenges, context).Message, Does.Contain("later milestone"));
            Assert.That(navigator.Navigate(LaboratoryHubAction.Settings, context).Message, Does.Contain("not available"));
        }

        [Test]
        public void PresenterDisablesSingleDishArrowsAndRefreshesSelectionText()
        {
            LaboratoryHubPresenter presenter = BuildPresenter(new RecordingNavigator());

            Assert.That(Find<Button>(presenter.transform, "PreviousDishButton").interactable, Is.False);
            Assert.That(Find<Button>(presenter.transform, "NextDishButton").interactable, Is.False);
            Assert.That(Find<Text>(presenter.transform, "DishNavigationState").text, Is.EqualTo("Dish A     1 / 1"));
            Assert.That(Find<Text>(presenter.transform, "DishName").text, Is.EqualTo("Dish A"));
            Assert.That(Find<Text>(presenter.transform, "Organism").text, Is.EqualTo("Bacillus subtilis"));
            Assert.That(Find<Text>(presenter.transform, "Medium").text, Is.EqualTo("Nutrient Agar"));
        }

        private LaboratoryHubPresenter BuildPresenter(ILaboratoryHubNavigator navigator)
        {
            GameObject root = new GameObject("LaboratoryHub");
            owners.Add(root);
            root.SetActive(false);
            string[] buttonNames =
            {
                "NavLabButton", "NavNewExperimentButton", "NewExperimentButton", "OpenDishButton",
                "NavCompareButton", "CompareButton", "NavJournalButton", "NavCollectionButton",
                "NavChallengesButton", "NavSettingsButton", "PreviousDishButton", "NextDishButton"
            };
            foreach (string name in buttonNames)
                new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)).transform.SetParent(root.transform);
            foreach (string name in new[] { "DishNavigationState", "DishName", "Organism", "Medium" })
                new GameObject(name, typeof(RectTransform), typeof(Text)).transform.SetParent(root.transform);

            LaboratoryHubPresenter presenter = root.AddComponent<LaboratoryHubPresenter>();
            presenter.ConfigureNavigation(navigator);
            root.SetActive(true);
            return presenter;
        }

        private static T Find<T>(Transform root, string name) where T : Component
        {
            foreach (T component in root.GetComponentsInChildren<T>(true))
                if (component.name == name) return component;
            return null;
        }

        private sealed class RecordingNavigator : ILaboratoryHubNavigator
        {
            public readonly List<LaboratoryHubAction> Actions = new List<LaboratoryHubAction>();

            public LaboratoryHubNavigationResult Navigate(
                LaboratoryHubAction action,
                LaboratoryHubNavigationContext context)
            {
                Actions.Add(action);
                return LaboratoryHubNavigationResult.Success();
            }
        }

        private sealed class ListDishProvider : ILaboratoryDishProvider
        {
            private readonly LaboratoryDishViewData[] dishes;
            public int Count => dishes.Length;

            public ListDishProvider(params LaboratoryDishViewData[] dishes) => this.dishes = dishes;
            public LaboratoryDishViewData GetDish(int index) => dishes[index];
        }
    }
}
