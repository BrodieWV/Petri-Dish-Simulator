using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PetriDish.Presentation.UI
{
    public sealed class LaboratoryHubPresenter : MonoBehaviour
    {
        [SerializeField] private Button[] placeholderButtons;
        [SerializeField] private Text feedbackText;
        [SerializeField] private GameObject feedbackPanel;
        [SerializeField, Min(0.25f)] private float feedbackDuration = 2f;
        private Coroutine hideRoutine;

        private void OnEnable()
        {
            if (placeholderButtons != null)
                foreach (Button button in placeholderButtons)
                    if (button != null) { button.onClick.RemoveListener(ShowFeedbackForButton); button.onClick.AddListener(ShowFeedbackForButton); }
            if (feedbackPanel != null) feedbackPanel.SetActive(false);
        }

        private void OnDisable()
        {
            if (placeholderButtons != null)
                foreach (Button button in placeholderButtons)
                    if (button != null) button.onClick.RemoveListener(ShowFeedbackForButton);
        }

        public void ShowFeedbackForButton()
        {
            GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            ShowPlaceholder(selected != null ? selected.name.Replace("Button", string.Empty) : "Laboratory action");
        }

        public void ShowPlaceholder(string action)
        {
            if (feedbackText != null) feedbackText.text = action + " is represented by mock data in this Phase 3 UI foundation.";
            if (feedbackPanel != null) feedbackPanel.SetActive(true);
            Debug.Log("[Laboratory Hub] " + action + " selected (placeholder).");
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
