using UnityEngine;
using UnityEngine.Serialization;

namespace PetriDish.Presentation.UI
{
    public enum LaboratoryDishStatus { Growing, Healthy, Stressed, Paused, Severe }

    [CreateAssetMenu(fileName = "PetriDishUITheme", menuName = "Petri Dish/UI Theme")]
    public sealed class PetriDishUITheme : ScriptableObject
    {
        [Header("Laboratory surfaces")]
        public Color background = new Color(0.957f, 0.945f, 0.918f, 1f);
        public Color bench = new Color(0.914f, 0.937f, 0.941f, 1f);
        public Color panel = new Color(0.995f, 0.995f, 0.988f, 1f);
        public Color panelRaised = new Color(0.965f, 0.976f, 0.976f, 1f);
        public Color panelHover = new Color(0.892f, 0.949f, 0.949f, 1f);
        public Color border = new Color(0.820f, 0.856f, 0.860f, 1f);
        public Color shadow = new Color(0.12f, 0.18f, 0.20f, 0.09f);

        [Header("Signals")]
        public Color cyan = new Color(0.075f, 0.522f, 0.565f, 1f);
        public Color green = new Color(0.220f, 0.596f, 0.380f, 1f);
        public Color amber = new Color(0.765f, 0.470f, 0.145f, 1f);
        public Color red = new Color(0.745f, 0.235f, 0.235f, 1f);

        [Header("Typography")]
        public Color textPrimary = new Color(0.105f, 0.145f, 0.155f, 1f);
        public Color textSecondary = new Color(0.310f, 0.380f, 0.400f, 1f);
        public Color textDisabled = new Color(0.570f, 0.625f, 0.640f, 1f);
        public Color textOnAccent = new Color(0.985f, 1f, 1f, 1f);

        [Header("Responsive layout")]
        [Min(4f)] public float compactSpacing = 12f;
        [Min(4f)] public float standardSpacing = 20f;
        [Min(48f)] public float navigationWidth = 224f;
        [Min(48f)] public float compactNavigationWidth = 72f;
        [FormerlySerializedAs("activityWidth")]
        [Min(280f)] public float notesWidth = 360f;
        [Min(1.5f)] public float compactLandscapeAspect = 1.95f;
        [Min(800f)] public float compactLandscapeWidth = 1200f;

        public Color GetStatusColor(LaboratoryDishStatus status)
        {
            switch (status)
            {
                case LaboratoryDishStatus.Growing:
                case LaboratoryDishStatus.Healthy: return green;
                case LaboratoryDishStatus.Stressed: return amber;
                case LaboratoryDishStatus.Severe: return red;
                default: return textSecondary;
            }
        }
    }
}
