using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PetriDish.Presentation.UI
{
    public sealed class LaboratoryHubPresenter : MonoBehaviour
    {
        [SerializeField] private Button[] placeholderButtons;
        [SerializeField] private Text feedbackText;
        [SerializeField] private GameObject feedbackPanel;
        [SerializeField, Min(0.25f)] private float feedbackDuration = 2f;

        private readonly Dictionary<Button, UnityAction> actionListeners =
            new Dictionary<Button, UnityAction>();
        private Coroutine hideRoutine;
        private ILaboratoryHubNavigator navigator;
        private LaboratoryHubDishSelection selection;
        private Button previousDishButton;
        private Button nextDishButton;
        private Text dishNavigationState;
        private Text dishName;
        private Text organismName;
        private Text mediumName;

        public LaboratoryHubDishSelection Selection => selection;

        private void OnEnable()
        {
            if (navigator == null) navigator = new UnityLaboratoryHubNavigator();
            if (selection == null) selection = new LaboratoryHubDishSelection(new SingleLaboratoryDishProvider());
            ResolveSelectionControls();
            BindActions();
            RefreshSelection();
            if (feedbackPanel != null) feedbackPanel.SetActive(false);
        }

        private void OnDisable()
        {
            UnbindActions();
            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
                hideRoutine = null;
            }
        }

        public void ConfigureNavigation(ILaboratoryHubNavigator navigation)
        {
            navigator = navigation ?? throw new ArgumentNullException(nameof(navigation));
        }

        public void ConfigureDishProvider(ILaboratoryDishProvider provider)
        {
            selection = new LaboratoryHubDishSelection(provider);
            if (isActiveAndEnabled) RefreshSelection();
        }

        public void Execute(LaboratoryHubAction action)
        {
            LaboratoryHubNavigationResult result = navigator.Navigate(
                action,
                new LaboratoryHubNavigationContext(selection.SelectedDish, selection.Count));
            if (!string.IsNullOrWhiteSpace(result.Message)) ShowFeedback(result.Message);
        }

        public void SelectPreviousDish()
        {
            selection.SelectPrevious();
            RefreshSelection();
        }

        public void SelectNextDish()
        {
            selection.SelectNext();
            RefreshSelection();
        }

        // Compatibility entry point for any existing serialized events and the Phase 2 UI test.
        public void ShowFeedbackForButton()
        {
            GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (selected != null && TryGetAction(selected.name, out LaboratoryHubAction action))
            {
                Execute(action);
                return;
            }
            ShowPlaceholder(selected != null ? selected.name.Replace("Button", string.Empty) : "Laboratory action");
        }

        public void ShowPlaceholder(string action)
        {
            ShowFeedback(action + " is represented by mock data in this Phase 3 UI foundation.");
            Debug.Log("[Laboratory Hub] " + action + " selected (placeholder).");
        }

        private void BindActions()
        {
            UnbindActions();
            foreach (Button button in GetComponentsInChildren<Button>(true))
            {
                if (!TryGetAction(button.name, out LaboratoryHubAction action)) continue;
                LaboratoryHubAction captured = action;
                UnityAction listener = () => Execute(captured);
                button.onClick.AddListener(listener);
                actionListeners.Add(button, listener);
            }

            if (previousDishButton != null) previousDishButton.onClick.AddListener(SelectPreviousDish);
            if (nextDishButton != null) nextDishButton.onClick.AddListener(SelectNextDish);
        }

        private void UnbindActions()
        {
            foreach (KeyValuePair<Button, UnityAction> pair in actionListeners)
                if (pair.Key != null) pair.Key.onClick.RemoveListener(pair.Value);
            actionListeners.Clear();
            if (previousDishButton != null) previousDishButton.onClick.RemoveListener(SelectPreviousDish);
            if (nextDishButton != null) nextDishButton.onClick.RemoveListener(SelectNextDish);
        }

        private void ResolveSelectionControls()
        {
            previousDishButton = FindNamed<Button>("PreviousDishButton");
            nextDishButton = FindNamed<Button>("NextDishButton");
            dishNavigationState = FindNamed<Text>("DishNavigationState");
            dishName = FindNamed<Text>("DishName");
            organismName = FindNamed<Text>("Organism");
            mediumName = FindNamed<Text>("Medium");
        }

        private void RefreshSelection()
        {
            LaboratoryDishViewData selected = selection.SelectedDish;
            if (previousDishButton != null) previousDishButton.interactable = selection.CanSelectPrevious;
            if (nextDishButton != null) nextDishButton.interactable = selection.CanSelectNext;
            if (dishNavigationState != null) dishNavigationState.text = selection.PositionLabel;
            if (dishName != null) dishName.text = selected.Name;
            if (organismName != null) organismName.text = selected.OrganismName;
            if (mediumName != null) mediumName.text = selected.MediumName;
        }

        private T FindNamed<T>(string objectName) where T : Component
        {
            foreach (T component in GetComponentsInChildren<T>(true))
                if (component.name == objectName) return component;
            return null;
        }

        private static bool TryGetAction(string buttonName, out LaboratoryHubAction action)
        {
            switch (buttonName)
            {
                case "NavLabButton": action = LaboratoryHubAction.Lab; return true;
                case "NavNewExperimentButton":
                case "NewExperimentButton": action = LaboratoryHubAction.NewExperiment; return true;
                case "OpenDishButton": action = LaboratoryHubAction.OpenDish; return true;
                case "NavCompareButton":
                case "CompareButton": action = LaboratoryHubAction.Compare; return true;
                case "NavJournalButton": action = LaboratoryHubAction.Journal; return true;
                case "NavCollectionButton": action = LaboratoryHubAction.Collection; return true;
                case "NavChallengesButton": action = LaboratoryHubAction.Challenges; return true;
                case "NavSettingsButton": action = LaboratoryHubAction.Settings; return true;
                default: action = default; return false;
            }
        }

        private void ShowFeedback(string message)
        {
            if (feedbackText != null) feedbackText.text = message;
            if (feedbackPanel != null) feedbackPanel.SetActive(true);
            if (!isActiveAndEnabled) return;
            if (hideRoutine != null) StopCoroutine(hideRoutine);
            hideRoutine = StartCoroutine(HideFeedback());
        }

        private IEnumerator HideFeedback()
        {
            yield return new WaitForSecondsRealtime(feedbackDuration);
            if (feedbackPanel != null) feedbackPanel.SetActive(false);
            hideRoutine = null;
        }
    }
}
