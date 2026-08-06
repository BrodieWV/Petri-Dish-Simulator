using System.Collections;
using System.Reflection;
using PetriDish.Application;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PetriDish.Presentation
{
    /// <summary>
    /// Controls whether the legacy guided heater-fault scenario is active.
    /// Free-experiment mode is the default, so normal play never overwrites the
    /// player's selected target temperature.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GuidedScenarioMode : MonoBehaviour
    {
        [SerializeField] private bool guidedScenarioEnabled;

        private ExperimentController controller;
        private FieldInfo stageField;
        private FieldInfo stageStartSecondsField;

        public bool GuidedScenarioEnabled => guidedScenarioEnabled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallDefaultFreeExperimentMode()
        {
            if (FindFirstObjectByType<GuidedScenarioMode>() != null)
                return;

            GameObject host = new GameObject("GuidedScenarioMode");
            host.hideFlags = HideFlags.HideAndDontSave;
            host.AddComponent<GuidedScenarioMode>();
        }

        private IEnumerator Start()
        {
            for (int frame = 0; frame < 240; frame++)
            {
                controller = FindFirstObjectByType<ExperimentController>();
                if (controller != null && controller.Simulation != null)
                    break;

                yield return null;
            }

            if (controller == null)
            {
                Destroy(gameObject);
                yield break;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            stageField = typeof(ExperimentController).GetField("stage", flags);
            stageStartSecondsField = typeof(ExperimentController).GetField("stageStartSeconds", flags);

            if (!guidedScenarioEnabled)
                DisableGuidedScenario();
        }

        private void LateUpdate()
        {
            if (!guidedScenarioEnabled && controller != null)
                DisableGuidedScenario();
        }

        private void DisableGuidedScenario()
        {
            if (stageField == null || controller.Simulation == null)
                return;

            GuidedStage current = (GuidedStage)stageField.GetValue(controller);
            if (current == GuidedStage.Complete)
                return;

            stageField.SetValue(controller, GuidedStage.Complete);
            stageStartSecondsField?.SetValue(
                controller,
                controller.Simulation.ElapsedSimSeconds);
        }
    }
}
