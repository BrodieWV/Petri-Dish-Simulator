using UnityEngine;

namespace PetriDish.Presentation
{
    public enum PetriDishSceneRole
    {
        Experiment,
        NonExperiment
    }

    /// <summary>
    /// Explicitly declares whether the scene owns experiment runtime presentation.
    /// The persistent simulation controller may exist in either role.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PetriDishRuntimeScene : MonoBehaviour
    {
        [SerializeField] private PetriDishSceneRole role = PetriDishSceneRole.Experiment;

        public PetriDishSceneRole Role => role;

        public void Configure(PetriDishSceneRole sceneRole) => role = sceneRole;
    }
}
